using SharperImage.Formats.Interfaces;

namespace SharperImage.Formats;

public enum FileFormat
{
    BMP,
    PNG,
    JPEG,
    QOI,
    GIF,
    UNFORMATTED
}

public static class FileFormatExtensions
{
    public static IFormat GetFormatter(this FileFormat format)
    {
        return format switch
        {
            FileFormat.BMP => new BitmapImage(),
            FileFormat.PNG => new PngImage(),
            FileFormat.QOI => new QoiImage(),
            FileFormat.GIF => new GifImage(),
            FileFormat.UNFORMATTED => new UnformattedImage(),
            _ => throw new NotImplementedException("This format is not supported :("),
        };
    }
}