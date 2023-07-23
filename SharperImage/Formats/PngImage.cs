using System.Text;
using Newtonsoft.Json;
using SharperImage.Exceptions;

namespace SharperImage.Formats;

public class PngImage : IImage
{
    private uint _width;
    private uint _height;
    private Pixel[,] _pixelData;

    public FileFormat FileFormat() => Formats.FileFormat.PNG;

    public uint Width() => _width;
    public uint Height() => _height;

    public Pixel[,] PixelArray() => _pixelData;
    public Pixel GetPixel(uint x, uint y) => _pixelData[x, y];
    public void SetPixel(uint x, uint y, Pixel pixel) => _pixelData[x, y] = pixel;

    public static PngImage LoadImage(Stream stream)
    {
        var png = new PngImage();
        png.Decode(stream);
        return png;
    }

    public void Decode(Stream stream)
    {
        var signature = stream.ReadBytes(8);
        if (!signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            throw new ImageDecodeException("Unexpected PNG file signature");

        ImageHeader? imageHeader = null;
        var chunks = new List<Chunk>();
        while (stream.Position != stream.Length)
        {
            var chunk = Chunk.Load(stream);
            switch (chunk.Type)
            {
                case "IHDR":
                    imageHeader = chunk.ToImageHeader();
                    break;
                default: 
                    Console.WriteLine($"Chunk {chunk.Type}");
                    break;
                    //throw new ArgumentException("Unexpected chunk type", nameof(chunk));
            }
            chunks.Add(chunk);
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
        
        _width = imageHeader.Width;
        _height = imageHeader.Height;

        Console.WriteLine(JsonConvert.SerializeObject(imageHeader));
        Console.WriteLine(JsonConvert.SerializeObject(chunks));
    }
    
    private class Chunk
    {
        public uint Length { get; set; }
        public string Type { get; set; } = "";
        public byte[] Data { get; set; } = { };
        public uint Crc { get; set; }

        public static Chunk Load(Stream stream)
        {
            var chunkLength = stream.ReadU32();
            return new Chunk
            {
                Length = chunkLength,
                Type = Encoding.UTF8.GetString(stream.ReadBytes(4)),
                Data = stream.ReadBytes(chunkLength),
                Crc = stream.ReadU32(),
            };
        }

        public ImageHeader ToImageHeader()
        {
            return new ImageHeader
            {
                Width = Data[..4].ToU32(),
                Height = Data[4..8].ToU32(),
                BitDepth = Data[8],
                ColorType = Data[9],
                CompressionMethod = Data[10],
                FilterMethod = Data[11],
                InterlaceMethod = Data[12],
            };
        }
    }

    private class ImageHeader
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public byte BitDepth { get; set; }
        public byte ColorType { get; set; }
        public byte CompressionMethod { get; set; }
        public byte FilterMethod { get; set; }
        public byte InterlaceMethod { get; set; }
    }

    public void Encode(Stream stream)
    {
        throw new NotImplementedException();
    }
    
    
}

