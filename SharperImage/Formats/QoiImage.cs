using System.Text;
using Newtonsoft.Json;
using SharperImage.Enumerators;

namespace SharperImage.Formats;

public class QoiImage : IImage
{
    // TODO add more parameters here so that we can control the output of the QOI image
    // channels 3 rgb / 4 rgba 
    // colorspace 0 srgb / 1 all channels linear
    private uint _height;
    private Pixel[,] _pixelData = { };
    private uint _width;

    public FileFormat FileFormat() => Formats.FileFormat.QOI;
    public uint Height() => _height;
    public uint Width() => _width;
    public Pixel[,] PixelArray() => _pixelData;
    public Pixel GetPixel(uint x, uint y) => _pixelData[x, y];

    public void Encode(Stream stream)
    {
        stream.WriteString("qoif");
        stream.WriteU32(_width);
        stream.WriteU32(_height);
        stream.WriteByte(4); // TODO fix these
        stream.WriteByte(0);

        var pixels = this.ToRowRankPixelEnumerable();
        // pixel map specified by qoi spec
        var pixelHashMap = new Pixel[64];
        var lastPixel = new Pixel(0, 0, 0, 0, 0, 255);
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

                var redDiff = pixel.R - lastPixel.R;
                var greenDiff = pixel.G - lastPixel.G;
                var blueDiff = pixel.B - lastPixel.B;
                var alphaDiff = pixel.A - lastPixel.A;
                var drdg = redDiff - greenDiff;
                var dbdg = blueDiff - greenDiff;

                if (alphaDiff != 0)
                {
                    stream.Write(OpRgba(pixel.R, pixel.G, pixel.B, pixel.A));
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
                    stream.Write(OpRgb(pixel.R, pixel.G, pixel.B));
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

    public void Decode(Stream stream)
    {
        var headerBytes = stream.ReadBytes(0, 14);
        var parsedHeader = new QoiHeader
        {
            MagicNumber = Encoding.Default.GetString(headerBytes[..4]),
            Width = headerBytes[4..8].ToU32(),
            Height = headerBytes[8..12].ToU32(),
            Channels = headerBytes[12],
            ColorSpace = headerBytes[13]
        };
        Console.WriteLine(JsonConvert.SerializeObject(parsedHeader));

        _width = parsedHeader.Width;
        _height = parsedHeader.Height;
        _pixelData = new Pixel[parsedHeader.Width, parsedHeader.Height];

        if (parsedHeader.MagicNumber != "qoif")
            throw new ArgumentException(
                $"This stream is not a QOI image. Expected magic number 'qoif' but got '{parsedHeader.MagicNumber}'",
                nameof(stream));

        var pixels = new List<Pixel>();
        var index = new Pixel?[64];
        var fileOffset = 14;
        var fileLength = stream.Length - 8; //chop off last 8 bytes

        while (fileOffset < fileLength)
        {
            var tag = stream.ReadByte(fileOffset);
            var lastPixel = pixels.LastOrDefault(new Pixel(0, 0, 0, 0, 0, 255));
            if (tag == 254) // QOI_OP_RGB
            {
                var r = stream.ReadByte(fileOffset + 1);
                var g = stream.ReadByte(fileOffset + 2);
                var b = stream.ReadByte(fileOffset + 3);
                pixels.Add(new Pixel {R = r, G = g, B = b, A = lastPixel.A});
                fileOffset += 4;
            }
            else if (tag == 255) // QOI_OP_RGBA
            {
                var r = stream.ReadByte(fileOffset + 1);
                var g = stream.ReadByte(fileOffset + 2);
                var b = stream.ReadByte(fileOffset + 3);
                var a = stream.ReadByte(fileOffset + 4);
                pixels.Add(new Pixel {R = r, G = g, B = b, A = a});
                fileOffset += 5;
            }
            else if ((tag & 192) == 0) // QOI_OP_INDEX
            {
                pixels.Add(index[tag] ?? new Pixel(0, 0, 0, 0, 0, 0));
                fileOffset += 1;
            }
            else if ((tag & 192) == 64) // QOI_OP_DIFF
            {
                var nr = lastPixel.R.ByteSum(((tag >> 4) & 3) - 2);
                var ng = lastPixel.G.ByteSum(((tag >> 2) & 3) - 2);
                var nb = lastPixel.B.ByteSum((tag & 3) - 2);
                pixels.Add(new Pixel {R = nr, G = ng, B = nb, A = lastPixel.A});
                fileOffset += 1;
            }
            else if ((tag & 192) == 128) // QOI_OP_LUMA
            {
                var nextByte = stream.ReadByte(fileOffset + 1);
                var diffGreen = (tag & 63) - 32;
                var drdg = ((nextByte >> 4) & 15) - 8;
                var dbdg = (nextByte & 15) - 8;
                var nr = lastPixel.R.ByteSum(diffGreen + drdg);
                var ng = lastPixel.G.ByteSum(diffGreen);
                var nb = lastPixel.B.ByteSum(diffGreen + dbdg);
                pixels.Add(new Pixel {R = nr, G = ng, B = nb, A = lastPixel.A});
                fileOffset += 2;
            }
            else if ((tag & 192) == 192) // QOI_OP_RUN
            {
                var count = (tag & 63) + 1;
                pixels.AddRange(MakeCopies(lastPixel, count));
                fileOffset += 1;
            }

            var toIndex = pixels.LastOrDefault(new Pixel(0, 0, 0, 0, 0, 255));
            index[GetPixelHash(toIndex)] = toIndex;
        }

        //? Can we modify the above to make this not need the extra pixels array?
        for (var y = 0; y < parsedHeader.Height; y++)
        {
            for (var x = 0; x < parsedHeader.Width; x++)
            {
                var pixelIndex = (int) (x + y * parsedHeader.Width);
                var pixel = pixels[pixelIndex];
                pixel.X = x;
                pixel.Y = y;
                _pixelData[x, y] = pixel;
            }
        }
    }

    public static QoiImage LoadImage(Stream stream)
    {
        var qoi = new QoiImage();
        qoi.Decode(stream);
        return qoi;
    }

    private static int GetPixelHash(Pixel pixel)
    {
        return (pixel.R * 3 + pixel.G * 5 + pixel.B * 7 + pixel.A * 11) % 64;
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