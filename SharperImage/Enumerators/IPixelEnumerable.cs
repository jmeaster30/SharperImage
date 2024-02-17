namespace SharperImage.Enumerators;

public interface IPixelEnumerable : IReadOnlyList<Pixel>
{
    public uint GetWidth();
    public uint GetHeight();
    public IPixelEnumerator GetPixelEnumerator();
    Pixel this[uint x, uint y] { get; }
}

public interface IPixelEnumerator : IEnumerator<Pixel>
{
    public uint Count();
    public uint GetWidth();
    public uint GetHeight();
    public void SetIndex(uint index);
    public void SetX(uint x);
    public void SetY(uint y);
}