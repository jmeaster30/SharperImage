namespace SharperImage.Formats;

public class BitmapImage : IImage
{
    public FileFormat FileFormat()
    {
        return Formats.FileFormat.BMP;
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

    public static BitmapImage LoadImage(Stream stream)
    {
        var bmp = new BitmapImage();
        bmp.Load(stream);
        return bmp;
    }
}