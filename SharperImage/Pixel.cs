namespace SharperImage;

public struct Pixel
{
    public int X { get; set; }
    public int Y { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; } = 255;

    public Pixel(int x, int y, byte r, byte g, byte b, byte a)
    {
        X = x;
        Y = y;
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public byte[] Cmyk()
    {
        var rPrime = R / 255.0;
        var gPrime = G / 255.0;
        var bPrime = B / 255.0;
        var k = 1 - Math.Max(rPrime, Math.Max(gPrime, bPrime));
        var c = (1 - rPrime - k) / (1 - k);
        var m = (1 - gPrime - k) / (1 - k);
        var y = (1 - bPrime - k) / (1 - k);
        return new[] {(byte) (c * 255), (byte) (m * 255), (byte) (y * 255), (byte) (k * 255)};
    }
    
    public override bool Equals(object? obj) => obj is Pixel other && this.Equals(other);
    public bool Equals(Pixel p) => R == p.R && G == p.G && B == p.B && A == p.A;
    public override int GetHashCode() => (X, Y).GetHashCode();
    public static bool operator ==(Pixel lhs, Pixel rhs) => lhs.Equals(rhs);
    public static bool operator !=(Pixel lhs, Pixel rhs) => !(lhs == rhs);
}