namespace SharperImage;

public struct Color
{
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public double Alpha { get; set; } = 1.0;

    public byte RedByte => (byte)(Red * 255.0);
    public byte GreenByte => (byte)(Green * 255.0);
    public byte BlueByte => (byte)(Blue * 255.0);
    public byte AlphaByte => (byte)(Alpha * 255.0);
    
    public Color(byte red, byte green, byte blue, byte alpha)
    {
        Red = red / 255.0;
        Green = green / 255.0;
        Blue = blue / 255.0;
        Alpha = alpha / 255.0;
    }
    
    public Color(double red, double green, double blue, double alpha)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    public double Hue() {
        var xmax = Math.Max(Math.Max(Red, Green), Blue);
        var xmin = Math.Min(Math.Min(Red, Green), Blue);
        var chroma = xmax - xmin;
        if (chroma == 0) return 0;
        if (xmax == Red) return 60 * ((Green - Blue) / chroma % 6);
        if (xmax == Green) return 60 * ((Blue - Red) / chroma + 2);
        // xmax == blue
        return 60 * ((Red - Green) / chroma + 4);
    }
    
    public double Saturation()
    {
        if (Luminosity() is 0 or 1) return 0;
        var xmax = Math.Max(Math.Max(Red, Green), Blue);
        var xmin = Math.Min(Math.Min(Red, Green), Blue);
        var chroma = xmax - xmin;
        return chroma / (1 - Math.Abs(2 * xmax - chroma - 1));
    }
    
    public double Luminosity()
    {
        var xmax = Math.Max(Math.Max(Red, Green), Blue);
        var xmin = Math.Min(Math.Min(Red, Green), Blue);
        return (xmax + xmin) / 2.0;
    }

    public byte[] Cmyk()
    {
        var rPrime = Red;
        var gPrime = Green;
        var bPrime = Blue;
        var k = 1 - Math.Max(rPrime, Math.Max(gPrime, bPrime));
        var c = (1 - rPrime - k) / (1 - k);
        var m = (1 - gPrime - k) / (1 - k);
        var y = (1 - bPrime - k) / (1 - k);
        return new[] {(byte) (c * 255), (byte) (m * 255), (byte) (y * 255), (byte) (k * 255)};
    }

    public byte[] Rgba() => new[]
        { (byte)(Red * 255.0), (byte)(Green * 255.0), (byte)(Blue * 255.0), (byte)(Alpha * 255.0) };


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

    public static Color operator +(Color lhs, Color rhs) => new(
        lhs.Red + rhs.Red, lhs.Green + rhs.Green,
        lhs.Blue + rhs.Blue, lhs.Alpha + rhs.Alpha);
    public static Color operator -(Color lhs, Color rhs) => new(
        lhs.Red - rhs.Red, lhs.Green - rhs.Green,
        lhs.Blue - rhs.Blue, lhs.Alpha - rhs.Alpha);
    public static Color operator /(Color lhs, double rhs) =>
        new(lhs.Red / rhs, lhs.Green / rhs, lhs.Blue / rhs, lhs.Alpha / rhs);
    public static Color operator *(Color lhs, double rhs) =>
        new(lhs.Red * rhs, lhs.Green * rhs, lhs.Blue * rhs, lhs.Alpha * rhs);
    
    public static Color CLEAR = new(0, 0, 0, 0);
    public static Color BLACK = new(0, 0, 0, 255);
    public static Color RED = new(255, 0, 0, 255);
    public static Color GREEN = new(78, 154, 6, 255);
    public static Color YELLOW = new(196, 160, 0, 255);
    public static Color BLUE = new(114, 159, 207, 255);
    public static Color MAGENTA = new(117, 80, 123, 255);
    public static Color CYAN = new(6, 152, 154, 255);
    public static Color WHITE = new(211, 215, 207, 255);
    public static Color BRIGHT_BLACK = new(85, 87, 83, 255);
    public static Color BRIGHT_RED = new(239, 41, 41, 255);
    public static Color BRIGHT_GREEN = new(138, 226, 52, 255);
    public static Color BRIGHT_YELLOW = new(252, 233, 79, 255);
    public static Color BRIGHT_BLUE = new(50, 175, 255, 255);
    public static Color BRIGHT_MAGENTA = new(173, 127, 168, 255);
    public static Color BRIGHT_CYAN = new(52, 226, 226, 255);
    public static Color BRIGHT_WHITE = new(255, 255, 255, 255);

    public double Distance(Color other)
    {
        return Math.Sqrt(DistanceSquared(other));
    }
    
    public double DistanceSquared(Color other)
    {
        var dr = Red - other.Red;
        var dg = Green - other.Green;
        var db = Blue - other.Blue;
        var da = Alpha - other.Alpha;
        return dr * dr + dg * dg + db * db + da * da;
    }
    
    public static Color Interpolate(Color from, Color to, double percent)
    {
        var dr = from.Red - to.Red;
        var dg = from.Green - to.Green;
        var db = from.Blue - to.Blue;
        var da = from.Alpha - to.Alpha;
        return new Color(from.Red + dr * percent, from.Green + dg * percent, from.Blue + db * percent, from.Alpha + da * percent);
    }

    public Color Grayscale()
    {
        var value = (byte) (0.299 * Red + 0.587 * Green + 0.114 * Blue);
        return Transform(_ => value);
    }

    public Color Invert()
    {
        return Transform(x => 1 - x);
    }
    
    public Color Combine(Color other, Func<double, double, double> func)
    {
        return new Color(
            func(Red, other.Red), 
            func(Green, other.Green), 
            func(Blue, other.Blue), 
            Alpha);
    }
    
    public Color CombineByte(Color other, Func<byte, byte, byte> func)
    {
        return new Color(
            func(RedByte, other.RedByte), 
            func(GreenByte, other.GreenByte), 
            func(BlueByte, other.BlueByte), 
            func(AlphaByte, other.AlphaByte));
    }

    public Color TransformWithAlpha(Func<double, double> func)
    {
        return new Color(
            func(Red), 
            func(Green), 
            func(Blue), 
            func(Alpha));
    }
    
    public Color Transform(Func<double, double> func)
    {
        return new Color(
            func(Red), 
            func(Green), 
            func(Blue), 
            Alpha);
    }

    public Color Composite(Color other)
    {
        var alpha = other.Alpha + Alpha * (1 - other.Alpha);
        var red = (other.Red * other.Alpha + Red * Alpha * (1 - other.Alpha)) / alpha;
        var green = (other.Green * other.Alpha + Green * Alpha * (1 - other.Alpha)) / alpha;
        var blue = (other.Blue * other.Alpha + Blue * Alpha * (1 - other.Alpha)) / alpha;
        return new Color(red, green, blue, alpha);
    }
    
    public Color Add(Color other) => Combine(other, (a, b) => a + b);

    public Color And(Color other) => CombineByte(other, (a, b) => (byte)(a & b));

    public Color ChannelDissolve(Color other) => Combine(other, (a, b) =>
    {
        var rnd = new Random();
        return rnd.NextDouble() < 0.5 ? a : b;
    });
    
    public Color ColorBlend(Color other) => HSLA(other.Hue(), other.Saturation(), Luminosity(), Alpha);
    
    public Color ColorBurn(Color other) => Combine(other, (a, b) => 1 - (1 - a) / b);

    public Color ColorDodge(Color other) => Combine(other, (a, b) => a / (1 - b));

    public Color Darken(Color other) => Combine(other, (a, b) => Math.Min(a, b));
    
    public Color Difference(Color other) => Combine(other, (a, b) => Math.Abs(a - b));

    public Color Dissolve(Color other)
    {
        var rnd = new Random();
        return rnd.NextDouble() < 0.5 ? this : other;
    }

    public Color Divide(Color other) => Combine(other, (a, b) => a / b);

    public Color Exclusion(Color other) => Combine(other, (a, b) => a + b - 2 * a * b);

    public Color HardLight(Color other) => Combine(other, (a, b) => b < 0.5 ? 2 * a * b : 1 - 2 * (1 - a) * (1 - b));
    
    public Color Hue(Color other) => HSLA(other.Hue(), Saturation(), Luminosity(), Alpha);
    
    public Color Lighten(Color other) => Combine(other, (a, b) => Math.Max(a, b));

    public Color LinearBurn(Color other) => Combine(other, (a, b) => 1 - (1 - a + (1 - b)));
    
    public Color Luminosity(Color other) => HSLA(Hue(), Saturation(), other.Luminosity(), Alpha);

    public Color Multiply(Color other) => Combine(other, (a, b) => a * b);
    
    public Color Nand(Color other) => CombineByte(other, (a, b) => (byte)~(a & b));

    public Color Normal(Color other) => Composite(other);
    
    public Color Or(Color other) => CombineByte(other, (a, b) => (byte)(a | b));

    public Color Overlay(Color other) =>
        Combine(other, (a, b) => a < 0.5 ? 2 * a * b : 1 - 2 * (1 - a) * (1 - b));

    public Color PlusDarker(Color other) => Darken(other).Add(Multiply(other));
    
    public Color PlusLighter(Color other) => Lighten(other).Add(Screen(other));

    public Color Saturation(Color other) => HSLA(Hue(), other.Saturation(), Luminosity(), Alpha);
    
    public Color Screen(Color other) => Combine(other, (a, b) => 1 - (1 - a) * (1 - b));

    public Color SoftLight(Color other) => Combine(other,
        (a, b) => b < 0.5 ? 2 * a * b + a * a * (1 - 2 * b) : Math.Sqrt(a) * (2 * b - 1) + 2 * a * (1 - b));

    public Color Subtract(Color other) => Combine(other, (a, b) => a - b);
    
    public Color Xor(Color other) => CombineByte(other, (a, b) => (byte)(a ^ b));
}