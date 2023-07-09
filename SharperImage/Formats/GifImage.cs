namespace SharperImage.Formats;

public class GifImage : IImage
{
    private uint _width;
    private uint _height;
    private Pixel[,] _pixelData;

    public FileFormat FileFormat() => Formats.FileFormat.GIF;
    public uint Height() => _height;
    public uint Width() => _width;
    public Pixel[,] PixelArray() => _pixelData;
    public Pixel GetPixel(uint x, uint y) => _pixelData[x, y];

    public void Decode(Stream stream)
    {
        throw new NotImplementedException();
    }

    public void Encode(Stream stream)
    {
        throw new NotImplementedException();
    }

    public static GifImage LoadImage(Stream stream)
    {
        var gif = new GifImage();
        gif.Decode(stream);
        return gif;
    }
}