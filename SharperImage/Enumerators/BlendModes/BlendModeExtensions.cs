namespace SharperImage.Enumerators.BlendModes;

public static class BlendModeExtensions
{
    public static IPixelEnumerable Blend(this IPixelEnumerable a, IPixelEnumerable b, BlendMode mode)
    {
        return new BlendModeEnumerable(a, b, mode);
    }
}