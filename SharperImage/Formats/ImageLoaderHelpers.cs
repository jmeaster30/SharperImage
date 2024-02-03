namespace SharperImage.Formats;

public static class ImageLoaderHelpers
{
    public static byte ByteSum(this byte lhs, int rhs)
    {
        return (byte) ((lhs + rhs + 256) % 256);
    }
}