using System.Collections;

namespace SharperImage.Enumerators.Transform;

public class CropEnumerable : IPixelEnumerable
{
    private readonly CropEnumerator _cropEnumerator;

    public CropEnumerable(IPixelEnumerable internalEnumerable, uint newWidth, uint newHeight)
    {
        _cropEnumerator = new CropEnumerator(internalEnumerable, newWidth, newHeight);
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _cropEnumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _cropEnumerator.Count;

    public uint GetWidth() => _cropEnumerator.GetWidth();

    public uint GetHeight() => _cropEnumerator.GetHeight();

    public Pixel this[int index] {
        get
        {
            _cropEnumerator.SetIndex((uint)index);
            return _cropEnumerator.Current;
        }
    }

    public Pixel this[uint x, uint y] {
        get
        {
            _cropEnumerator.SetX(x);
            _cropEnumerator.SetY(y);
            return _cropEnumerator.Current;
        }
    }
}

public class CropEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerable _internalEnumerator;
    private readonly uint _newWidth;
    private readonly uint _newHeight;
    private uint _x;
    private uint _y;
    
    public CropEnumerator(IPixelEnumerable internalEnumerator, uint newWidth, uint newHeight)
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
        GC.SuppressFinalize(this);
    }

    public int Count => (int)_newWidth * (int)_newHeight;

    public uint GetWidth() => _newWidth;
    public uint GetHeight() => _newHeight;
    
    public void SetIndex(uint index)
    {
        _x = index % _newWidth;
        _y = index / _newHeight;
    }
    public void SetX(uint x) => _x = x;
    public void SetY(uint y) => _y = y;
}