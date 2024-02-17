using System.Collections;

namespace SharperImage.Enumerators.Drawing;

public class DrawRectangleEnumerable : IPixelEnumerable
{
    private DrawRectangleEnumerator _drawRectangleEnumerator;

    public DrawRectangleEnumerable(IPixelEnumerable baseEnumerable, uint x, uint y, uint width, uint height, Color color)
    {
        _drawRectangleEnumerator = new DrawRectangleEnumerator(baseEnumerable, x, y, width, height, color);
    }
    
    public IPixelEnumerator GetPixelEnumerator()
    {
        return _drawRectangleEnumerator;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return GetPixelEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_drawRectangleEnumerator.Count();
    
    public uint GetWidth() => _drawRectangleEnumerator.GetWidth();

    public uint GetHeight() => _drawRectangleEnumerator.GetHeight();

    public Pixel this[int index] {
        get
        {
            _drawRectangleEnumerator.SetIndex((uint)index);
            return _drawRectangleEnumerator.Current;
        }
    }

    public Pixel this[uint x, uint y] {
        get
        {
            _drawRectangleEnumerator.SetX(x);
            _drawRectangleEnumerator.SetY(y);
            return _drawRectangleEnumerator.Current;
        }
    }
}

public class DrawRectangleEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerator _baseEnumerator;
    private readonly uint _x;
    private readonly uint _y;
    private readonly uint _width;
    private readonly uint _height;
    private readonly Color _color;
    

    public DrawRectangleEnumerator(IPixelEnumerable baseEnumerable, uint x, uint y, uint width, uint height, Color color)
    {
        _baseEnumerator = (IPixelEnumerator)baseEnumerable.GetEnumerator();
        _x = x;
        _y = y;
        _width = width;
        _height = height;
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
            return pixel.X >= _x && pixel.Y >= _y && pixel.X <= _x + _width && pixel.Y <= _y + _height 
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