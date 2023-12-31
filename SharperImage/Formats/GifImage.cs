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

/*
using Newtonsoft.Json;

namespace SharperImage.ImageLoaders;

public class GifColor
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
}

public class GifHeader
{
    public string MagicNumber { get; set; }
    public string Version { get; set; }
}

public class GifLogicalScreenDescriptor
{
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public bool GlobalColorTableFlag { get; set; }
    public byte ColorResolution { get; set; } // 3 bit
    public bool SortFlag { get; set; }
    public byte SizeOfGlobalColorTable { get; set; } // 3 bit
    public byte BackgroundColorIndex { get; set; }
    public byte PixelAspectRatio { get; set; }
}

public class GifImageDescriptor
{
    public byte ImageSeparator { get; set; }
    public ushort ImageLeftPosition { get; set; }
    public ushort ImageTopPosition { get; set; }
    public ushort ImageWidth { get; set; }
    public ushort ImageHeight { get; set; }
    public bool LocalColorTableFlag { get; set; }
    public bool InterlaceFlag { get; set; }
    public bool SortFlag { get; set; }
    public byte Reserved { get; set; }
    public byte SizeOfLocalColorTable { get; set; }
}

public class GifCommentExtension
{
    
}


public class GifLoader : IImageLoader
{
    public static Image Load(string filename)
    {
        using var imageFile = File.OpenRead(filename);

        var header = new GifHeader
        {
            MagicNumber = imageFile.ReadString(3),
            Version = imageFile.ReadString(3),
        };

        var screenDescriptor = new GifLogicalScreenDescriptor
        {
            Width = imageFile.ReadU16(true),
            Height = imageFile.ReadU16(true),
        };

        var packedBytes = imageFile.ReadByte();
        screenDescriptor.GlobalColorTableFlag = (packedBytes & 0b10000000) != 0;
        screenDescriptor.ColorResolution = (byte)((packedBytes & 0b01110000) >> 4);
        screenDescriptor.SortFlag = (packedBytes & 0b00001000) != 0;
        screenDescriptor.SizeOfGlobalColorTable = (byte)(packedBytes & 0b00000111);

        screenDescriptor.BackgroundColorIndex = (byte)imageFile.ReadByte();
        screenDescriptor.PixelAspectRatio = (byte)imageFile.ReadByte();

        var globalColorTable = new List<GifColor>();
        var gctSize = 1 << (screenDescriptor.SizeOfGlobalColorTable + 1);
        for (var i = 0; i < gctSize; i++)
        {
            globalColorTable.Add(new GifColor
            {
                R = (byte)imageFile.ReadByte(),
                G = (byte)imageFile.ReadByte(),
                B = (byte)imageFile.ReadByte(),
            });
        }

        var separator = (byte)imageFile.ReadByte();
        imageFile.Seek(-1, SeekOrigin.Current);

        switch (separator)
        {
            case 0x21: // comment extension
                break;
        }
        
        Console.WriteLine($"SEP {(char)separator}");
        
        Console.WriteLine(JsonConvert.SerializeObject(globalColorTable));

        return null;
    }

    private static GifCommentExtension ParseCommentExtension(Stream stream)
    {
        return null;
    }
}
*/
