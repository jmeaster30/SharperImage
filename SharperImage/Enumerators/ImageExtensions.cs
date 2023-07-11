namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static PixelEnumerable<ColumnMajorPixelEnumerator> ToColumnRankPixelEnumerable(this IImage image)
    {
        return new PixelEnumerable<ColumnMajorPixelEnumerator>(image, (i, idx) => new ColumnMajorPixelEnumerator(i, idx));
    }
    
    public static PixelEnumerable<RowMajorPixelEnumerator> ToRowRankPixelEnumerable(this IImage image)
    {
        return new PixelEnumerable<RowMajorPixelEnumerator>(image, (i, idx) => new RowMajorPixelEnumerator(i, idx));
    }
}