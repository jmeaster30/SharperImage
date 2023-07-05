// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using SharperImage.Formats;

using var imageFile = File.OpenRead("../../../images/bmp/cat.bmp");

var result = BitmapImage.LoadImage(imageFile);
Console.WriteLine(result.Width());
Console.WriteLine(result.Height());
Console.WriteLine(JsonConvert.SerializeObject(result.PixelArray()));