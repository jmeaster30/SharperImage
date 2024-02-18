using SharperImage.Enumerators;

namespace SharperImage.Filters.Adjustments;

public static class InvertFilter
{
    public static Pixel Invert(this Pixel pixel)
    {
        return pixel.Process(c => c.Invert());
    }

    public static Image Invert(this Image image)
    {
        return image.Process(Invert);
    }
    
    public static IPixelEnumerable Invert(this IPixelEnumerable pixelEnumerable)
    {
        return pixelEnumerable.Process(Invert);
    }
}