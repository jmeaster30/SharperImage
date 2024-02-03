using System.Collections;

namespace SharperImage.Enumerators;

public class ImageDataEnumerable : IPixelEnumerable
{
    private ImageDataEnumerator _imageDataEnumerator;
    private Image _image;
    private uint _width;
    private uint _height;
    private PixelOrdering _ordering;

    public ImageDataEnumerable(Image image, PixelOrdering ordering)
    {
        _imageDataEnumerator = new ImageDataEnumerator(image, ordering);
        _image = image;
        _width = image.Width;
        _height = image.Height;
        _ordering = ordering;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _imageDataEnumerator;
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


public class ImageDataEnumerator : IEnumerator<Pixel>
{
    private readonly Image _image;
    private readonly PixelOrdering _ordering;
    private uint _x;
    private uint _y;

    public ImageDataEnumerator(Image image, PixelOrdering ordering = PixelOrdering.ROW)
    {
        _x = 0;
        _y = 0;
        _ordering = ordering;
        _image = image;
    }

    public bool MoveNext()
    {
        bool result = false;
        switch (_ordering) 
        {
            case PixelOrdering.COLUMN:
            {
                _y += 1;
                if (_y >= _image.Height)
                {
                    _y = 0;
                    _x += 1;
                }
                result = _x < _image.Width;
                break;
            }
            case PixelOrdering.ROW:
            {
                _x += 1;
                if (_x >= _image.Width)
                {
                    _x = 0;
                    _y += 1;
                }
                result = _y < _image.Height;
                break;
            }
        }

        return result;
    }

    public void Reset()
    {
        _x = 0;
        _y = 0;
    }

    public Pixel Current => _image.PixelData[_x, _y];

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void SetIndex(uint index)
    {
        _x = _ordering switch
        {
            PixelOrdering.COLUMN => index / _image.Width,
            PixelOrdering.ROW => index % _image.Width
        };
        _y = _ordering switch
        {
            PixelOrdering.COLUMN => index % _image.Height,
            PixelOrdering.ROW => index / _image.Height
        };
    }

    public void SetX(uint x) => _x = x;
    public void SetY(uint y) => _y = y;
}