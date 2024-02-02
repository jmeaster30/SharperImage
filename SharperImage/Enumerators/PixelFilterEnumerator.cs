using System.Collections;

namespace SharperImage.Enumerators;

public class PixelFilterEnumerator : IEnumerator<Pixel>
{
    private IEnumerable<Pixel> _internalEnumerator;
    private Func<Pixel, Pixel> _pixelFilter;

    public PixelFilterEnumerator(IEnumerable<Pixel> enumerator, Func<Pixel, Pixel> filter)
    {
        _internalEnumerator = enumerator;
        _pixelFilter = filter;
    }
    
    public bool MoveNext()
    {
        return _internalEnumerator.GetEnumerator().MoveNext();
    }

    public void Reset()
    {
        _internalEnumerator.GetEnumerator().Reset();
    }

    public Pixel Current => _pixelFilter(_internalEnumerator.GetEnumerator().Current);

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _internalEnumerator.GetEnumerator().Dispose();
    }
}