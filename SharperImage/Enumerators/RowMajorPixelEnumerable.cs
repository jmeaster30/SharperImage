using System.Collections;

namespace SharperImage.Enumerators;

public class RowMajorPixelEnumerable : IEnumerable<Pixel>
{
    private Pixel[,] _pixels;
    private uint _width;
    private uint _height;

    public RowMajorPixelEnumerable(IImage image)
    {
        _pixels = image.PixelArray();
        _width = image.Width();
        _height = image.Height();
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        for (uint y = 0; y < _height; y++)
        {
            for (uint x = 0; x < _width; x++)
            {
                yield return _pixels[x, y];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}