// See https://aka.ms/new-console-template for more information

using SharperImage;
using SharperImage.Enumerators;
using SharperImage.Enumerators.Drawing;
using SharperImage.Filters.Artsy;
using SharperImage.Formats;
using SharperImage.Viewer;

var loadWatch = System.Diagnostics.Stopwatch.StartNew();

using var imageFile1 = File.OpenRead("images/qoi/kodim23.qoi");
//using var imageFile2 = File.OpenRead("images/png/PNG_Test.png");
var diceImage = FileFormat.QOI.GetFormatter().Decode(imageFile1).ToPixelEnumerable();
//var image = FileFormat.PNG.GetFormatter().Decode(imageFile2).ToPixelEnumerable();

loadWatch.Stop();
var loadWatchElapsedMs = loadWatch.ElapsedMilliseconds;
Console.WriteLine($"Decoded image in {loadWatchElapsedMs / 1000.0} sec");

var renderWatch = System.Diagnostics.Stopwatch.StartNew();

var enumerable = diceImage
    .PixelSort(color => color.Distance(Color.RED) < 0.75,
        (a, b) => a.Red.CompareTo(b.Red),
        PixelSortDirection.VERTICAL);
    
var result = enumerable.ToImage();

renderWatch.Stop();
var renderWatchElapsedMs = renderWatch.ElapsedMilliseconds;
Console.WriteLine($"Rendered image in {renderWatchElapsedMs / 1000.0} sec");

Viewer.Open(result);
