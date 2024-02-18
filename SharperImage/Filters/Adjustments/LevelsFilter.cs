using SharperImage.Enumerators;

namespace SharperImage.Filters.Adjustments;

public static class LevelsFilter
{
    public static IPixelEnumerable Levels(this IPixelEnumerable enumerable, Func<double, double> redMapFunction, Func<double, double> greenMapFunction,
        Func<double, double> blueMapFunction)
    {
        return enumerable.Levels(redMapFunction, greenMapFunction, blueMapFunction, x => x);
    }
    
    public static IPixelEnumerable Levels(this IPixelEnumerable enumerable, Func<double, double> redMapFunction, Func<double, double> greenMapFunction,
        Func<double, double> blueMapFunction, Func<double, double> alphaMapFunction)
    {
        return enumerable.PixelFilter(pixel => new Pixel(pixel.X, pixel.Y,
            new Color(redMapFunction(pixel.Color.Red), 
                greenMapFunction(pixel.Color.Green),
                blueMapFunction(pixel.Color.Blue), 
                alphaMapFunction(pixel.Color.Alpha))));
    }
}