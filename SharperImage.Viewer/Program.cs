using System;
using Gtk;
using SharperImage.Formats;

namespace SharperImage.Viewer
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var filename = args[0];
            var format = args[1] switch
            {
                "png" => FileFormat.PNG,
                "bmp" => FileFormat.BMP,
                "qoi" => FileFormat.QOI,
                "unformatted" => FileFormat.UNFORMATED,
                "gif" => FileFormat.GIF,
                _ => throw new NotImplementedException(),
            };
            Viewer.Open(filename, format);
        }
    }
}