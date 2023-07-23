using System.Text;

namespace SharperImage.Formats;

public static class ImageLoaderHelpers
{
    public static IEnumerable<T> PadLeft<T>(this IEnumerable<T> list, int amount, T value)
    {
        var result = new List<T>();
        for (var i = 0; i < amount; i++)
            if (i < list.Count())
                result.Add(list.ElementAt(i));
            else
                result = result.Prepend(value).ToList();
        return result;
    }

    public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    public static float Remap(this int value, int fromMin, int fromMax, int toMin, int toMax)
    {
        return (float) (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    public static int Floor(this float value)
    {
        return (int) Math.Floor(value);
    }

    public static int Ceiling(this double value)
    {
        return (int) Math.Ceiling(value);
    }

    public static void WriteString(this Stream stream, string value)
    {
        stream.Write(Encoding.UTF8.GetBytes(value));
    }
    
    public static void WriteU32(this Stream stream, uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            bytes = bytes.Reverse().ToArray();
        }
        stream.Write(bytes);
    }

    public static string ReadString(this Stream stream, int length)
    {
        var buffer = new byte[length];
        var bytesRead = stream.Read(buffer, 0, length);
        if (bytesRead != length) throw new IndexOutOfRangeException();
        return Encoding.UTF8.GetString(buffer);
    }

    public static ushort ReadU16(this Stream stream, bool skipEndianCheck = false)
    {
        var buffer = new byte[2];
        var bytesRead = stream.Read(buffer, 0, 2);
        if (bytesRead != 2) throw new IndexOutOfRangeException();
        if (BitConverter.IsLittleEndian && !skipEndianCheck) buffer = buffer.Reverse().ToArray();
        return BitConverter.ToUInt16(buffer);
    }
    
    public static uint ReadU32(this Stream stream, bool skipEndianCheck = false)
    {
        var buffer = new byte[4];
        var bytesRead = stream.Read(buffer, 0, 4);
        if (bytesRead != 4) throw new IndexOutOfRangeException();
        if (BitConverter.IsLittleEndian && !skipEndianCheck) buffer = buffer.Reverse().ToArray();
        return BitConverter.ToUInt32(buffer);
    }
    
    public static byte[] ReadBytes(this Stream stream, int count)
    {
        var buffer = new byte[count];
        var bytesRead = stream.Read(buffer, 0, count);
        if (bytesRead != count)
            Console.Error.WriteLine("Didn't read the correct amount of bytes");
        return buffer;
    }
    
    public static byte[] ReadBytes(this Stream stream, uint count)
    {
        var buffer = new byte[count];
        var bytesRead = stream.Read(buffer, 0, (int)count);
        if (bytesRead != count)
            Console.Error.WriteLine("Didn't read the correct amount of bytes");
        return buffer;
    }

    public static byte[] ReadBytes(this Stream stream, long fileOffset, int count)
    {
        var buffer = new byte[count];
        stream.Seek(fileOffset, SeekOrigin.Begin);
        var bytesRead = stream.Read(buffer, 0, count);
        if (bytesRead != count)
            Console.Error.WriteLine("Didn't read the correct amount of bytes");
        return buffer;
    }

    public static byte ReadByte(this Stream stream, long fileOffset)
    {
        stream.Seek(fileOffset, SeekOrigin.Begin);
        var value = stream.ReadByte();
        if (value == -1) throw new ArgumentException("File offset past end of file :(", nameof(fileOffset));
        return (byte) value;
    }

    public static uint ToU32(this byte[] bytes)
    {
        if (bytes.Length > 4) throw new ArgumentException("Too many bytes :( I only wanted 4 or less", nameof(bytes));
        uint result = 0;
        foreach (var b in bytes)
        {
            result *= 256;
            result += b;
        }

        return result;
    }

    public static ushort ToU16(this byte[] bytes)
    {
        if (bytes.Length > 2) throw new ArgumentException("Too many bytes :( I only wanted 2 or less", nameof(bytes));
        ushort result = 0;
        foreach (var b in bytes)
        {
            result *= 256;
            result += b;
        }

        return result;
    }

    public static byte ByteSum(this byte lhs, int rhs)
    {
        return (byte) ((lhs + rhs + 256) % 256);
    }
}