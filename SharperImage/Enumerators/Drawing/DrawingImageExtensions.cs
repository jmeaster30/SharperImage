namespace SharperImage.Enumerators.Drawing;

using MyLib.Math;

public static class DrawingImageExtensions
{
    public static IPixelEnumerable DrawPixel(this IPixelEnumerable enumerable, Pixel pixel)
    {
        return new DrawPixelEnumerable(enumerable, pixel);
    }

    public static IPixelEnumerable DrawPixel(this IPixelEnumerable enumerable, uint x, uint y, Color color)
    {
        return new DrawPixelEnumerable(enumerable, new Pixel(x, y, color));
    }

    public static IPixelEnumerable DrawRectangle(this IPixelEnumerable enumerable, uint x, uint y, uint width,
        uint height, Color color)
    {
        return new DrawRectangleEnumerable(enumerable, x, y, width, height, color);
    }

    public static IPixelEnumerable DrawLine(this IPixelEnumerable enumerable, int x1, int y1, int x2, int y2,
        Color color, double tolerance = 1.0)
    {
        return new DrawLineEnumerable(enumerable, x1, y1, x2, y2, color, tolerance);
    }

    public static IPixelEnumerable DrawLine(this IPixelEnumerable enumerable, int x1, int y1, uint length, double radians,
        Color color, double tolerance)
    {
        var xoff = System.Math.Cos(radians) * length;
        var yoff = System.Math.Sin(radians) * length;
        Console.WriteLine($"{xoff} {yoff}");
        return new DrawLineEnumerable(enumerable, x1, y1, (x1 + xoff).Round(), (y1 + yoff).Round(), color, tolerance);
    }
}