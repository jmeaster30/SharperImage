using SharperImage.Enumerators.Transform;

namespace SharperImage.Enumerators;

public static class ImageExtensions
{
    public static ImageDataEnumerable ToPixelEnumerable(this Image image, PixelOrdering ordering = PixelOrdering.ROW)
    {
        return new ImageDataEnumerable(image, ordering);
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
    
    public static Image ToImage(this IPixelEnumerable pixelEnumerable)
    {
        var image = new Image(pixelEnumerable.GetWidth(), pixelEnumerable.GetHeight());
        
        foreach (var pixel in pixelEnumerable)
        {
            image.SetPixel(pixel.X, pixel.Y, pixel);
        }

        return image;
    }
}