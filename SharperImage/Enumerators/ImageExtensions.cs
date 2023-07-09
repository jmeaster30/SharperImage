namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static ColumnMajorPixelEnumerable ToColumnRankPixelEnumerable(this IImage image)
    {
        return new ColumnMajorPixelEnumerable(image);
    }
    
    public static RowMajorPixelEnumerable ToRowRankPixelEnumerable(this IImage image)
    {
        return new RowMajorPixelEnumerable(image);
    }

    public static IEnumerable<Pixel> ToEnumerable(this IImage image)
    {
        return image.ToRowRankPixelEnumerable();
    }
}