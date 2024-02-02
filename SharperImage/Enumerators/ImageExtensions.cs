using System.Net.NetworkInformation;
using SharperImage.Formats;

namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static PixelEnumerable ToPixelEnumerable(this Image image, PixelOrdering ordering = PixelOrdering.ROW)
    {
        return new PixelEnumerable(image, ordering);
    }

    public static IEnumerable<Pixel> PixelFilter(this IEnumerable<Pixel> enumerable, Func<Pixel, Pixel> pixelFilter)
    {
        return new PixelFilterEnumerable(enumerable, pixelFilter);
    }

    public static Image ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height)
    {
        return pixelEnumerable.ToImage(width, height);
    }
    
    public static Image ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height, FileFormat format)
    {
        var image = new Image(width, height);
        
        foreach (var pixel in pixelEnumerable)
        {
            image.SetPixel(pixel.X, pixel.Y, pixel);
        }

        return image;
    }
}