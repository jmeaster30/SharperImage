using SharperImage.Enumerators;

namespace SharperImage.Filters.Blurs;

public static class BoxBlurFilter
{
    public static IPixelEnumerable BoxBlur(this IPixelEnumerable pixelEnumerable, uint blurWidth, uint blurHeight, KernelEdgeMode edgeMode = KernelEdgeMode.EXTEND)
    {
        return new KernelEnumerable(pixelEnumerable, blurWidth, blurHeight, kernel =>
        {
            var sum = kernel.Cast<Pixel>().Aggregate(Color.Clear, (current, pix) => current + pix.Color);
            return sum / kernel.Length;
        }, edgeMode);
    }
}