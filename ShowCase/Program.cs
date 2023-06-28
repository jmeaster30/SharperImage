// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using SharperImage.ImageLoaders;

var result = GifLoader.Load("../../../images/gif/bird.gif");
Console.WriteLine(JsonConvert.SerializeObject(result));