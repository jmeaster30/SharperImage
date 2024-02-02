namespace SharperImage.Filters;

public static class GrayscaleFilter
{
    public static Color Grayscale(this Color color)
    {
        var value = (byte) (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
        return new Color
        {
            Red = value,
            Green = value,
            Blue = value,
            Alpha = color.Alpha
        };
    }

    public static Pixel Grayscale(this Pixel pixel)
    {
        return pixel.Process(Grayscale);
    }

    public static Image Grayscale(this Image image)
    {
        return image.Process(Grayscale);
    }

    public static IEnumerable<Pixel> Grayscale(this IEnumerable<Pixel> pixelEnumerable)
    {
        return pixelEnumerable.Process(Grayscale);
    }
}