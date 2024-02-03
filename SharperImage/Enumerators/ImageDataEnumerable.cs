using System.Collections;

namespace SharperImage.Enumerators;

public class ImageDataEnumerable : IReadOnlyList<Pixel>
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

    public int Count => (int)_width * (int)_height;

    public Pixel this[int index] {
        get
        {
            var enumerator = new ImageDataEnumerator(_image, index, _ordering);
            return enumerator.Current;
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
    
    public ImageDataEnumerator(Image image, int index, PixelOrdering ordering = PixelOrdering.ROW)
    {
        _ordering = ordering;
        _image = image;
        _x = _ordering switch
        {
            PixelOrdering.COLUMN => (uint)index / image.Width,
            PixelOrdering.ROW => (uint)index % image.Width
        };
        _y = _ordering switch
        {
            PixelOrdering.COLUMN => (uint)index % image.Height,
            PixelOrdering.ROW => (uint)index / image.Height
        };
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
}