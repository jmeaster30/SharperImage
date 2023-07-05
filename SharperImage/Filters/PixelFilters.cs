namespace SharperImage.Filters;

public static class PixelFilters
{
    public static Pixel Grayscale(this Pixel pixel)
    {
        var value = (byte) (0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
        return new Pixel
        {
            X = pixel.X,
            Y = pixel.Y,
            R = value,
            G = value,
            B = value,
            A = pixel.A
        };
    }
}