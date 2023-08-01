using System.Text;
using Newtonsoft.Json;
using SharperImage.Exceptions;
using SharperImage.Formats.Interfaces;

namespace SharperImage.Formats;

public class PngImage : IFormat
{
    public Image Decode(Stream stream)
    {
        var signature = stream.ReadBytes(8);
        if (!signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            throw new ImageDecodeException("Unexpected PNG file signature");

        var chunks = new List<Chunk>();
        ImageHeader? imageHeader = null;
        while (stream.Position != stream.Length)
        {
            chunks.Add(Chunk.Load(stream, imageHeader));
            imageHeader = (ImageHeader?)chunks.SingleOrDefault(x => x.Is<ImageHeader>());
        }

        if (imageHeader == null)
        {
            throw new ImageDecodeException("PNG image missing the critical IHDR chunk.");
        }

        var allowedBitDepths = imageHeader.ColorType switch
        {
            0 => new byte[] { 1, 2, 4, 6, 8, 16 },
            2 => new byte[] { 8, 16 },
            3 => new byte[] { 1, 2, 4, 8 },
            4 => new byte[] { 8, 16 },
            6 => new byte[] { 8, 16 },
            _ => throw new ImageDecodeException("Unexpected color type :("),
        };

        if (!allowedBitDepths.Contains(imageHeader.BitDepth))
        {
            throw new ImageDecodeException("Unexpected bit depth :(");
        }

        if (imageHeader.CompressionMethod != 0)
        {
            throw new ImageDecodeException("Unexpected compression method :(");
        }

        switch (imageHeader.ColorType)
        {
            case 3 when !chunks.Any(x => x.Is<Palette>()):
                throw new ImageDecodeException("Expected a palette chunk with the color type 3");
            case 0 or 4 when chunks.Any(x => x.Is<Palette>()):
                throw new ImageDecodeException(
                    "Expected no palette chunks is the color type 0 or 4 but found a palette chunk");
        }

        var width = imageHeader.Width;
        var height = imageHeader.Height;

        Console.WriteLine(JsonConvert.SerializeObject(imageHeader));
        Console.WriteLine(JsonConvert.SerializeObject(chunks));
        return new Image(width, height);
    }
    
    private abstract class Chunk
    {
        public uint Length { get; set; }
        public string Type { get; set; } = "";
        public byte[] Data { get; set; } = { };
        public uint Crc { get; set; }

        public abstract bool Is<T>();

        public static Chunk Load(Stream stream, ImageHeader? imageHeader)
        {
            var chunkLength = stream.ReadU32();
            var type = Encoding.UTF8.GetString(stream.ReadBytes(4));
            Console.WriteLine(type);
            var data = stream.ReadBytes(chunkLength);
            var crc = stream.ReadU32();
            return type switch
            {
                "IHDR" => new ImageHeader
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc,
                    Width = data[..4].ToU32(),
                    Height = data[4..8].ToU32(),
                    BitDepth = data[8],
                    ColorType = data[9],
                    CompressionMethod = data[10],
                    FilterMethod = data[11],
                    InterlaceMethod = data[12],
                },
                "PLTE" => Palette.Load(chunkLength, type, data, crc),
                "gAMA" => new Gamma
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc,
                    Value = data[..4].ToU32(),
                },
                "sBIT" => SignificantBits.Load(chunkLength, type, data, crc, imageHeader),
                _ => new Unknown
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc,
                }
            };
        }
    }

    private class ImageHeader : Chunk
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public byte BitDepth { get; set; }
        public byte ColorType { get; set; }
        public byte CompressionMethod { get; set; }
        public byte FilterMethod { get; set; }
        public byte InterlaceMethod { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(ImageHeader);
        }
    }

    private class Palette : Chunk
    {
        public List<(byte, byte, byte)> PaletteTable { get; set; } = new();

        public override bool Is<T>()
        {
            return typeof(T) == typeof(Palette);
        }
        
        public static Palette Load(uint length, string type, byte[] data, uint crc)
        {
            if (length % 3 != 0)
            {
                throw new ImageDecodeException("Pallette table has length not divisible by 3");
            }
            
            return new Palette
            {
                Length = length,
                Type = type,
                Data = data,
                Crc = crc,
                PaletteTable = data.Chunk(3).Select(group => (group[0], group[1], group[2])).ToList(),
            };
        }
    }

    private class Gamma : Chunk
    {
        // Scaled by 100,000
        public uint Value { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(Gamma);
        }
    }

    private class SignificantBits : Chunk
    {
        public byte RedScale { get; set; }
        public byte GreenScale { get; set; }
        public byte BlueScale { get; set; }
        public byte AlphaScale { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(SignificantBits);
        }

        public static SignificantBits Load(uint length, string type, byte[] data, uint crc, ImageHeader? header)
        {
            if (header == null)
            {
                throw new ImageDecodeException(
                    "Found an sBIT chunk without having an IHDR chunk. The structure of the sBIT chunk is defined with the color type of the IHDR chunk");
            }

            return header.ColorType switch
            {
                0 =>
                    new SignificantBits
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        RedScale = data[0],
                        GreenScale = data[0],
                        BlueScale = data[0],
                        AlphaScale = data[0],
                    },
                2 or 3 => new SignificantBits
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        RedScale = data[0],
                        GreenScale = data[1],
                        BlueScale = data[2],
                        AlphaScale = 0,
                    },
                4 => new SignificantBits
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        RedScale = data[0],
                        GreenScale = data[0],
                        BlueScale = data[0],
                        AlphaScale = data[1],
                    },
                6 => new SignificantBits
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        RedScale = data[0],
                        GreenScale = data[1],
                        BlueScale = data[2],
                        AlphaScale = data[3],
                    },
                _ => throw new ImageDecodeException("Unexpected color type for the sBIT header")
            };
        }
    }

    private class ImageData : Chunk
    {
        public override bool Is<T>()
        {
            return typeof(T) == typeof(ImageData);
        }
    }

    private class Unknown : Chunk
    {
        public override bool Is<T>()
        {
            return typeof(T) == typeof(Unknown);
        }
    }
    
    public void Encode(Image image, Stream stream)
    {
        throw new NotImplementedException();
    }
    
    
}

