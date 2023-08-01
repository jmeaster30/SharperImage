namespace SharperImage.Filters;

public static class InvertFilter
{
    public static Color Invert(this Color color)
    {
        return new Color
        {
            Red = (byte)(255 - color.Red),
            Green = (byte)(255 - color.Green),
            Blue = (byte)(255 - color.Blue),
            Alpha = (byte)(255 - color.Alpha)
        };
    }

    public static Pixel Invert(this Pixel pixel)
    {
        return pixel.Process(Invert);
    }

    public static IImage Invert(this IImage image)
    {
        return image.Process(Invert);
    }
}