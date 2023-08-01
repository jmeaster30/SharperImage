// See https://aka.ms/new-console-template for more information

using SharperImage.Filters;
using SharperImage.Formats;
using SharperImage.Viewer;

using var imageFile = File.OpenRead("../../../images/qoi/dice.qoi");
var image = FileFormat.QOI.GetFormatter().Decode(imageFile).Grayscale().Invert();
Viewer.Open(image);
