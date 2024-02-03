using SharperImage.Formats;

namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static ImageDataEnumerable ToPixelEnumerable(this Image image, PixelOrdering ordering = PixelOrdering.ROW)
    {
        return new ImageDataEnumerable(image, ordering);
    }

    public static IEnumerable<Pixel> PixelFilter(this IEnumerable<Pixel> enumerable, Func<Pixel, Pixel> pixelFilter)
    {
        return new PixelFilterEnumerable(enumerable, pixelFilter);
    }
    
    public static Image ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height)
    {
        var image = new Image(width, height);
        
        foreach (var pixel in pixelEnumerable)
        {
            image.SetPixel(pixel.X, pixel.Y, pixel);
        }

        return image;
    }
}