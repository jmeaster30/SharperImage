using System.Net.NetworkInformation;
using SharperImage.Formats;

namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static PixelEnumerable<ColumnMajorPixelEnumerator> ToColumnRankPixelEnumerable(this IImage image)
    {
        // FIXME I would prefer if I could just do:
        // return new PixelEnumerable<ColumnMajorPixelEnumerator>(image);
        return new PixelEnumerable<ColumnMajorPixelEnumerator>(image, (i, idx) => new ColumnMajorPixelEnumerator(i, idx));
    }
    
    public static PixelEnumerable<RowMajorPixelEnumerator> ToRowRankPixelEnumerable(this IImage image)
    {
        // FIXME I would prefer if I could just do:
        // return new PixelEnumerable<RowMajorPixelEnumerator>(image);
        return new PixelEnumerable<RowMajorPixelEnumerator>(image, (i, idx) => new RowMajorPixelEnumerator(i, idx));
    }

    public static IImage ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height)
    {
        return pixelEnumerable.ToImage(width, height, FileFormat.UNFORMATED);
    }
    
    public static IImage ToImage(this IEnumerable<Pixel> pixelEnumerable, uint width, uint height, FileFormat format)
    {
        IImage image = format switch
        {
            FileFormat.UNFORMATED => new UnformattedImage(width, height),
            FileFormat.BMP => new BitmapImage(width, height),
            FileFormat.GIF => new GifImage(width, height),
            FileFormat.QOI => new QoiImage(width, height),
            _ => throw new NotImplementedException("This file format is currently unsupported :(")
        };
        
        foreach (var pixel in pixelEnumerable)
        {
            image.SetPixel(pixel.X, pixel.Y, pixel);
        }

        return image;
    }
}