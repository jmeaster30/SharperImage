namespace SharperImage.Formats;

public class GifImage : IImage
{
    public FileFormat FileFormat()
    {
        return Formats.FileFormat.GIF;
    }

    public uint Height()
    {
        throw new NotImplementedException();
    }

    public uint Width()
    {
        throw new NotImplementedException();
    }

    public Pixel[,] PixelArray()
    {
        throw new NotImplementedException();
    }

    public void Load(Stream stream)
    {
        throw new NotImplementedException();
    }

    public void Save(Stream stream)
    {
        throw new NotImplementedException();
    }

    public static GifImage LoadImage(Stream stream)
    {
        var gif = new GifImage();
        gif.Load(stream);
        return gif;
    }
}