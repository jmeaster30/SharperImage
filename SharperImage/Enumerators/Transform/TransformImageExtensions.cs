namespace SharperImage.Enumerators.Transform;

public static class TransformImageExtensions
{
    public static IPixelEnumerable CropAbsolute(this IPixelEnumerable enumerable, uint newWidth, uint newHeight)
    {
        return new CropEnumerable(enumerable, newWidth, newHeight);
    }
    
    public static IPixelEnumerable CropRelative(this IPixelEnumerable enumerable, int newRelativeWidth, int newRelativeHeight)
    {
        return new CropEnumerable(enumerable, (uint)(enumerable.GetWidth() + newRelativeWidth), (uint)(enumerable.GetHeight() + newRelativeHeight));
    }

    public static IPixelEnumerable CropPercent(this IPixelEnumerable enumerable, double widthPercent,
        double heightPercent)
    {
        return new CropEnumerable(enumerable, (uint)Math.Floor(enumerable.GetWidth() * widthPercent),
            (uint)Math.Floor(enumerable.GetHeight() * heightPercent));
    }

    public static IPixelEnumerable Scale(this IPixelEnumerable enumerable, uint newWidth, uint newHeight, ScaleMode mode = ScaleMode.NEAREST_NEIGHBOR)
    {
        return new ScaleEnumerable(enumerable, newWidth, newHeight, mode);
    }
}