using System.Collections;
using MyLib.Enumerables;

namespace SharperImage.Enumerators;

public class BlendModeEnumerable : IPixelEnumerable
{
    private BlendModeEnumerator _enumerator;

    public BlendModeEnumerable(IPixelEnumerable enumerableA, IPixelEnumerable enumerableB, Func<Color, Color, Color> blendFunction)
    {
        _enumerator = new BlendModeEnumerator(enumerableA, enumerableB, blendFunction);
    }
    
    public IPixelEnumerator GetPixelEnumerator()
    {
        return _enumerator;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return GetPixelEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_enumerator.Count();
    public uint GetWidth() => _enumerator.GetWidth();
    public uint GetHeight() => _enumerator.GetHeight();

    public Pixel this[int index]
    {
        get
        {
            _enumerator.SetIndex((uint)index);
            return _enumerator.Current;
        }
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

public class BlendModeEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerable _a;
    private readonly IPixelEnumerable _b;
    private readonly Index2dEnumerator _index2dEnumerator;
    private readonly uint _width;
    private readonly uint _height;
    private readonly Func<Color, Color, Color> _blendFunction;

    public BlendModeEnumerator(IPixelEnumerable a, IPixelEnumerable b, Func<Color, Color, Color>  blendFunction)
    {
        _blendFunction = blendFunction;
        _a = a;
        _b = b;
        _width = a.GetWidth() > b.GetWidth() ? a.GetWidth() : b.GetWidth();
        _height = a.GetHeight() > b.GetHeight() ? a.GetHeight() : b.GetHeight();
        _index2dEnumerator = new Index2dEnumerator(_width, _height, Ordering.Row);
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
            var (x, y) = _index2dEnumerator.Current;
            var a = x < _a.GetWidth() && y < _a.GetHeight() ? _a[x, y].Color : Color.CLEAR;
            var b = x < _b.GetWidth() && y < _b.GetHeight() ? _b[x, y].Color : Color.CLEAR;
            return new Pixel(x, y, _blendFunction(a, b));
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _a.GetEnumerator().Dispose();
        _b.GetEnumerator().Dispose();
        GC.SuppressFinalize(this);
    }

    public void SetIndex(uint index) => _index2dEnumerator.SetIndex(index);
    public void SetX(uint x) => _index2dEnumerator.SetX(x);
    public void SetY(uint y) => _index2dEnumerator.SetY(y);

    public uint Count() => GetWidth() * GetHeight();
    public uint GetWidth() => _width;
    public uint GetHeight() => _height;
}