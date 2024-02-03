using SharperImage.Enumerators;

namespace SharperImage.Filters;

public static class GrayscaleFilter
{
    public static Pixel Grayscale(this Pixel pixel)
    {
        return pixel.Process(color => color.Grayscale());
    }

    public static Image Grayscale(this Image image)
    {
        return image.Process(Grayscale);
    }

    public static IPixelEnumerable Grayscale(this IPixelEnumerable pixelEnumerable)
    {
        return pixelEnumerable.Process(Grayscale);
    }
}