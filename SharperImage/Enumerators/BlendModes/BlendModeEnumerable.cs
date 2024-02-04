using System.Collections;

namespace SharperImage.Enumerators.BlendModes;

public class BlendModeEnumerable : IPixelEnumerable
{
    private BlendModeEnumerator _enumerator;

    public BlendModeEnumerable(IPixelEnumerable enumerableA, IPixelEnumerable enumerableB, Func<Color, Color, Color> blendFunction)
    {
        _enumerator = new BlendModeEnumerator(enumerableA, enumerableB, blendFunction);
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _enumerator.Count;
    public uint GetWidth() => _enumerator.Width;
    public uint GetHeight() => _enumerator.Height;

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

public class BlendModeEnumerator : IEnumerator<Pixel>
{
    private readonly IPixelEnumerable _a;
    private readonly IPixelEnumerable _b;
    private uint _x;
    private uint _y;
    private readonly uint _width;
    private readonly uint _height;
    private readonly Func<Color, Color, Color> _blendFunction;

    public BlendModeEnumerator(IPixelEnumerable a, IPixelEnumerable b, Func<Color, Color, Color>  blendFunction)
    {
        _blendFunction = blendFunction;
        _a = a;
        _b = b;
        _x = 0;
        _y = 0;
        _width = a.GetWidth() > b.GetWidth() ? a.GetWidth() : b.GetWidth();
        _height = a.GetHeight() > b.GetHeight() ? a.GetHeight() : b.GetHeight();
    }

    public bool MoveNext()
    {
        _x += 1;
        if (_x >= _width)
        {
            _x = 0;
            _y += 1;
        }

        return _y < _height;
    }

    public void Reset()
    {
        _x = 0;
        _y = 0;
    }

    public Pixel Current
    {
        get
        {
            var a = _x < _a.GetWidth() && _y < _a.GetHeight() ? _a[_x, _y].Color : Color.Clear;
            var b = _x < _b.GetWidth() && _y < _b.GetHeight() ? _b[_x, _y].Color : Color.Clear;
            return new Pixel(_x, _y, _blendFunction(a, b));
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _a.GetEnumerator().Dispose();
        _b.GetEnumerator().Dispose();
        GC.SuppressFinalize(this);
    }

    public void SetIndex(uint index)
    {
        _x = index % _width;
        _y = index / _height;
    }

    public void SetX(uint x)
    {
        _x = x;
    }

    public void SetY(uint y)
    {
        _y = y;
    }

    public int Count => (int)(_width * _height);
    public uint Width => _width;
    public uint Height => _height;
}