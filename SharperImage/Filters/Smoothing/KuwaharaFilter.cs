using MyLib.Math;
using SharperImage.Enumerators;

namespace SharperImage.Filters.Smoothing;

public static class KuwaharaFilter
{
    private static (Color, double) MeanStandardDeviation(List<Color> colors)
    {
        var colormean = colors.Aggregate(Color.CLEAR, (current, c) => current + c / colors.Count);
        
        var lumas = colors.Select(x => x.Luma());
        var lumamean = lumas.Sum() / colors.Count;
        var standardDeviation = System.Math.Sqrt(lumas.Aggregate(0.0, (current, x) => current + (x - lumamean) * (x - lumamean)) / lumas.Count());

        return (colormean, standardDeviation);
    }

    private static List<Color> Quadrant(Pixel[,] data, uint fromX, uint toX, uint fromY, uint toY)
    {
        // TODO it would be cool to have a GetRange type function for 2d arrays
        var result = new List<Color>();
        for (var x = fromX; x < toX; x++)
        {
            for (var y = fromY; y < toY; y++)
            {
                result.Add(data[x, y].Color);
            }
        }

        return result;
    }

    private static List<List<Color>> Quadrants(Pixel[,] data, uint windowSize)
    {
        var results = new List<List<Color>>();

        var quadrantSize = (uint)(windowSize / 2.0).Ceiling();
        
        results.Add(Quadrant(data, 0, quadrantSize, 0, quadrantSize));
        results.Add(Quadrant(data, quadrantSize - 1, windowSize, 0, quadrantSize));
        results.Add(Quadrant(data, 0, quadrantSize, quadrantSize - 1, windowSize));
        results.Add(Quadrant(data, quadrantSize - 1, windowSize, quadrantSize - 1, windowSize));

        return results;
    } 
    
    public static IPixelEnumerable Kuwahara(this IPixelEnumerable enumerable, uint windowSize)
    {
        return enumerable.Kernel(windowSize, windowSize, kernel =>
        {
            return Quadrants(kernel, windowSize).Select(MeanStandardDeviation).MinBy(x => x.Item2).Item1;
        });
    }
}