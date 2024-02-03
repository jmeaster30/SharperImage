using System.Collections;

namespace SharperImage.Enumerators;

public class PixelFilterEnumerable : IPixelEnumerable
{
    private PixelFilterEnumerator _pixelEnumerator;

    public PixelFilterEnumerable(IPixelEnumerable pixelEnumerable, Func<Pixel, Pixel> pixelFilter)
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

    public uint GetWidth() => _pixelEnumerator.GetWidth();

    public uint GetHeight() => _pixelEnumerator.GetHeight();

    public int Count => _pixelEnumerator.Count();

    public Pixel this[int index] => _pixelEnumerator[index];

    public Pixel this[uint x, uint y] => _pixelEnumerator[x, y];
}

public class PixelFilterEnumerator : IEnumerator<Pixel>
{
    private IPixelEnumerable _internalEnumerator;
    private Func<Pixel, Pixel> _pixelFilter;

    public PixelFilterEnumerator(IPixelEnumerable enumerator, Func<Pixel, Pixel> filter)
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

    public uint GetWidth() => _internalEnumerator.GetWidth();
    
    public uint GetHeight() => _internalEnumerator.GetHeight();
    
    public int Count() => _internalEnumerator.Count;
    
    public Pixel this[int index] => _pixelFilter(_internalEnumerator[index]); 
    
    public Pixel this[uint x, uint y] => _pixelFilter(_internalEnumerator[x, y]);
}