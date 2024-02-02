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
            var format = args[0][^3..] switch
            {
                "png" => FileFormat.PNG,
                "bmp" => FileFormat.BMP,
                "qoi" => FileFormat.QOI,
                "gif" => FileFormat.GIF,
                _ => throw new NotImplementedException(),
            };
            Viewer.Open(filename, format);
        }
    }
}