using System.Text;
using MyLib.Enumerables;
using MyLib.Streams;
using SharperImage.Enumerators;
using SharperImage.Exceptions;
using SharperImage.Formats.Interfaces;

namespace SharperImage.Formats;

public class QoiImage : IFormat
{
    // TODO add more parameters here so that we can control the output of the QOI image
    // channels 3 rgb / 4 rgba 
    // colorspace 0 srgb / 1 all channels linear
    public void Encode(Image image, Stream stream)
    {
        stream.WriteString("qoif");
        stream.WriteU32(image.Width);
        stream.WriteU32(image.Height);
        stream.WriteByte(4); // TODO fix these
        stream.WriteByte(0);

        var pixels = image.ToPixelEnumerable();
        // pixel map specified by qoi spec
        var pixelHashMap = new Pixel[64];
        var lastPixel = new Pixel(0, 0, Color.CLEAR);
        var run = 0;

        foreach (var pixel in pixels)
        {
            if (pixel == lastPixel)
            {
                run += 1;
                if (run == 62)
                {
                    stream.WriteByte(OpRun(run));
                    run = 0;
                }
            }
            else if (pixelHashMap[GetPixelHash(pixel)] == pixel)
            {
                stream.WriteByte(OpIndex(GetPixelHash(pixel)));
            }
            else
            {
                // output existing run
                if (run > 0)
                {
                    stream.WriteByte(OpRun(run));
                }
                run = 0;

                var redDiff = (int)Math.Floor(255 * pixel.Color.Red - 255 * lastPixel.Color.Red);
                var greenDiff = (int)Math.Floor(255 * pixel.Color.Green - 255 * lastPixel.Color.Green);
                var blueDiff = (int)Math.Floor(255 * pixel.Color.Blue - 255 * lastPixel.Color.Blue);
                var alphaDiff = (int)Math.Floor(255 * pixel.Color.Alpha - 255 * lastPixel.Color.Alpha);
                var drdg = redDiff - greenDiff;
                var dbdg = blueDiff - greenDiff;

                if (alphaDiff != 0)
                {
                    var rgbaBytes = pixel.Color.Rgba();
                    stream.Write(OpRgba(rgbaBytes[0], rgbaBytes[1], rgbaBytes[2], rgbaBytes[3]));
                }
                else if (redDiff is <= 1 and >= -2
                         && greenDiff is <= 1 and >= -2
                         && blueDiff is <= 1 and >= -2)
                {
                    stream.WriteByte(OpDiff(redDiff, greenDiff, blueDiff));
                }
                else if (greenDiff is <= 31 and >= -32 
                         && drdg is <= 7 and >= -8
                         && dbdg is <= 7 and >= -8)
                {
                    stream.Write(OpLuma(greenDiff, drdg, dbdg));
                }
                else
                {
                    var rgbaBytes = pixel.Color.Rgba();
                    stream.Write(OpRgb(rgbaBytes[0], rgbaBytes[1], rgbaBytes[2]));
                }

            }
            
            pixelHashMap[GetPixelHash(pixel)] = pixel;
            lastPixel = pixel;
        }

        if (run != 0)
        {
            stream.WriteByte(OpRun(run));
        }

        stream.Write(new byte[] {0, 0, 0, 0, 0, 0, 0, 1});
    }

    private static byte[] OpRgb(byte r, byte g, byte b)
    {
        return new byte[] { 254, r, g, b };
    }

    private static byte[] OpRgba(byte r, byte g, byte b, byte a)
    {
        return new byte[] { 255, r, g, b, a };
    }

    private static byte OpIndex(int index)
    {
        return (byte)(0b00111111 & index);
    }

    private static byte OpDiff(int dr, int dg, int db)
    {
        var fr = ((dr + 2) & 0b00000011) << 4;
        var fg = ((dg + 2) & 0b00000011) << 2;
        var fb = (db + 2) & 0b00000011;
        return (byte)(0b01000000 | fr | fg | fb);
    }

    private static byte[] OpLuma(int dg, int drdg, int dbdg)
    {
        return new byte[]
        {
            (byte) (0b10000000 | (0b00111111 & (dg + 32))),
            (byte) (((0b00001111 & (drdg + 8)) << 4) | (0b00001111 & (dbdg + 8)))
        };
    }

    private static byte OpRun(int run)
    {
        return (byte)(0b11000000 | (0b00111111 & (run - 1)));
    }

    public Image Decode(Stream stream)
    {
        var headerBytes = stream.ReadBytesAt(0, 14);
        var parsedHeader = new QoiHeader
        {
            MagicNumber = Encoding.Default.GetString(headerBytes[..4]),
            Width = headerBytes[4..8].ToU32(),
            Height = headerBytes[8..12].ToU32(),
            Channels = headerBytes[12],
            ColorSpace = headerBytes[13]
        };

        var width = parsedHeader.Width;
        var height = parsedHeader.Height;
        var pixelData = new Pixel[parsedHeader.Width, parsedHeader.Height];

        if (parsedHeader.MagicNumber != "qoif")
            throw new ImageDecodeException(
                $"This stream is not a QOI image. Expected magic number 'qoif' but got '{parsedHeader.MagicNumber}'");

        var pixels = new List<Pixel>();
        var index = new Pixel?[64];
        var fileOffset = 14;
        var fileLength = stream.Length - 8; //chop off last 8 bytes

        var defaultPixelColor = Color.CLEAR;

        while (fileOffset < fileLength)
        {
            var tag = stream.ReadBytesAt(fileOffset, 1)[0];
            var lastPixel = pixels.LastOrDefault(new Pixel(0, 0, Color.BLACK));
            if (tag == 254) // QOI_OP_RGB
            {
                var r = stream.ReadBytesAt(fileOffset + 1, 1)[0];
                var g = stream.ReadBytesAt(fileOffset + 2, 1)[0];
                var b = stream.ReadBytesAt(fileOffset + 3, 1)[0];
                pixels.Add(new Pixel {Color = new Color(r, g, b, lastPixel.Color.AlphaByte)});
                fileOffset += 4;
            }
            else if (tag == 255) // QOI_OP_RGBA
            {
                var r = stream.ReadBytesAt(fileOffset + 1, 1)[0];
                var g = stream.ReadBytesAt(fileOffset + 2, 1)[0];
                var b = stream.ReadBytesAt(fileOffset + 3, 1)[0];
                var a = stream.ReadBytesAt(fileOffset + 4, 1)[0];
                pixels.Add(new Pixel {Color = new Color(r, g, b, a)});
                fileOffset += 5;
            }
            else if ((tag & 192) == 0) // QOI_OP_INDEX
            {
                pixels.Add(index[tag] ?? new Pixel(0, 0, defaultPixelColor));
                fileOffset += 1;
            }
            else if ((tag & 192) == 64) // QOI_OP_DIFF
            {
                var nr = lastPixel.Color.RedByte.ByteSum(((tag >> 4) & 3) - 2);
                var ng = lastPixel.Color.GreenByte.ByteSum(((tag >> 2) & 3) - 2);
                var nb = lastPixel.Color.BlueByte.ByteSum((tag & 3) - 2);
                pixels.Add(new Pixel {Color = new Color(nr, ng, nb, lastPixel.Color.AlphaByte)});
                fileOffset += 1;
            }
            else if ((tag & 192) == 128) // QOI_OP_LUMA
            {
                var nextByte = stream.ReadBytesAt(fileOffset + 1, 1)[0];
                var diffGreen = (tag & 63) - 32;
                var drdg = ((nextByte >> 4) & 15) - 8;
                var dbdg = (nextByte & 15) - 8;
                var nr = lastPixel.Color.RedByte.ByteSum(diffGreen + drdg);
                var ng = lastPixel.Color.GreenByte.ByteSum(diffGreen);
                var nb = lastPixel.Color.BlueByte.ByteSum(diffGreen + dbdg);
                pixels.Add(new Pixel {Color = new Color(nr, ng, nb, lastPixel.Color.AlphaByte)});
                fileOffset += 2;
            }
            else if ((tag & 192) == 192) // QOI_OP_RUN
            {
                var count = (tag & 63) + 1;
                pixels.AddRange(MakeCopies(lastPixel, count));
                fileOffset += 1;
            }

            var toIndex = pixels.LastOrDefault(new Pixel(0, 0, defaultPixelColor));
            index[GetPixelHash(toIndex)] = toIndex;
        }

        //? Can we modify the above to make this not need the extra pixels array?
        for (uint y = 0; y < parsedHeader.Height; y++)
        {
            for (uint x = 0; x < parsedHeader.Width; x++)
            {
                var pixelIndex = (int) (x + y * parsedHeader.Width);
                var pixel = pixels[pixelIndex];
                pixel.X = x;
                pixel.Y = y;
                pixelData[x, y] = pixel;
            }
        }

        return new Image(width, height, pixelData);
    }

    private static int GetPixelHash(Pixel pixel)
    {
        return (pixel.Color.RedByte * 3 + pixel.Color.GreenByte * 5 + pixel.Color.BlueByte * 7 + pixel.Color.AlphaByte * 11) % 64;
    }

    private static IEnumerable<Pixel> MakeCopies(Pixel pixel, int count)
    {
        var results = new List<Pixel>();
        for (var i = 0; i < count; i++) results.Add(pixel);
        return results;
    }

    private class QoiHeader
    {
        public string MagicNumber { get; init; } = "";
        public uint Width { get; init; }
        public uint Height { get; init; }
        public byte Channels { get; init; }
        public byte ColorSpace { get; init; }
    }
}