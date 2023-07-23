using System;
using System.IO;
using Cairo;
using GLib;
using Gtk;
using SharperImage.Enumerators;
using SharperImage.Formats;
using Application = Gtk.Application;

namespace SharperImage.Viewer
{
    class MainWindow : Window
    {
        public MainWindow(string filename, FileFormat format) : this(IImage.Decode(filename, format)) { }
        public MainWindow(IImage image)  : base("SharperImage.Viewer")
        {
            DeleteEvent += delegate { Application.Quit(); };
            SetDefaultSize(800, 600);
            
            Console.WriteLine($"Image {image.Width()} x {image.Height()}");
            
            var drawingArea = new ImageArea(image);
            Add(drawingArea);
            drawingArea.ShowNow();
        }

        private class ImageArea : DrawingArea
        {
            private IImage _image;

            public ImageArea(IImage image)
            {
                _image = image;
            }

            protected override bool OnDrawn(Context c)
            {
                Console.WriteLine("Drawing Image :)");
                foreach (var pixel in _image.ToRowRankPixelEnumerable())
                {
                    c.SetSourceRGBA(pixel.Red / 255.0, pixel.Green / 255.0, pixel.Blue / 255.0, pixel.Alpha / 255.0);
                    c.Rectangle(pixel.X, pixel.Y, 1, 1);
                    c.Fill();
                }
                return true;
            }
        }
    }
}