using System;
using System.IO;
using SharperImage.Exceptions;
using SharperImage.Formats;
using Xunit;

namespace SharperImage.Test;

public class PngImageTest
{
    [Theory]
    [InlineData("TestCases/Png/Suite/s01i3p01.png", 1, 1)]
    [InlineData("TestCases/Png/Suite/s01n3p01.png", 1, 1)]
    [InlineData("TestCases/Png/Suite/s02i3p01.png", 2, 2)]
    [InlineData("TestCases/Png/Suite/s02n3p01.png", 2, 2)]
    [InlineData("TestCases/Png/Suite/s03i3p01.png", 3, 3)]
    [InlineData("TestCases/Png/Suite/s03n3p01.png", 3, 3)]
    [InlineData("TestCases/Png/Suite/s04i3p01.png", 4, 4)]
    [InlineData("TestCases/Png/Suite/s04n3p01.png", 4, 4)]
    [InlineData("TestCases/Png/Suite/s05i3p02.png", 5, 5)]
    [InlineData("TestCases/Png/Suite/s05n3p02.png", 5, 5)]
    [InlineData("TestCases/Png/Suite/s06i3p02.png", 6, 6)]
    [InlineData("TestCases/Png/Suite/s06n3p02.png", 6, 6)]
    [InlineData("TestCases/Png/Suite/s07i3p02.png", 7, 7)]
    [InlineData("TestCases/Png/Suite/s07n3p02.png", 7, 7)]
    [InlineData("TestCases/Png/Suite/s08i3p02.png", 8, 8)]
    [InlineData("TestCases/Png/Suite/s08n3p02.png", 8, 8)]
    public void TestValidFiles(string filename, uint width, uint height)
    {
        var imageFile = File.OpenRead(filename);
        var pngImage = new PngImage();
        pngImage.Decode(imageFile);
        
        Assert.Equal(pngImage.Width(), width);
        Assert.Equal(pngImage.Height(), height);
        
        // TODO compare the pixels???
    }
    
    [Theory]
    [InlineData("TestCases/Png/Suite/xc1n0g08.png")]
    [InlineData("TestCases/Png/Suite/xc9n2c08.png")]
    [InlineData("TestCases/Png/Suite/xcrn0g04.png")]
    [InlineData("TestCases/Png/Suite/xcsn0g01.png")]
    [InlineData("TestCases/Png/Suite/xd0n2c08.png")]
    [InlineData("TestCases/Png/Suite/xd3n2c08.png")]
    [InlineData("TestCases/Png/Suite/xd9n2c08.png")]
    [InlineData("TestCases/Png/Suite/xdtn0g01.png")]
    [InlineData("TestCases/Png/Suite/xhdn0g08.png")]
    [InlineData("TestCases/Png/Suite/xlfn0g04.png")]
    [InlineData("TestCases/Png/Suite/xs1n0g01.png")]
    [InlineData("TestCases/Png/Suite/xs2n0g01.png")]
    [InlineData("TestCases/Png/Suite/xs4n0g01.png")]
    [InlineData("TestCases/Png/Suite/xs7n0g01.png")]
    public void TestCorruptedFiles(string filename)
    {
        var imageFile = File.OpenRead(filename);
        var pngImage = new PngImage();
        Assert.Throws<ImageDecodeException>(() => pngImage.Decode(imageFile));
    }
}