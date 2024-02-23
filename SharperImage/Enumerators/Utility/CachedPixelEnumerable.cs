using System.Collections;
using MyLib.Enumerables;

namespace SharperImage.Enumerators.Utility;

public class CachedPixelEnumerable : IPixelEnumerable
{
    private CachedPixelEnumerator _enumerator;

    public CachedPixelEnumerable(IPixelEnumerable baseEnumerable)
    {
        _enumerator = new CachedPixelEnumerator(baseEnumerable);
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_enumerator.Count();

    public Pixel this[int index]
    {
        get
        {
            _enumerator.SetIndex((uint)index);
            return _enumerator.Current;
        }
    }

    public uint GetWidth()
    {
        return _enumerator.GetWidth();
    }

    public uint GetHeight()
    {
        return _enumerator.GetHeight();
    }

    public IPixelEnumerator GetPixelEnumerator()
    {
        return _enumerator;
    }

    public Pixel this[uint x, uint y]
    {
        get
        {
            _enumerator.SetX(x);
            _enumerator.SetY(y);
            return _enumerator.Current;
        }
    }
}

public class CachedPixelEnumerator : IPixelEnumerator
{
    private IPixelEnumerable _baseEnumerable;
    private Dictionary<(uint, uint), Pixel> _cache;
    private Index2dEnumerator _index2dEnumerator;

    public CachedPixelEnumerator(IPixelEnumerable baseEnumerable)
    {
        _baseEnumerable = baseEnumerable;
        _cache = new Dictionary<(uint, uint), Pixel>();
        _index2dEnumerator = new Index2dEnumerator(baseEnumerable.GetWidth(), baseEnumerable.GetHeight(), Ordering.Row);
    }
    
    public bool MoveNext()
    {
        return _index2dEnumerator.MoveNext();
    }

    public void Reset()
    {
        _index2dEnumerator.Reset();
    }

    public Pixel Current
    {
        get
        {
            if (_cache.TryGetValue(_index2dEnumerator.Current, out var current))
                return current;
            _baseEnumerable.GetPixelEnumerator().SetX(_index2dEnumerator.Current.Item1);
            _baseEnumerable.GetPixelEnumerator().SetY(_index2dEnumerator.Current.Item2);
            var pixel = _baseEnumerable.GetPixelEnumerator().Current;
            _cache.Add(_index2dEnumerator.Current, pixel);
            return pixel;
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _baseEnumerable.GetEnumerator().Dispose();
    }

    public uint Count()
    {
        return _baseEnumerable.GetWidth() * _baseEnumerable.GetHeight();
    }

    public uint GetWidth()
    {
        return _baseEnumerable.GetWidth();
    }

    public uint GetHeight()
    {
        return _baseEnumerable.GetHeight();
    }

    public void SetIndex(uint index)
    {
        _index2dEnumerator.SetIndex(index);
    }

    public void SetX(uint x)
    {
        _index2dEnumerator.SetX(x);
    }

    public void SetY(uint y)
    {
        _index2dEnumerator.SetY(y);
    }
}