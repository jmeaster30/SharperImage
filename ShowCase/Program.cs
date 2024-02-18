// See https://aka.ms/new-console-template for more information

using SharperImage;
using SharperImage.Enumerators;
using SharperImage.Enumerators.Drawing;
using SharperImage.Enumerators.Transform;
using SharperImage.Filters.Artsy;
using SharperImage.Filters.Smoothing;
using SharperImage.Formats;
using SharperImage.Viewer;

var loadWatch = System.Diagnostics.Stopwatch.StartNew();

using var imageFile1 = File.OpenRead("images/qoi/Artesonraju3.qoi");
//using var imageFile2 = File.OpenRead("images/png/PNG_Test.png");
var diceImage = FileFormat.QOI.GetFormatter().Decode(imageFile1).ToPixelEnumerable();
//var image = FileFormat.PNG.GetFormatter().Decode(imageFile2).ToPixelEnumerable();

loadWatch.Stop();
var loadWatchElapsedMs = loadWatch.ElapsedMilliseconds;
Console.WriteLine($"Decoded image in {loadWatchElapsedMs / 1000.0} sec");

var renderWatch = System.Diagnostics.Stopwatch.StartNew();

var enumerable = diceImage
    .Scale(diceImage.GetWidth() / 2, diceImage.GetHeight() / 2)
    .Kuwahara(31)
    .PixelSort(color => color.Luma() < 0.75,
        (a, b) => a.Luma().CompareTo(b.Luma()),
        PixelSortDirection.VERTICAL);
    
var result = enumerable.ToImage(true);

renderWatch.Stop();
var renderWatchElapsedMs = renderWatch.ElapsedMilliseconds;
Console.WriteLine($"Rendered image in {renderWatchElapsedMs / 1000.0} sec");

Viewer.Open(result);
