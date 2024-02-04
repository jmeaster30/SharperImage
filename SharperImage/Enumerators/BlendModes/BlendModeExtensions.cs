namespace SharperImage.Enumerators.BlendModes;

public static class BlendModeExtensions
{
    public static IPixelEnumerable Blend(this IPixelEnumerable a, IPixelEnumerable b, BlendMode mode)
    {
        return new BlendModeEnumerable(a, b, mode);
    }

    public static IPixelEnumerable Blend(this (IPixelEnumerable, IPixelEnumerable) a, BlendMode mode)
    {
        return new BlendModeEnumerable(a.Item1, a.Item2, mode);
    }
}