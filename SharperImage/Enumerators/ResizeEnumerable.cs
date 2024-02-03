using System.Collections;

namespace SharperImage.Enumerators;

public class ResizeEnumerable : IPixelEnumerable
{
    private readonly ResizeEnumerator _resizeEnumerator;

    public ResizeEnumerable(IPixelEnumerable internalEnumerable, uint newWidth, uint newHeight)
    {
        _resizeEnumerator = new ResizeEnumerator(internalEnumerable, newWidth, newHeight);
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _resizeEnumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _resizeEnumerator.Count;

    public uint GetWidth() => _resizeEnumerator.Width;

    public uint GetHeight() => _resizeEnumerator.Height;

    public Pixel this[int index] {
        get
        {
            _resizeEnumerator.SetIndex((uint)index);
            return _resizeEnumerator.Current;
        }
    }

    public Pixel this[uint x, uint y] {
        get
        {
            _resizeEnumerator.SetX(x);
            _resizeEnumerator.SetY(y);
            return _resizeEnumerator.Current;
        }
    }
}

public class ResizeEnumerator : IEnumerator<Pixel>
{
    private readonly IPixelEnumerable _internalEnumerator;
    private readonly uint _newWidth;
    private readonly uint _newHeight;
    private uint _x;
    private uint _y;
    
    public ResizeEnumerator(IPixelEnumerable internalEnumerator, uint newWidth, uint newHeight)
    {
        _internalEnumerator = internalEnumerator;
        _newWidth = newWidth;
        _newHeight = newHeight;
        _x = 0;
        _y = 0;
    }
    
    public bool MoveNext()
    {
        _x += 1;
        if (_x >= _newWidth)
        {
            _x = 0;
            _y += 1;
        }
        return _y < _newHeight;
    }

    public void Reset()
    {
        _x = 0;
        _y = 0;
    }

    public Pixel Current =>
        _x < _internalEnumerator.GetWidth() && _y < _internalEnumerator.GetHeight()
            ? _internalEnumerator[_x, _y]
            : new Pixel(_x, _y, Color.Clear);

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _internalEnumerator.GetEnumerator().Dispose();
    }

    public int Count => (int)_newWidth * (int)_newHeight;

    public uint Width => _newWidth;
    public uint Height => _newHeight;
    
    public void SetIndex(uint index)
    {
        _x = index % _newWidth;
        _y = index / _newHeight;
    }
    public void SetX(uint x) => _x = x;
    public void SetY(uint y) => _y = y;
}