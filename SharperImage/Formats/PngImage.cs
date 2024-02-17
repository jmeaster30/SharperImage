using System.Text;
using MyLib.Compression;
using MyLib.Enumerables;
using MyLib.Streams;
using Newtonsoft.Json;
using SharperImage.Exceptions;
using SharperImage.Formats.Interfaces;

namespace SharperImage.Formats;

public class PngImage : IFormat
{
    private static bool IsCritical(string type)
    {
        return char.IsUpper(type[0]);
    }
    
    private static bool IsPublic(string type)
    {
        return char.IsUpper(type[1]);
    }
    
    private static bool IsReserved(string type)
    {
        return char.IsUpper(type[0]);
    }
    
    private static bool IsSafeToCopy(string type)
    {
        return char.IsLower(type[0]);
    }
    
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

        var data = chunks.Where(x => x.Is<ImageData>()).SelectMany(x => ((ImageData)x).Data);
        var uncompressedData = new Zlib().Decode(data).ToList();

        foreach (var x in uncompressedData.GetRange(0, 4))
        {
            Console.WriteLine(x);
        }
        
        return new Image(width, height);
    }
    
    private abstract class Chunk
    {
        public uint Length { get; set; }
        public string Type { get; set; } = "";
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public uint Crc { get; set; }

        public abstract bool Is<T>();

        public static Chunk Load(Stream stream, ImageHeader? imageHeader)
        {
            var chunkLength = stream.ReadU32();
            var type = Encoding.UTF8.GetString(stream.ReadBytes(4));
            var data = stream.ReadBytes((int)chunkLength);
            var crc = stream.ReadU32();
            
            // TODO check CRC
            
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
                "cHRM" => Chrm.Load(chunkLength, type, data, crc),
                "bKGD" => BackgroundColor.Load(chunkLength, type, data, crc, imageHeader),
                "pHYs" => new PhysicalPixelDimensions
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc,
                    PixelsPerUnitX = data[..4].ToU32(),
                    PixelsPerUnitY = data[4..8].ToU32(),
                    UnitSpecifier = data[8]
                },
                "tIME" => new ImageLastModificationTime
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc,
                    Year = data[..2].ToU16(),
                    Month = data[2],
                    Day = data[3],
                    Hour = data[4],
                    Minute = data[5],
                    Second = data[6]
                },
                "IDAT" => new ImageData 
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc,
                },
                "tEXt" => TextualData.Load(chunkLength, type, data, crc),
                "IEND" => new End
                {
                    Length = chunkLength,
                    Type = type,
                    Data = data,
                    Crc = crc
                },
                _ => Unknown.Load(chunkLength, type, data, crc),
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

    private class Chrm : Chunk
    {
        // all of these are scaled by 100,000
        public uint WhitePointX { get; set; }
        public uint WhitePointY { get; set; }
        public uint RedX { get; set; }
        public uint RedY { get; set; }
        public uint GreenX { get; set; }
        public uint GreenY { get; set; }
        public uint BlueX { get; set; }
        public uint BlueY { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(Chrm);
        }

        public static Chrm Load(uint length, string type, byte[] data, uint crc)
        {
            if (length != 32)
                throw new ImageDecodeException("Data length for cHRM chunk was not 32 bytes");
            
            return new Chrm
            {
                Length = length,
                Type = type,
                Data = data,
                Crc = crc,
                WhitePointX = data[..4].ToU32(),
                WhitePointY = data[4..8].ToU32(),
                RedX = data[8..12].ToU32(),
                RedY = data[12..16].ToU32(),
                GreenX = data[16..20].ToU32(),
                GreenY = data[20..24].ToU32(),
                BlueX = data[24..28].ToU32(),
                BlueY = data[28..32].ToU32(),
            };
        }
    }

    private class BackgroundColor : Chunk
    {
        public byte PaletteIndex { get; set; }
        public ushort Gray { get; set; }
        public ushort Red { get; set; }
        public ushort Green { get; set; }
        public ushort Blue { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(BackgroundColor);
        }
        
        public static BackgroundColor Load(uint length, string type, byte[] data, uint crc, ImageHeader? header)
        {
            if (header == null)
            {
                throw new ImageDecodeException(
                    "Found a bKGD chunk without having an IHDR chunk. The structure of the bKGD chunk is defined with the color type of the IHDR chunk");
            }

            return header.ColorType switch
            {
                3 =>
                    new BackgroundColor
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        PaletteIndex = data[0]
                    },
                0 or 4 => new BackgroundColor
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        Gray = data[..2].ToU16(),
                    },
                2 or 6 => new BackgroundColor
                    {
                        Length = length,
                        Type = type,
                        Data = data,
                        Crc = crc,
                        Red = data[..2].ToU16(),
                        Green = data[2..4].ToU16(),
                        Blue = data[4..6].ToU16()
                    },
                _ => throw new ImageDecodeException("Unexpected color type for the bKGD header")
            };
        }
    }

    private class PhysicalPixelDimensions : Chunk
    {
        public uint PixelsPerUnitX { get; set; }
        public uint PixelsPerUnitY { get; set; }
        public byte UnitSpecifier { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(PhysicalPixelDimensions);
        }
    }

    private class ImageLastModificationTime : Chunk
    {
        public ushort Year { get; set; }
        public byte Month { get; set; }
        public byte Day { get; set; }
        public byte Hour { get; set; }
        public byte Minute { get; set; }
        public byte Second { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(ImageLastModificationTime);
        }
    }

    private class TextualData : Chunk
    {
        public string Keyword { get; set; }
        public string Text { get; set; }

        public override bool Is<T>()
        {
            return typeof(T) == typeof(TextualData);
        }

        public static TextualData Load(uint length, string type, byte[] data, uint crc)
        {
            var textualData = new TextualData
            {
                Length = length,
                Type = type,
                Data = data,
                Crc = crc
            };

            var inKeyword = true;
            foreach (var d in data)
            {
                if (inKeyword && d == 0)
                    inKeyword = false;
                else if (inKeyword)
                    textualData.Keyword += Encoding.UTF8.GetString(new [] { d });
                else
                    textualData.Text += Encoding.UTF8.GetString(new [] { d });
            }

            return textualData;
        }
    }

    private class End : Chunk
    {
        public override bool Is<T>()
        {
            return typeof(T) == typeof(End);
        }
    }

    private class Unknown : Chunk
    {
        public override bool Is<T>()
        {
            return typeof(T) == typeof(Unknown);
        }

        public static Unknown Load(uint length, string type, byte[] data, uint crc)
        {
            if (IsCritical(type))
                throw new ImageDecodeException("Cannot safely interpret file. Found critical chunk with unknown type.");
            
            return new Unknown
            {
                Length = length,
                Type = type,
                Data = data,
                Crc = crc
            };
        }
    }
    
    public void Encode(Image image, Stream stream)
    {
        throw new NotImplementedException();
    }
}

