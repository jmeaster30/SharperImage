using System.Collections;

namespace SharperImage.Enumerators.Transform;

public enum ScaleMode
{
    NEAREST_NEIGHBOR,
    BILINEAR,
    BICUBIC,
}

public class ScaleEnumerable : IPixelEnumerable
{
    private readonly ScaleEnumerator _enumerator;

    public ScaleEnumerable(IPixelEnumerable enumerable, uint newWidth, uint newHeight, ScaleMode mode = ScaleMode.NEAREST_NEIGHBOR)
    {
        _enumerator = new ScaleEnumerator(enumerable, newWidth, newHeight, mode);
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
    public uint GetWidth() => _enumerator.GetWidth();
    public uint GetHeight() => _enumerator.GetHeight();
    
    public Pixel this[int index] {
        get
        {
            _enumerator.SetIndex((uint)index);
            return _enumerator.Current;
        }
    }
    public Pixel this[uint x, uint y] {
        get
        {
            _enumerator.SetX(x);
            _enumerator.SetY(y);
            return _enumerator.Current;
        }
    }
}

public class ScaleEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerable _enumerable;
    private readonly uint _newWidth;
    private readonly uint _newHeight;
    private readonly ScaleMode _mode;

    private uint _x;
    private uint _y;
    
    public ScaleEnumerator(IPixelEnumerable enumerable, uint newWidth, uint newHeight, ScaleMode mode)
    {
        _enumerable = enumerable;
        _newWidth = newWidth;
        _newHeight = newHeight;
        _mode = mode;
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

    public Pixel Current
    {
        get
        {
            return _mode switch
            {
                ScaleMode.NEAREST_NEIGHBOR => nearestNeighbor(),
                ScaleMode.BILINEAR => bilinear(),
                ScaleMode.BICUBIC => bicubic(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private Pixel nearestNeighbor()
    {
        var adjx = (uint)Math.Round((_enumerable.GetWidth() - 1) * _x / (double)_newWidth);
        var adjy = (uint)Math.Round((_enumerable.GetHeight() - 1) * _y / (double)_newHeight);
        return new Pixel(_x, _y, _enumerable[adjx, adjy].Color);
    }
    
    private Pixel bilinear()
    {
        throw new NotImplementedException();
    }
    
    private Pixel bicubic()
    {
        throw new NotImplementedException();
    }
    

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _enumerable.GetEnumerator().Dispose();
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