using MyLib.Math;
using SharperImage.Enumerators;

namespace SharperImage.Filters.Smoothing;

public static class KuwaharaFilter
{
    private static (Color, double) MeanStandardDeviation(List<Color> colors)
    {
        var colormean = colors.Aggregate(Color.CLEAR, (current, c) => current + c / colors.Count);
        
        var lumas = colors.Select(x => x.Grayscale().Red);
        var lumamean = lumas.Sum() / colors.Count;
        var standardDeviation = System.Math.Sqrt(lumas.Aggregate(0.0, (current, x) => current + (x - lumamean) * (x - lumamean)) / lumas.Count());

        return (colormean, standardDeviation);
    }

    private static List<List<Color>> Quadrants(Pixel[,] data, uint windowSize)
    {
        // TODO this feels crappy. I definitely want to add a GetRange type extension but for 2d arrays
        var results = new List<List<Color>>();

        var quadrantSize = (windowSize / 2.0).Ceiling();
        
        var q1 = new List<Color>();
        for (var x = 0; x < quadrantSize; x++)
        {
            for (var y = 0; y < quadrantSize; y++)
            {
                q1.Add(data[x, y].Color);
            }
        }
        results.Add(q1);
        
        var q2 = new List<Color>();
        for (var x = quadrantSize - 1; x < windowSize; x++)
        {
            for (var y = 0; y < quadrantSize; y++)
            {
                q2.Add(data[x, y].Color);
            }
        }
        results.Add(q2);
        
        var q3 = new List<Color>();
        for (var x = 0; x < quadrantSize; x++)
        {
            for (var y = quadrantSize - 1; y < windowSize; y++)
            {
                q3.Add(data[x, y].Color);
            }
        }
        results.Add(q3);
        
        var q4 = new List<Color>();
        for (var x = quadrantSize - 1; x < windowSize; x++)
        {
            for (var y = quadrantSize - 1; y < windowSize; y++)
            {
                q4.Add(data[x, y].Color);
            }
        }
        results.Add(q4);
        
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