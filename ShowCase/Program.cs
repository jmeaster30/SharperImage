// See https://aka.ms/new-console-template for more information

using SharperImage.Enumerators;
using SharperImage.Filters.BlendModes;
using SharperImage.Filters.Blurs;
using SharperImage.Formats;
using SharperImage.Viewer;

var loadWatch = System.Diagnostics.Stopwatch.StartNew();

//using var imageFile1 = File.OpenRead("images/qoi/dice.qoi");
using var imageFile2 = File.OpenRead("images/qoi/kodim23.qoi");
//var diceImage = FileFormat.QOI.GetFormatter().Decode(imageFile1).ToPixelEnumerable();
var kodimImage = FileFormat.QOI.GetFormatter().Decode(imageFile2).ToPixelEnumerable();

loadWatch.Stop();
var loadWatchElapsedMs = loadWatch.ElapsedMilliseconds;
Console.WriteLine($"Decoded image in {loadWatchElapsedMs / 1000.0} sec");

var renderWatch = System.Diagnostics.Stopwatch.StartNew();

var result = kodimImage.BoxBlur(20, 20);//(kodimImage, diceImage).Blend(BlendMode.XOR).Blend(diceImage, BlendMode.DIVIDE);
var final = result.ToImage();

renderWatch.Stop();
var renderWatchElapsedMs = renderWatch.ElapsedMilliseconds;
Console.WriteLine($"Rendered image in {renderWatchElapsedMs / 1000.0} sec");

Viewer.Open(final);
