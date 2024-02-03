namespace SharperImage;

public struct Color
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; } = 255;

    public Color(byte red, byte green, byte blue, byte alpha)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }
    
    public byte[] Cmyk()
    {
        var rPrime = Red / 255.0;
        var gPrime = Green / 255.0;
        var bPrime = Blue / 255.0;
        var k = 1 - Math.Max(rPrime, Math.Max(gPrime, bPrime));
        var c = (1 - rPrime - k) / (1 - k);
        var m = (1 - gPrime - k) / (1 - k);
        var y = (1 - bPrime - k) / (1 - k);
        return new[] {(byte) (c * 255), (byte) (m * 255), (byte) (y * 255), (byte) (k * 255)};
    }
    
    public override bool Equals(object? obj) => obj is Color other && this.Equals(other);
    public bool Equals(Color p) => Red == p.Red && Green == p.Green && Blue == p.Blue && Alpha == p.Alpha;
    public override int GetHashCode() => (Red, Green, Blue, Alpha).GetHashCode();
    public static bool operator ==(Color lhs, Color rhs) => lhs.Equals(rhs);
    public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);

    public static Color Clear = new(0, 0, 0, 0);
    public static Color Black = new(0, 0, 0, 255);
    public static Color White = new(255, 255, 255, 255);
}