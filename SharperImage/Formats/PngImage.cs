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
        Console.WriteLine(Convert.ToHexString(signature));
        if (!signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            throw new ImageDecodeException("Unexpected PNG file signature");

        var chunks = new List<Chunk>();
        while (stream.Position != stream.Length)
        {
            chunks.Add(Chunk.Load(stream));
        }

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
    }

    public void Encode(Stream stream)
    {
        throw new NotImplementedException();
    }
    
    
}

