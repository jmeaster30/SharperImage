using SharperImage.Enumerators;

namespace SharperImage.Filters;

public static class ImageExtensions
{
    public static Pixel Process(this Pixel pixel, Func<Color, Color> colorFilter)
    {
        pixel.Color = colorFilter(pixel.Color);
        return pixel;
    }

    public static Image Process(this Image image, Func<Pixel, Pixel> pixelFilter)
    {
        var final = new Pixel[image.Width, image.Height];
        foreach (var pixel in image.ToPixelEnumerable().Select(pixelFilter))
        {
            final[pixel.X, pixel.Y] = pixel;
        }

        return new Image(image.Width, image.Height, final);
    }

    public static IPixelEnumerable Process(this IPixelEnumerable pixelEnumerable, Func<Pixel, Pixel> pixelFilter)
    {
        return pixelEnumerable.PixelFilter(pixelFilter);
    }
}