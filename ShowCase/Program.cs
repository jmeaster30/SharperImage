// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using SharperImage;
using SharperImage.Enumerators;
using SharperImage.Formats;

using var imageFile = File.OpenRead("../../../images/qoi/testcard_rgba.qoi");
var decoded1 = QoiImage.LoadImage(imageFile);

Console.WriteLine("Decoded base file");

using var encodedStream1 = new MemoryStream();
decoded1.Encode(encodedStream1);

Console.WriteLine("Encoded to memory stream");

var decoded2 = QoiImage.LoadImage(encodedStream1);

Console.WriteLine("Decoded memory stream");

var d1pixels = decoded1.ToRowRankPixelEnumerable();
var d2pixels = decoded2.ToRowRankPixelEnumerable();

Console.WriteLine("Got enumerables");

var d1size = d1pixels.Count;
Console.WriteLine($"D1 size {d1size}");
var d2size = d2pixels.Count;
Console.WriteLine($"D2 size {d2size}");

if (d1size != d2size)
{
    Console.WriteLine("The decoded images have different amounts of pixels");
}

var match = -1;
for (int i = 0; i < d1size && i < d2size; i++)
{
    var p1 = d1pixels[i];
    var p2 = d2pixels[i];

    if (p1 != p2)
    {
        match = i;
    }
}

if (match == -1)
{
    Console.WriteLine("THE IMAGES DO MATCH :)");
}
else
{
    Console.WriteLine("THE IMAGES DO NOT MATCH");
    Console.WriteLine(match);
    Console.WriteLine($"D1 R: {d1pixels[match].Red} G: {d1pixels[match].Green} B: {d1pixels[match].Blue} A: {d1pixels[match].Alpha}");
    Console.WriteLine($"D2 R: {d2pixels[match].Red} G: {d2pixels[match].Green} B: {d2pixels[match].Blue} A: {d2pixels[match].Alpha}");
}