using System.Collections;

namespace SharperImage.Enumerators;

public class PixelEnumerator : IEnumerator<Pixel>
{
    private readonly Image _image;
    private readonly PixelOrdering _ordering;
    private uint _x;
    private uint _y;

    public PixelEnumerator(Image image, PixelOrdering ordering = PixelOrdering.ROW)
    {
        _x = 0;
        _y = 0;
        _ordering = ordering;
        _image = image;
    }
    
    public PixelEnumerator(Image image, int index, PixelOrdering ordering = PixelOrdering.ROW)
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