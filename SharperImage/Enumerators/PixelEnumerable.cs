using System.Collections;

namespace SharperImage.Enumerators;

public class PixelEnumerable<T> : IReadOnlyList<Pixel> where T : IEnumerator<Pixel>
{
    private Image _image;
    private Func<Image, int, T> _constructor;

    public PixelEnumerable(Image image, Func<Image, int, T> constructor)
    {
        _image = image;
        _constructor = constructor;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _constructor(_image, 0);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)_image.Width * (int)_image.Height;

    public Pixel this[int index] {
        get
        {
            var enumerator = _constructor(_image, index);
            return enumerator.Current;
        }
    }
}