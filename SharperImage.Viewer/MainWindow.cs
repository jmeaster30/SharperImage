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
        private ImageArea _imageArea;
        
        public MainWindow(string filename, FileFormat format) : this(IImage.Decode(filename, format)) { }
        public MainWindow(IImage image)  : base("SharperImage.Viewer")
        {
            DeleteEvent += delegate { Application.Quit(); };
            SetDefaultSize(800, 600);
            
            Console.WriteLine($"Image {image.Width()} x {image.Height()}");

            _imageArea = new ImageArea(image);
            
            var swin = new ScrolledWindow();
            swin.Add(_imageArea);
            
            var label = new Label($"Width: {image.Width()} Height {image.Height()}");

            var hbox = new HBox(false, 0);
            hbox.Add(label);

            var backgroundSliders = new VBox(false, 0);
            var redSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var redScale = new HScale(redSlider);
            redScale.ChangeValue += RedScaleOnChangeValue;
            var greenSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var greenScale = new HScale(greenSlider);
            greenScale.ChangeValue += GreenScaleOnChangeValue;
            var blueSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var blueScale = new HScale(blueSlider);
            blueScale.ChangeValue += BlueScaleOnChangeValue;
            backgroundSliders.Add(redScale);
            backgroundSliders.Add(greenScale);
            backgroundSliders.Add(blueScale);
            hbox.Add(backgroundSliders);
            
            var vbox = new VPaned();
            vbox.BorderWidth = 12;
            vbox.Position = 900;
            vbox.Pack1(swin, false, false);
            vbox.Pack2(hbox, false, false);
            Add(vbox);
            ShowAll();
        }

        private void RedScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            _imageArea.SetBackground((byte)((HScale)o).Value, null, null, null);
        }
        
        private void GreenScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            _imageArea.SetBackground(null, (byte)((HScale)o).Value, null, null);
        }
        
        private void BlueScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            _imageArea.SetBackground(null, null, (byte)((HScale)o).Value, null);
        }
        
        private void AlphaScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            _imageArea.SetBackground(null, null, null, (byte)((HScale)o).Value);
        }

        private class ImageArea : DrawingArea
        {
            private IImage _image;

            private Pixel _background = new Pixel { Red = 0, Green = 0, Blue = 0, Alpha = 255 };
            
            public ImageArea(IImage image)
            {
                _image = image;
            }

            public void SetBackground(byte? red, byte? green, byte? blue, byte? alpha)
            {
                _background.Red = red ?? _background.Red;
                _background.Green = green ?? _background.Green;
                _background.Blue = blue ?? _background.Blue;
                _background.Alpha = alpha ?? _background.Alpha;
                QueueDraw();
            }

            protected override bool OnDrawn(Context c)
            {
                // TODO reduce calls to this function
                foreach (var pixel in _image.ToRowRankPixelEnumerable())
                {
                    var alpha = pixel.Alpha + (_background.Alpha * (255 - pixel.Alpha) / 255);
                    var red = (pixel.Red * pixel.Alpha + _background.Red * _background.Alpha * (255 - pixel.Alpha) / 255) / alpha;
                    var green = (pixel.Green * pixel.Alpha + _background.Green * _background.Alpha * (255 - pixel.Alpha) / 255) / alpha;
                    var blue = (pixel.Blue * pixel.Alpha + _background.Blue * _background.Alpha * (255 - pixel.Alpha) / 255) / alpha;
                    
                    c.SetSourceRGBA(red / 255.0, green / 255.0, blue / 255.0, alpha / 255.0);
                    c.Rectangle(pixel.X, pixel.Y, 1, 1);
                    c.Fill();
                }
                return true;
            }
        }
    }
}