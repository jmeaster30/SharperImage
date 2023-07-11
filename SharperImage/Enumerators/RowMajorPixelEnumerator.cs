using System.Collections;

namespace SharperImage.Enumerators;

public class RowMajorPixelEnumerator : IEnumerator<Pixel>
{
    private readonly IImage _image;
    private uint _x;
    private uint _y;

    public RowMajorPixelEnumerator(IImage image)
    {
        _x = 0;
        _y = 0;
        _image = image;
    }
    
    public RowMajorPixelEnumerator(IImage image, int index)
    {
        _x = (uint)index % image.Width();
        _y = (uint)index / image.Height();
        _image = image;
    }

    public bool MoveNext()
    {
        _x += 1;
        if (_x >= _image.Width())
        {
            _x = 0;
            _y += 1;
        }
        return _y < _image.Height();
    }

    public void Reset()
    {
        _x = 0;
        _y = 0;
    }

    public Pixel Current => _image.GetPixel(_x, _y);

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}