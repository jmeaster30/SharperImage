using SharperImage.Formats;

namespace SharperImage;

public interface IImage
{
    FileFormat FileFormat();
    uint Width();
    uint Height();
    Pixel[,] PixelArray();
    void Load(Stream filename);
    void Save(Stream filename);

    public static IImage Load(string imagePath, FileFormat format)
    {
        using var imageFile = File.OpenRead(imagePath);
        return format switch
        {
            Formats.FileFormat.BMP => BitmapImage.LoadImage(imageFile),
            Formats.FileFormat.QOI => QoiImage.LoadImage(imageFile),
            Formats.FileFormat.GIF => GifImage.LoadImage(imageFile),
            _ => throw new NotImplementedException("Format not currently supported :(")
        };
    }
}