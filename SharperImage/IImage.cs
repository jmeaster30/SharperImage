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

    public static IImage Decode(string imagePath, FileFormat format)
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
