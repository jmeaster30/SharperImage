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

    public double Hue() {
        var xmax = Math.Max(Math.Max(Red, Green), Blue) / 255.0;
        var xmin = Math.Min(Math.Min(Red, Green), Blue) / 255.0;
        var chroma = xmax - xmin;
        if (chroma == 0) return 0;
        if (xmax == Red / 255.0) return 60 * ((Green / 255.0 - Blue / 255.0) / chroma % 6);
        if (xmax == Green / 255.0) return 60 * ((Blue / 255.0 - Red / 255.0) / chroma + 2);
        // xmax == blue
        return 60 * ((Red / 255.0 - Green / 255.0) / chroma + 4);
    }
    
    public double Saturation()
    {
        if (Luminosity() is 0 or 1) return 0;
        var xmax = Math.Max(Math.Max(Red, Green), Blue) / 255.0;
        var xmin = Math.Min(Math.Min(Red, Green), Blue) / 255.0;
        var chroma = xmax - xmin;
        return chroma / (1 - Math.Abs(2 * xmax - chroma - 1));
    }
    
    public double Luminosity() {
        var xmax = Math.Max(Math.Max(Red, Green), Blue) / 255.0;
        var xmin = Math.Min(Math.Min(Red, Green), Blue) / 255.0;
        return (xmax + xmin) / 2.0;
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

    public static Color HSLA(double hue, double saturation, double luminosity, double alpha)
    {
        var chroma = (1 - Math.Abs(2 * luminosity - 1)) * saturation;
        var hprime = hue % 360 / 60;
        var x = chroma * (1 - Math.Abs(hprime % 2 - 1));
        var (r1, g1, b1) = hprime switch
        {
            >= 0 and < 1 => (chroma, x, 0.0),
            >= 1 and < 2 => (x, chroma, 0.0),
            >= 2 and < 3 => (0.0, chroma, x),
            >= 3 and < 4 => (0.0, x, chroma),
            >= 4 and < 5 => (x, 0.0, chroma),
            >= 5 and < 6 => (chroma, 0.0, x),
            _ => (chroma, 0.0, x),
        };
        var m = luminosity - chroma / 2;
        return new Color((byte)((r1 + m) * 255), (byte)((g1 + m) * 255), (byte)((b1 + m) * 255), (byte)(alpha * 255));
    }
    
    public override bool Equals(object? obj) => obj is Color other && this.Equals(other);
    public bool Equals(Color p) => Red == p.Red && Green == p.Green && Blue == p.Blue && Alpha == p.Alpha;
    public override int GetHashCode() => (Red, Green, Blue, Alpha).GetHashCode();
    public static bool operator ==(Color lhs, Color rhs) => lhs.Equals(rhs);
    public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);

    public static Color Clear = new(0, 0, 0, 0);
    public static Color Black = new(0, 0, 0, 255);
    public static Color White = new(255, 255, 255, 255);

    public Color Grayscale()
    {
        var value = (byte) (0.299 * Red + 0.587 * Green + 0.114 * Blue);
        return Transform(_ => value);
    }

    public Color Invert()
    {
        return Transform(x => 255 - x);
    }
    
    public Color Combine(Color other, Func<byte, byte, int> func)
    {
        return new Color(
            (byte)func(Red, other.Red), 
            (byte)func(Green, other.Green), 
            (byte)func(Blue, other.Blue), 
            (byte)func(Alpha, other.Alpha));
    }

    public Color TransformWithAlpha(Func<byte, int> func)
    {
        return new Color(
            (byte)func(Red), 
            (byte)func(Green), 
            (byte)func(Blue), 
            (byte)func(Alpha));
    }
    
    public Color Transform(Func<byte, int> func)
    {
        return new Color(
            (byte)func(Red), 
            (byte)func(Green), 
            (byte)func(Blue), 
            Alpha);
    }
    
    public Color Add(Color other) => Combine(other, (a, b) => a + b);
    
    public Color ColorBlend(Color other) => HSLA(other.Hue(), other.Saturation(), Luminosity(), Alpha);
    
    public Color ColorBurn(Color other) => Combine(other, (a, b) => 255 - (255 - a) / b);

    public Color ColorDodge(Color other) => Combine(other, (a, b) => a / (255 - b));

    public Color Darken(Color other) => Combine(other, (a, b) => a < b ? a : b);
    
    public Color Difference(Color other) => Combine(other, (a, b) => Math.Abs(a - b));

    public Color Exclusion(Color other) => Combine(other, (a, b) => a + b - 2 * a * b);

    public Color Hue(Color other) => HSLA(other.Hue(), Saturation(), Luminosity(), Alpha / 255.0);
    
    public Color Lighten(Color other) => Combine(other, (a, b) => a > b ? a : b);
    
    public Color Luminosity(Color other) => HSLA(Hue(), Saturation(), other.Luminosity(), Alpha);

    public Color Multiply(Color other) => Combine(other, (a, b) => a * b);

    public Color Normal(Color other)
    {
        var alpha = Alpha + other.Alpha * (255 - Alpha) / 255;
        var red = (Red * Alpha + other.Red * other.Alpha * (255 - Alpha) / 255) / alpha;
        var green = (Green * Alpha + other.Green * other.Alpha * (255 - Alpha) / 255) / alpha;
        var blue = (Blue * Alpha + other.Blue * other.Alpha * (255 - Alpha) / 255) / alpha;
        return new Color((byte)red, (byte)green, (byte)blue, (byte)alpha);
    }

    public Color Overlay(Color other) =>
        Combine(other, (a, b) => a < 255 / 2 ? 2 * a * b : 255 - 2 * (255 - a) * (255 - b));

    public Color PlusDarker(Color other) => Darken(other).Add(Multiply(other));
    
    public Color PlusLighter(Color other) => Lighten(other).Add(Screen(other));

    public Color Saturation(Color other) => HSLA(Hue(), other.Saturation(), Luminosity(), Alpha);
    
    public Color Screen(Color other) => Combine(other, (a, b) => 255 - (255 - a) * (255 - b));

    public Color SoftLight(Color other) => Combine(other,
        (a, b) => b < 255 / 2 ? 2 * a * b + a * a * (255 - 2 * b) : (int)(Math.Sqrt(a) * (2 * b - 255)) + 2 * a * (255 - b));

    public Color Subtract(Color other) => Combine(other, (a, b) => a - b);
}