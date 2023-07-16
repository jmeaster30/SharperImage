using System.Net;
using SharperImage.Enumerators;

namespace SharperImage.Formats;

public class UnformattedImage : IImage
{
    private uint _width { get; set; }
    private uint _height { get; set; }
    private Pixel[,] _pixelData { get; set; }

    public FileFormat FileFormat() => Formats.FileFormat.UNFORMATED;
    public uint Width() => _width;
    public uint Height() => _height;
    public Pixel[,] PixelArray() => _pixelData;
    public Pixel GetPixel(uint x, uint y) => _pixelData[x, y];
    public void SetPixel(uint x, uint y, Pixel pixel) => _pixelData[x, y] = pixel;

    public UnformattedImage(uint width, uint height)
    {
        _width = width;
        _height = height;
        _pixelData = new Pixel[width, height];
    }

    public void Decode(Stream stream)
    {
        _width = stream.ReadU32();
        _height = stream.ReadU32();
        // TODO will be compression method of the remainder of the bytes with 0 being raw RGBA bytes
        var compMethod = (byte)stream.ReadByte();
        
        // only uncompressed is supported now
        uint index = 0;
        while (stream.Position != stream.Length)
        {
            var x = index % _width;
            var y = index / _width;
            var pixel = new Pixel
            {
                Red = (byte) stream.ReadByte(),
                Green = (byte) stream.ReadByte(),
                Blue = (byte) stream.ReadByte(),
                Alpha = (byte) stream.ReadByte(),
                X = x,
                Y = y
            };
            _pixelData[x, y] = pixel;
            index += 1;
        }
    }

    public void Encode(Stream stream)
    {
        stream.WriteU32(_width);
        stream.WriteU32(_height);
        stream.WriteByte(0); // TODO add compression methods

        var pixels = this.ToRowRankPixelEnumerable();
        foreach (var pix in pixels)
        {
            stream.WriteByte(pix.Red);
            stream.WriteByte(pix.Green);
            stream.WriteByte(pix.Blue);
            stream.WriteByte(pix.Alpha);
        }
    }
}