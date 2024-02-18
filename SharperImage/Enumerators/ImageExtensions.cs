using MyLib.Enumerables;
using MyLib.Math;
using SharperImage.Enumerators.Transform;

namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static ImageDataEnumerable ToPixelEnumerable(this Image image)
    {
        return new ImageDataEnumerable(image);
    }

    public static IPixelEnumerable ConditionalApply(this IPixelEnumerable enumerable, bool condition,
        Func<IPixelEnumerable, IPixelEnumerable> trueEnumerable)
    {
        return enumerable.ConditionalApply(condition, trueEnumerable, pixelEnumerable => pixelEnumerable);
    }
    
    public static IPixelEnumerable ConditionalApply(this IPixelEnumerable enumerable, bool condition,
        Func<IPixelEnumerable, IPixelEnumerable> trueEnumerable, Func<IPixelEnumerable, IPixelEnumerable> falseEnumerable)
    {
        return condition ? trueEnumerable(enumerable) : falseEnumerable(enumerable);
    }

    public static IPixelEnumerable PixelFilter(this IPixelEnumerable enumerable, Func<Pixel, Pixel> pixelFilter)
    {
        return new PixelFilterEnumerable(enumerable, pixelFilter);
    }

    public static IPixelEnumerable Kernel(this IPixelEnumerable enumerable, uint kernelWidth, uint kernelHeight,
        Func<Pixel[,], Color> kernelFunction, KernelEdgeMode edgeMode = KernelEdgeMode.EXTEND)
    {
        return new KernelEnumerable(enumerable, kernelWidth, kernelHeight, kernelFunction, edgeMode);
    }

    public static IPixelEnumerable Blend(this IPixelEnumerable a, IPixelEnumerable b,
        Func<Color, Color, Color> blendFunction)
    {
        return new BlendModeEnumerable(a, b, blendFunction);
    }
    
    public static IPixelEnumerable Blend(this (IPixelEnumerable, IPixelEnumerable) a,
        Func<Color, Color, Color> blendFunction)
    {
        return a.Item1.Blend(a.Item2, blendFunction);
    }

    public static Image ToImage(this IPixelEnumerable pixelEnumerable, bool showProgress = false, int barLength = 25)
    {
        var image = new Image(pixelEnumerable.GetWidth(), pixelEnumerable.GetHeight());

        for (uint y = 0; y < pixelEnumerable.GetHeight(); y++)
        {
            if (showProgress)
                Console.Write(
                    $"Row: {(y + 1).ToString().PadLeft(pixelEnumerable.GetHeight().ToString().Length)} / {pixelEnumerable.GetHeight()} [");
            var baseCursorPos = Console.CursorLeft;
            double rowPercent = 0;
            for (uint x = 0; x < pixelEnumerable.GetWidth(); x++)
            {
                rowPercent = (double)x / pixelEnumerable.GetWidth();
                if (showProgress)
                {
                    var rowPercentNum = (rowPercent * barLength).Round();
                    Console.Write($"{'@'.Repeat(rowPercentNum).Join("")}{'-'.Repeat(barLength - rowPercentNum).Join("")}]");
                }

                image.SetPixel(x, y, pixelEnumerable[x, y]);
                if (showProgress)
                    Console.CursorLeft = baseCursorPos;
            }

            if (showProgress)
                Console.CursorLeft = 0;
        }

        if (showProgress)
            Console.WriteLine();

        return image;
    }
}