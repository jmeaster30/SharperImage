using System.Collections;

namespace SharperImage.Enumerators;

public class PixelFilterEnumerable : IEnumerable<Pixel>
{
    private PixelFilterEnumerator _pixelEnumerator;

    public PixelFilterEnumerable(IEnumerable<Pixel> pixelEnumerable, Func<Pixel, Pixel> pixelFilter)
    {
        _pixelEnumerator = new PixelFilterEnumerator(pixelEnumerable, pixelFilter);
    }

    public IEnumerator<Pixel> GetEnumerator()
    {
        return _pixelEnumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}