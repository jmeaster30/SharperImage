// See https://aka.ms/new-console-template for more information

using SharperImage.Enumerators;
using SharperImage.Enumerators.BlendModes;
using SharperImage.Filters.BlendModes;
using SharperImage.Formats;
using SharperImage.Viewer;

using var imageFile1 = File.OpenRead("images/qoi/dice.qoi");
using var imageFile2 = File.OpenRead("images/qoi/kodim23.qoi");
var diceImage = FileFormat.QOI.GetFormatter().Decode(imageFile1).ToPixelEnumerable();
var kodimImage = FileFormat.QOI.GetFormatter().Decode(imageFile2).ToPixelEnumerable();
var result = (kodimImage, diceImage).Blend(BlendMode.XOR).Blend(diceImage, BlendMode.DIVIDE);
Viewer.Open(result.ToImage());
