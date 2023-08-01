using SharperImage.Formats.Interfaces;

namespace SharperImage.Formats;

public class GifImage : IFormat
{
    public Image Decode(Stream stream)
    {
        throw new NotImplementedException();
    }

    public void Encode(Image image, Stream stream)
    {
        throw new NotImplementedException();
    }
}