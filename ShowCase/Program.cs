// See https://aka.ms/new-console-template for more information

using SharperImage;
using SharperImage.Enumerators;
using SharperImage.Enumerators.Drawing;
using SharperImage.Formats;
using SharperImage.Viewer;

var loadWatch = System.Diagnostics.Stopwatch.StartNew();

using var imageFile1 = File.OpenRead("images/qoi/dice.qoi");
//using var imageFile2 = File.OpenRead("images/png/PNG_Test.png");
var diceImage = FileFormat.QOI.GetFormatter().Decode(imageFile1).ToPixelEnumerable();
//var image = FileFormat.PNG.GetFormatter().Decode(imageFile2).ToPixelEnumerable();

loadWatch.Stop();
var loadWatchElapsedMs = loadWatch.ElapsedMilliseconds;
Console.WriteLine($"Decoded image in {loadWatchElapsedMs / 1000.0} sec");

var renderWatch = System.Diagnostics.Stopwatch.StartNew();

var enumerable = diceImage
    .DrawRectangle(200, 200, 100, 100, new Color(1.0, 1.0, 1.0, 0.5));

for (double a = 0; a < Math.PI / 2; a += 0.1)
{
    enumerable = enumerable.DrawLine(100, 100, 200, a, Color.White, 1);
}
    
var result = enumerable.ToImage();

renderWatch.Stop();
var renderWatchElapsedMs = renderWatch.ElapsedMilliseconds;
Console.WriteLine($"Rendered image in {renderWatchElapsedMs / 1000.0} sec");

Viewer.Open(result);
