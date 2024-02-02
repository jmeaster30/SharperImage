using System.Collections;

namespace SharperImage.Enumerators;

public class PixelEnumerable : IReadOnlyList<Pixel>
{
    private PixelEnumerator _pixelEnumerator;
    private Image _image;
    private uint _width;
    private uint _height;
    private PixelOrdering _ordering;

    public PixelEnumerable(Image image, PixelOrdering ordering)
    {
        _pixelEnumerator = new PixelEnumerator(image, ordering);
        _image = image;
        _width = image.Width;
        _height = image.Height;
        _ordering = ordering;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _pixelEnumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_width * (int)_height;

    public Pixel this[int index] {
        get
        {
            var enumerator = new PixelEnumerator(_image, index, _ordering);
            return enumerator.Current;
        }
    }
}