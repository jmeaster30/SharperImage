namespace SharperImage;

public struct Pixel
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public Color Color { get; set; }

    public Pixel(uint x, uint y, Color color)
    {
        X = x;
        Y = y;
        Color = color;
    }

    public override bool Equals(object? obj) => obj is Pixel other && this.Equals(other);
    public bool Equals(Pixel p) => X == p.X && Y == p.Y && Color == p.Color;
    public override int GetHashCode() => (X, Y, Color).GetHashCode();
    public static bool operator ==(Pixel lhs, Pixel rhs) => lhs.Equals(rhs);
    public static bool operator !=(Pixel lhs, Pixel rhs) => !(lhs == rhs);
}