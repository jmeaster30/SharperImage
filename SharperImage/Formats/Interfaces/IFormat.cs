namespace SharperImage.Formats.Interfaces;

public interface IFormat
{
    Image Decode(Stream stream);
    void Encode(Image image, Stream stream);
}