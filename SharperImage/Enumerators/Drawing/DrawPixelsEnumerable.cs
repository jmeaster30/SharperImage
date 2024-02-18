using System.Collections;

namespace SharperImage.Enumerators.Drawing;

public class DrawPixelsEnumerable : IPixelEnumerable
{
    private DrawPixelEnumerator _drawPixelEnumerator;

    public DrawPixelsEnumerable(IPixelEnumerable baseEnumerable, Pixel drawnPixel)
    {
        _drawPixelEnumerator = new DrawPixelEnumerator(baseEnumerable, new Dictionary<(uint, uint), Pixel> { { (drawnPixel.X, drawnPixel.Y), drawnPixel}});
    }
    
    public DrawPixelsEnumerable(IPixelEnumerable baseEnumerable, IEnumerable<Pixel> drawnPixel)
    {
        _drawPixelEnumerator = new DrawPixelEnumerator(baseEnumerable, drawnPixel.ToDictionary(pixel => (pixel.X, pixel.Y), pixel => pixel));
    }
    
    public IPixelEnumerator GetPixelEnumerator()
    {
        return _drawPixelEnumerator;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return GetPixelEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_drawPixelEnumerator.Count();
    
    public uint GetWidth() => _drawPixelEnumerator.GetWidth();

    public uint GetHeight() => _drawPixelEnumerator.GetHeight();

    public Pixel this[int index] {
        get
        {
            _drawPixelEnumerator.SetIndex((uint)index);
            return _drawPixelEnumerator.Current;
        }
    }

    public Pixel this[uint x, uint y] {
        get
        {
            _drawPixelEnumerator.SetX(x);
            _drawPixelEnumerator.SetY(y);
            return _drawPixelEnumerator.Current;
        }
    }
}

public class DrawPixelEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerator _baseEnumerator;
    private readonly Dictionary<(uint, uint), Pixel> _drawnPixel;

    public DrawPixelEnumerator(IPixelEnumerable baseEnumerable, Dictionary<(uint, uint), Pixel> drawnPixel)
    {
        _baseEnumerator = (IPixelEnumerator)baseEnumerable.GetEnumerator();
        _drawnPixel = drawnPixel;
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
            if (!_drawnPixel.ContainsKey((pixel.X, pixel.Y))) return pixel;
            
            var replacementPixel = _drawnPixel[(pixel.X, pixel.Y)];
            return new Pixel(replacementPixel.X, replacementPixel.Y, pixel.Color.Composite(replacementPixel.Color));
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