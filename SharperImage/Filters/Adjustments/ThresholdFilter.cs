using SharperImage.Enumerators;

namespace SharperImage.Filters.Adjustments;

public static class ThresholdFilter
{
    public static IPixelEnumerable Threshold(this IPixelEnumerable pixelEnumerable, double threshold)
    {
        return pixelEnumerable.Threshold(color => color.Luminosity() > threshold);
    }

    public static IPixelEnumerable Threshold(this IPixelEnumerable pixelEnumerable,
        Func<Color, bool> thresholdPredicate)
    {
        return pixelEnumerable.PixelFilter(pixel =>
            new Pixel(pixel.X, pixel.Y, thresholdPredicate(pixel.Color) ? Color.WHITE : Color.BLACK));
    }
    
    // TODO add multiple levels of threshold and the ability to custom set the colors of the threshold
}