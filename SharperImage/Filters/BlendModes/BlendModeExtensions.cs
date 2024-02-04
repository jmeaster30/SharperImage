using SharperImage.Enumerators;
using SharperImage.Enumerators.BlendModes;

namespace SharperImage.Filters.BlendModes;

public static class BlendModeExtensions
{
    public static IPixelEnumerable Blend(this IPixelEnumerable a, IPixelEnumerable b, BlendMode mode)
    {
        return a.Blend(b, mode switch
        {
            BlendMode.ADD => (a, b) => a.Add(b),
            BlendMode.AND => (a, b) => a.And(b),
            BlendMode.CHANNEL_DISSOLVE => (a, b) => a.ChannelDissolve(b),
            BlendMode.COLOR => (a, b) => a.ColorBlend(b),
            BlendMode.COLOR_BURN => (a, b) => a.ColorBurn(b),
            BlendMode.COLOR_DODGE => (a, b) => a.ColorDodge(b),
            BlendMode.DARKEN => (a, b) => a.Darken(b),
            BlendMode.DIFFERENCE => (a, b) => a.Difference(b),
            BlendMode.DISSOLVE => (a, b) => a.Dissolve(b),
            BlendMode.DIVIDE => (a, b) => a.Divide(b),
            BlendMode.EXCLUSION => (a, b) => a.Exclusion(b),
            BlendMode.HARD_LIGHT => (a, b) => a.HardLight(b),
            BlendMode.HUE => (a, b) => a.Hue(b),
            BlendMode.LIGHTEN => (a, b) => a.Lighten(b),
            BlendMode.LINEAR_BURN => (a, b) => a.LinearBurn(b),
            BlendMode.LUMINOSITY => (a, b) => a.Luminosity(b),
            BlendMode.MULTIPLY => (a, b) => a.Multiply(b),
            BlendMode.NAND => (a, b) => a.Nand(b),
            BlendMode.NORMAL => (a, b) => a.Normal(b),
            BlendMode.OR => (a, b) => a.Or(b),
            BlendMode.OVERLAY => (a, b) => a.Overlay(b),
            BlendMode.PLUS_DARKER => (a, b) => a.PlusDarker(b),
            BlendMode.PLUS_LIGHTER => (a, b) => a.PlusLighter(b),
            BlendMode.SATURATION => (a, b) => a.Saturation(b),
            BlendMode.SCREEN => (a, b) => a.Screen(b),
            BlendMode.SOFT_LIGHT => (a, b) => a.SoftLight(b),
            BlendMode.SUBTRACT => (a, b) => a.Subtract(b),
            BlendMode.XOR => (a, b) => a.Xor(b),
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    public static IPixelEnumerable Blend(this (IPixelEnumerable, IPixelEnumerable) a, BlendMode mode)
    {
        return a.Item1.Blend(a.Item2, mode);
    }
}