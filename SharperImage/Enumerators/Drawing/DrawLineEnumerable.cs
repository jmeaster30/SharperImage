using System.Collections;

namespace SharperImage.Enumerators.Drawing;

public class DrawLineEnumerable : IPixelEnumerable
{
    private DrawLineEnumerator _drawLineEnumerator;

    public DrawLineEnumerable(IPixelEnumerable baseEnumerable, uint x1, uint y1, uint x2, uint y2, Color color, double tolerance)
    {
        _drawLineEnumerator = new DrawLineEnumerator(baseEnumerable, x1, y1, x2, y2, color, tolerance);
    }
    
    public IPixelEnumerator GetPixelEnumerator()
    {
        return _drawLineEnumerator;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return GetPixelEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_drawLineEnumerator.Count();
    
    public uint GetWidth() => _drawLineEnumerator.GetWidth();

    public uint GetHeight() => _drawLineEnumerator.GetHeight();

    public Pixel this[int index] {
        get
        {
            _drawLineEnumerator.SetIndex((uint)index);
            return _drawLineEnumerator.Current;
        }
    }

    public Pixel this[uint x, uint y] {
        get
        {
            _drawLineEnumerator.SetX(x);
            _drawLineEnumerator.SetY(y);
            return _drawLineEnumerator.Current;
        }
    }
}

public class DrawLineEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerator _baseEnumerator;
    private readonly uint _x1;
    private readonly uint _y1;
    private readonly uint _x2;
    private readonly uint _y2;
    private readonly Color _color;
    private readonly double _tolerance;
    

    public DrawLineEnumerator(IPixelEnumerable baseEnumerable, uint x1, uint y1, uint x2, uint y2, Color color, double tolerance)
    {
        _baseEnumerator = (IPixelEnumerator)baseEnumerable.GetEnumerator();
        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
        _tolerance = tolerance;
        _color = color;
    }

    public bool MoveNext()
    {
        return _baseEnumerator.MoveNext();
    }

    public void Reset()
    {
        _baseEnumerator.Reset();
    }
    
    public Pixel Current
    {
        get
        {
            var pixel = _baseEnumerator.Current;
            var slope = ((int)_y2 - (int)_y1) / ((int)_x2 - (int)_x1);
            return _tolerance >= Math.Abs(slope * pixel.X + _y1 - slope * _x1 - pixel.Y)
                ? new Pixel(pixel.X, pixel.Y, pixel.Color.Composite(_color)) 
                : pixel;
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _baseEnumerator.Dispose();
        GC.SuppressFinalize(this);
    }

    public uint Count() => _baseEnumerator.Count();

    public uint GetWidth() => _baseEnumerator.GetWidth();
    public uint GetHeight() => _baseEnumerator.GetHeight();

    public void SetIndex(uint index) => _baseEnumerator.SetIndex(index);
    public void SetX(uint x) => _baseEnumerator.SetX(x);
    public void SetY(uint y) => _baseEnumerator.SetY(y);
}