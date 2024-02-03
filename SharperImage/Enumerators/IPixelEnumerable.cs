namespace SharperImage.Enumerators;

public interface IPixelEnumerable : IReadOnlyList<Pixel>
{
    public uint GetWidth();
    public uint GetHeight();
    Pixel this[uint x, uint y] { get; }
}