namespace SharperImage.Enumerators.Drawing;

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

    public static IPixelEnumerable DrawLine(this IPixelEnumerable enumerable, uint x1, uint y1, uint x2, uint y2,
        Color color, double tolerance = 1.0)
    {
        return new DrawLineEnumerable(enumerable, x1, y1, x2, y2, color, tolerance);
    }
}