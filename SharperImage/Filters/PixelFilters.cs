namespace SharperImage.Filters;

public static class PixelFilters
{
    public static Pixel Grayscale(this Pixel pixel)
    {
        var value = (byte) (0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue);
        return new Pixel
        {
            X = pixel.X,
            Y = pixel.Y,
            Red = value,
            Green = value,
            Blue = value,
            Alpha = pixel.Alpha
        };
    }
}