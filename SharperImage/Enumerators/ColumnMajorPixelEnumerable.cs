using System.Collections;

namespace SharperImage.Enumerators;

public class ColumnMajorPixelEnumerable : IEnumerable<Pixel>
{
    private IImage _image;

    public ColumnMajorPixelEnumerable(IImage image)
    {
        _image = image;
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        for (uint x = 0; x < _image.Width(); x++)
        {
            for (uint y = 0; y < _image.Height(); y++)
            {
                yield return _image.GetPixel(x, y);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}