using SharperImage.Formats;

namespace SharperImage;

public interface IImage
{
    FileFormat FileFormat();
    uint Width();
    uint Height();
    Pixel[,] PixelArray();
    Pixel GetPixel(uint x, uint y);
    void SetPixel(uint x, uint y, Pixel pixel);
    void Decode(Stream stream);
    void Encode(Stream stream);

    public IImage Reformat(FileFormat format)
    {
        return format switch
        {
            Formats.FileFormat.BMP => new BitmapImage(Width(), Height(), PixelArray()),
            Formats.FileFormat.QOI => new QoiImage(Width(), Height(), PixelArray()),
            Formats.FileFormat.GIF => new GifImage(Width(), Height(), PixelArray()),
            Formats.FileFormat.PNG => new PngImage(Width(), Height(), PixelArray()),
            _ => throw new NotImplementedException("Format not currently supported :(")
        };
    }
    
    public static IImage Decode(string imagePath, FileFormat format)
    {
        using var imageFile = File.OpenRead(imagePath);
        return format switch
        {
            Formats.FileFormat.BMP => BitmapImage.LoadImage(imageFile),
            Formats.FileFormat.QOI => QoiImage.LoadImage(imageFile),
            Formats.FileFormat.GIF => GifImage.LoadImage(imageFile),
            Formats.FileFormat.PNG => PngImage.LoadImage(imageFile),
            _ => throw new NotImplementedException("Format not currently supported :(")
        };
    }
    
    public static IImage Create(FileFormat format, uint width, uint height, Pixel[,] pixels)
    {
        return format switch
        {
            Formats.FileFormat.BMP => new BitmapImage(width, height, pixels),
            Formats.FileFormat.QOI => new QoiImage(width, height, pixels),
            Formats.FileFormat.GIF => new GifImage(width, height, pixels),
            Formats.FileFormat.PNG => new PngImage(width, height, pixels),
            _ => throw new NotImplementedException("Format not currently supported :(")
        };
    }

    
}
