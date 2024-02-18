using System.Collections;
using MyLib.Math;

namespace SharperImage.Enumerators.Drawing;

public class DrawLineEnumerable : IPixelEnumerable
{
    private DrawLineEnumerator _drawLineEnumerator;

    public DrawLineEnumerable(IPixelEnumerable baseEnumerable, int x1, int y1, int x2, int y2, Color color)
    {
        _drawLineEnumerator = new DrawLineEnumerator(baseEnumerable, x1, y1, x2, y2, color);
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
    private readonly int _x1;
    private readonly int _y1;
    private readonly int _x2;
    private readonly int _y2;
    private readonly Color _color;
    
    private readonly int _minX;
    private readonly int _minY;
    private readonly int _maxX;
    private readonly int _maxY;
    

    public DrawLineEnumerator(IPixelEnumerable baseEnumerable, int x1, int y1, int x2, int y2, Color color)
    {
        _baseEnumerator = (IPixelEnumerator)baseEnumerable.GetEnumerator();
        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
        _color = color;

        _minX = _x1.Min(_x2);
        _minY = _y1.Min(_y2);
        _maxX = _x1.Max(_x2);
        _maxY = _y1.Max(_y2);
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
            var slopeX = (double)(_y2 - _y1) / (_x2 - _x1);
            var onLineX = System.Math.Abs(slopeX * pixel.X + _y1 - slopeX * _x1 - pixel.Y);
            var slopeY = (double)(_x2 - _x1) / (_y2 - _y1);
            var onLineY = System.Math.Abs(slopeY * pixel.Y + _x1 - slopeY * _y1 - pixel.X);
            return (0.5 >= onLineX || 0.5 >= onLineY) 
                   && pixel.X >= _minX 
                   && pixel.Y >= _minY
                   && pixel.X <= _maxX
                   && pixel.Y <= _maxY
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