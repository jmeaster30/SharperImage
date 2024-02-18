using SharperImage.Enumerators;
using SharperImage.Enumerators.Drawing;
namespace SharperImage.Filters.Artsy;

public enum PixelSortDirection
{
    HORIZONTAL,
    VERTICAL,
}

internal class PixelSortRange
{
    public uint Start { get; set; }
    public uint End { get; set; }
    public uint StableCoord { get; set; }
    public List<Color> Colors { get; set; } = new();

    public List<Pixel> GetSortedRange(PixelSortDirection direction, Comparison<Color> colorComparator)
    {
        var results = new List<Pixel>();
        Colors.Sort(colorComparator);
        for (var i = Start; i < End; i++)
        {
            switch (direction)
            {
                case PixelSortDirection.VERTICAL:
                    results.Add(new Pixel(StableCoord, i, Colors[(int)(i - Start)]));
                    break;
                case PixelSortDirection.HORIZONTAL:
                    if ((int)(i - Start) >= Colors.Count)
                    {
                        Console.WriteLine($"Out of bounds: {direction} {StableCoord} {Start} {End} {i} {Colors.Count}");
                    }
                    results.Add(new Pixel(i, StableCoord, Colors[(int)(i - Start)]));
                    break;
            }
        }
        return results;
    }
}

public static class PixelSortFilter
{
    public static IPixelEnumerable PixelSort(this IPixelEnumerable pixelEnumerable, Func<Color, bool> intervalPredicate, Comparison<Color> pixelSort, PixelSortDirection intervalDirection = PixelSortDirection.HORIZONTAL)
    {
        // TODO I don't like that we go through the pixelEnumerable twice :(
        var thresholdRanges = getThresholdRanges(pixelEnumerable, intervalPredicate, intervalDirection);
        pixelEnumerable.GetEnumerator().Reset();
        return pixelEnumerable.DrawPixels(thresholdRanges.SelectMany(x => x.GetSortedRange(intervalDirection,
            pixelSort)));
    }

    private static List<PixelSortRange> getThresholdRanges(IPixelEnumerable enumerable, Func<Color, bool> intervalPredicate, PixelSortDirection intervalDirection)
    {
        // TODO this needs to be simplified
        var results = new List<PixelSortRange>();

        switch (intervalDirection)
        {
            case PixelSortDirection.VERTICAL:
            {
                for (uint x = 0; x < enumerable.GetWidth(); x++)
                {
                    uint start = 0;
                    var colors = new List<Color>();
                    var inInterval = false;
                    for (uint y = 0; y < enumerable.GetHeight(); y++)
                    {
                        if (inInterval)
                        {
                            if (intervalPredicate(enumerable[x, y].Color))
                            {
                                colors.Add(enumerable[x, y].Color);
                            }
                            else
                            {
                                results.Add(new PixelSortRange { Start = start, End = y, StableCoord = x, Colors = colors });
                                colors = new List<Color>();
                                inInterval = false;
                            }
                        }
                        else if (intervalPredicate(enumerable[x, y].Color))
                        {
                            start = y; 
                            colors.Add(enumerable[x, y].Color);
                            inInterval = true;
                        }
                    }

                    if (inInterval)
                    {
                        colors.Add(enumerable[x, enumerable.GetHeight() - 1].Color);
                        results.Add(new PixelSortRange
                            { Start = start, End = enumerable.GetHeight() - 1, StableCoord = x, Colors = colors });
                    }
                }
                break;
            }
            case PixelSortDirection.HORIZONTAL:
            {
                for (uint y = 0; y < enumerable.GetHeight(); y++)
                {
                    uint start = 0;
                    var colors = new List<Color>();
                    var inInterval = false;
                    for (uint x = 0; x < enumerable.GetWidth(); x++)
                    {
                        if (inInterval)
                        {
                            if (intervalPredicate(enumerable[x, y].Color))
                            {
                                colors.Add(enumerable[x, y].Color);
                            }
                            else
                            {
                                results.Add(new PixelSortRange { Start = start, End = x, StableCoord = y, Colors = colors });
                                colors = new List<Color>();
                                inInterval = false;
                            }
                        }
                        else if (intervalPredicate(enumerable[x, y].Color))
                        {
                            start = x; 
                            colors.Add(enumerable[x, y].Color);
                            inInterval = true;
                        }
                    }

                    if (inInterval)
                    {
                        colors.Add(enumerable[enumerable.GetWidth() - 1, y].Color);
                        results.Add(new PixelSortRange
                            { Start = start, End = enumerable.GetWidth() - 1, StableCoord = y, Colors = colors });
                    }
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(intervalDirection), intervalDirection, null);
        }
        
        return results;
    }
}