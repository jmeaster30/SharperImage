using SharperImage.Enumerators;

namespace SharperImage.Filters;

public static class ImageExtensions
{
    public static Pixel Process(this Pixel pixel, Func<Color, Color> colorFilter)
    {
        pixel.Color = colorFilter(pixel.Color);
        return pixel;
    }

    public static IImage Process(this IImage image, Func<Pixel, Pixel> pixelFilter)
    {
        var final = new Pixel[image.Width(), image.Height()];
        foreach (var pixel in image.ToRowRankPixelEnumerable().Select(pixelFilter))
        {
            final[pixel.X, pixel.Y] = pixel;
        }

        return IImage.Create(image.FileFormat(), image.Width(), image.Height(), final);
    }
}