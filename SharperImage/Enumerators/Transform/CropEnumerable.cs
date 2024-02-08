using System.Collections;
using MyLib.Enumerables;

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
    private readonly Index2dEnumerator _index2dEnumerator;
    private readonly uint _newWidth;
    private readonly uint _newHeight;
    
    public CropEnumerator(IPixelEnumerable internalEnumerator, uint newWidth, uint newHeight)
    {
        _internalEnumerator = internalEnumerator;
        _newWidth = newWidth;
        _newHeight = newHeight;
        _index2dEnumerator = new Index2dEnumerator(_newWidth, _newHeight, Ordering.Row);
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
            return x < _internalEnumerator.GetWidth() && y < _internalEnumerator.GetHeight()
                ? _internalEnumerator[x, y]
                : new Pixel(x, y, Color.Clear);
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _internalEnumerator.GetEnumerator().Dispose();
        GC.SuppressFinalize(this);
    }

    public int Count => (int)_newWidth * (int)_newHeight;

    public uint GetWidth() => _newWidth;
    public uint GetHeight() => _newHeight;

    public void SetIndex(uint index) => _index2dEnumerator.SetIndex(index);
    public void SetX(uint x) => _index2dEnumerator.SetX(x);
    public void SetY(uint y) => _index2dEnumerator.SetY(y);
}