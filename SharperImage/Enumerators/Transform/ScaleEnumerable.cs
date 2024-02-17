using System.Collections;
using MyLib.Enumerables;
using MyLib.Math;

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
    private readonly Index2dEnumerator _index2dEnumerator;
    private readonly IPixelEnumerable _enumerable;
    private readonly uint _newWidth;
    private readonly uint _newHeight;
    private readonly ScaleMode _mode;
    
    public ScaleEnumerator(IPixelEnumerable enumerable, uint newWidth, uint newHeight, ScaleMode mode)
    {
        _enumerable = enumerable;
        _newWidth = newWidth;
        _newHeight = newHeight;
        _mode = mode;
        _index2dEnumerator = new Index2dEnumerator(newWidth, newHeight, Ordering.Row);
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
        var (x, y) = _index2dEnumerator.Current;
        var adjx = (uint)((_enumerable.GetWidth() - 1) * x / (double)_newWidth).Round();
        var adjy = (uint)((_enumerable.GetHeight() - 1) * y / (double)_newHeight).Round();
        return new Pixel(x, y, _enumerable[adjx, adjy].Color);
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
        _index2dEnumerator.Dispose();
        _enumerable.GetEnumerator().Dispose();
        GC.SuppressFinalize(this);
    }

    public uint Count() => GetWidth() * GetHeight();

    public uint GetWidth() => _newWidth;
    public uint GetHeight() => _newHeight;

    public void SetIndex(uint index) => _index2dEnumerator.SetIndex(index);
    public void SetX(uint x) => _index2dEnumerator.SetX(x);
    public void SetY(uint y) => _index2dEnumerator.SetY(y);
}