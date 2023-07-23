// See https://aka.ms/new-console-template for more information

using SharperImage.Formats;
using SharperImage.Viewer;

using var imageFile = File.OpenRead("../../../images/png/png_suite/s01n3p01.png");
var image = PngImage.LoadImage(imageFile);
//Viewer.Open(image);
