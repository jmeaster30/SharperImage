using MyLib.Bytes;
using MyLib.Streams;
using SharperImage.Enumerators;
using SharperImage.Formats.Interfaces;

namespace SharperImage.Formats;

public class UnformattedImage : IFormat
{
    public Image Decode(Stream stream)
    {
        var width = stream.ReadU32();
        var height = stream.ReadU32();
        var pixelData = new Pixel[width, height];
        // TODO will be compression method of the remainder of the bytes with 0 being raw RGBA bytes
        var compMethod = (byte)stream.ReadByte();
        
        // only uncompressed is supported now
        uint index = 0;
        while (stream.Position != stream.Length)
        {
            var x = index % width;
            var y = index / width;
            var pixel = new Pixel
            {
                Color = new Color
                {
                    Red = (byte) stream.ReadByte(),
                    Green = (byte) stream.ReadByte(),
                    Blue = (byte) stream.ReadByte(),
                    Alpha = (byte) stream.ReadByte(),
                },
                X = x,
                Y = y
            };
            pixelData[x, y] = pixel;
            index += 1;
        }

        return new Image(width, height, pixelData);
    }

    public void Encode(Image image, Stream stream)
    {
        stream.WriteU32(image.Width);
        stream.WriteU32(image.Height);
        stream.WriteByte(0); // TODO add compression methods

        var pixels = image.ToPixelEnumerable();
        foreach (var pix in pixels)
        {
            var bytes = pix.Color.Rgba();
            stream.WriteByte(bytes[0]);
            stream.WriteByte(bytes[1]);
            stream.WriteByte(bytes[2]);
            stream.WriteByte(bytes[3]);
        }
    }
}