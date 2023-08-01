using System.Net.NetworkInformation;
using SharperImage.Formats;

namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static PixelEnumerable<ColumnMajorPixelEnumerator> ToColumnRankPixelEnumerable(this Image image)
    {
        // FIXME I would prefer if I could just do:
        // return new PixelEnumerable<ColumnMajorPixelEnumerator>(image);
        return new PixelEnumerable<ColumnMajorPixelEnumerator>(image, (i, idx) => new ColumnMajorPixelEnumerator(i, idx));
    }
    
    public static PixelEnumerable<RowMajorPixelEnumerator> ToRowRankPixelEnumerable(this Image image)
    {
        // FIXME I would prefer if I could just do:
        // return new PixelEnumerable<RowMajorPixelEnumerator>(image);
        return new PixelEnumerable<RowMajorPixelEnumerator>(image, (i, idx) => new RowMajorPixelEnumerator(i, idx));
    }

    public static Image ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height)
    {
        return pixelEnumerable.ToImage(width, height);
    }
    
    public static Image ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height, FileFormat format)
    {
        Image image = new Image(width, height);
        
        foreach (var pixel in pixelEnumerable)
        {
            image.SetPixel(pixel.X, pixel.Y, pixel);
        }

        return image;
    }
}