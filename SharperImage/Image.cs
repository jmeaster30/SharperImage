using SharperImage.Formats;

namespace SharperImage;

public class Image
{
    public uint Height { get; private set; }
    public Pixel[,] PixelData { get; }
    public uint Width { get; private set; }

    public void SetPixel(uint x, uint y, Pixel pixel)
    {
        PixelData[x, y] = pixel;
    }

    public Image(uint width, uint height) : this(width, height, new Pixel[width, height]) { }
    public Image(uint width, uint height, Pixel[,] pixels)
    {
        Width = width;
        Height = height;
        PixelData = pixels;
    }

    public void Encode(FileFormat format, Stream stream)
    {
        var formatter = format.GetFormatter();
        formatter.Encode(this, stream);
    }

    public static Image Decode(string imagePath, FileFormat format)
    {
        using var imageFile = File.OpenRead(imagePath);
        var formatter = format.GetFormatter();
        return formatter.Decode(imageFile);
    }

    public static Image Create(uint width, uint height, Color color)
    {
        var image = new Image(width, height);
        for (uint x = 0; x < width; x++)
        {
            for (uint y = 0; y < height; y++)
            {
                image.PixelData[x, y] = new Pixel(x, y, color);
            }
        }

        return image;
    }
}
