using System.Collections;
using MyLib.Enumerables;

namespace SharperImage.Enumerators;

public class ImageDataEnumerable : IPixelEnumerable
{
    private ImageDataEnumerator _imageDataEnumerator;
    private uint _width;
    private uint _height;

    public ImageDataEnumerable(Image image, Ordering ordering = Ordering.Row)
    {
        _imageDataEnumerator = new ImageDataEnumerator(image, ordering);
        _width = image.Width;
        _height = image.Height;
    }

    public IPixelEnumerator GetPixelEnumerator()
    {
        return _imageDataEnumerator;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return GetPixelEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public uint GetWidth() => _width;
    public uint GetHeight() => _height;
    
    public int Count => (int)GetWidth() * (int)GetHeight();

    public Pixel this[int index] {
        get
        {
            _imageDataEnumerator.SetIndex((uint)index);
            return _imageDataEnumerator.Current;
        }
    }

    public Pixel this[uint x, uint y]
    {
        get
        {
            _imageDataEnumerator.SetX(x);
            _imageDataEnumerator.SetY(y);
            return _imageDataEnumerator.Current;
        }
    }
}


public class ImageDataEnumerator : IPixelEnumerator
{
    private readonly Image _image;
    private readonly Index2dEnumerator _index2dEnumerator;

    public ImageDataEnumerator(Image image, Ordering ordering)
    {
        _index2dEnumerator = new Index2dEnumerator(image.Width, image.Height, ordering);
        _image = image;
    }

    public bool MoveNext()
    {
        return _index2dEnumerator.MoveNext();
    }

    public void Reset()
    {
        _index2dEnumerator.Reset();
    }

    public Pixel Current => _image.PixelData[_index2dEnumerator.Current.Item1, _index2dEnumerator.Current.Item2];

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _index2dEnumerator.Dispose();
        GC.SuppressFinalize(this);
    }

    public uint Count() => GetWidth() * GetHeight();
    
    public uint GetWidth() => _image.Width;
    public uint GetHeight() => _image.Height;

    public void SetIndex(uint index) => _index2dEnumerator.SetIndex(index);
    public void SetX(uint x) => _index2dEnumerator.SetX(x);
    public void SetY(uint y) => _index2dEnumerator.SetY(y);
}