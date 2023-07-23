using System;
using System.IO;
using SharperImage.Exceptions;
using SharperImage.Formats;
using Xunit;

namespace SharperImage.Test;

public class PngImageTest
{
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