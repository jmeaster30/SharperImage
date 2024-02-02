using System;
using System.Collections.Generic;
using Cairo;
using Gdk;
using Gtk;
using SharperImage.Enumerators;
using SharperImage.Filters;
using SharperImage.Formats;
using Application = Gtk.Application;
using EventMotion = Gdk.EventMotion;
using Window = Gtk.Window;

namespace SharperImage.Viewer
{
    class MainWindow : Window
    {
        private ImageArea MyImageArea;

        private Label HoverPixelX;
        private Label HoverPixelY;
        private Label HoverPixelColorRed;
        private Label HoverPixelColorGreen;
        private Label HoverPixelColorBlue;
        private Label HoverPixelColorAlpha;
        
        public MainWindow(string filename, FileFormat format) : this(Image.Decode(filename, format)) { }
        public MainWindow(Image image)  : base("SharperImage.Viewer")
        {
            DeleteEvent += delegate { Application.Quit(); };
            SetDefaultSize(800, 600);
            
            Console.WriteLine($"Image {image.Width} x {image.Height}");

            MyImageArea = new ImageArea(image);
            MyImageArea.AddEvents((int)EventMask.PointerMotionMask);
            MyImageArea.MouseMoveAction += delegate(EventMotion motion)
            {
                var x = (int)Math.Clamp(motion.X, 0, image.Width - 1);
                var y = (int)Math.Clamp(motion.Y, 0, image.Height - 1);
                try
                {
                    var pix = image.PixelData[x, y];
                    HoverPixelX.Text = $"X: {x}";
                    HoverPixelY.Text = $"Y: {y}";
                    HoverPixelColorRed.Text = $"Red: {pix.Color.Red}";
                    HoverPixelColorGreen.Text = $"Green: {pix.Color.Green}";
                    HoverPixelColorBlue.Text = $"Blue: {pix.Color.Blue}";
                    HoverPixelColorAlpha.Text = $"Alpha: {pix.Color.Alpha}";
                }
                catch
                {
                    Console.WriteLine($"X {x} Y {y}");
                    throw;
                }
            };
            
            var swin = new ScrolledWindow();
            swin.Add(MyImageArea);
            
            var widthLabel = new Label($"Width: {image.Width}");
            var heightLabel = new Label($"Height {image.Height}");
 
            HoverPixelX = new Label("X:");
            HoverPixelY = new Label("Y:");
            HoverPixelColorRed = new Label("Red:");
            HoverPixelColorGreen = new Label("Green:");
            HoverPixelColorBlue = new Label("Blue:");
            HoverPixelColorAlpha = new Label("Alpha:");

            var vboxLabels = new VBox(false, 0);
            vboxLabels.Add(widthLabel);
            vboxLabels.Add(heightLabel);
            vboxLabels.Add(HoverPixelX);
            vboxLabels.Add(HoverPixelY);
            vboxLabels.Add(HoverPixelColorRed);
            vboxLabels.Add(HoverPixelColorGreen);
            vboxLabels.Add(HoverPixelColorBlue);
            vboxLabels.Add(HoverPixelColorAlpha);
            
            var hbox = new HBox(false, 0);
            hbox.Add(vboxLabels);

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
            MyImageArea.SetBackground((byte)((HScale)o).Value, null, null, null);
        }
        
        private void GreenScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            MyImageArea.SetBackground(null, (byte)((HScale)o).Value, null, null);
        }
        
        private void BlueScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            MyImageArea.SetBackground(null, null, (byte)((HScale)o).Value, null);
        }
        
        private void AlphaScaleOnChangeValue(object o, ChangeValueArgs args)
        {
            MyImageArea.SetBackground(null, null, null, (byte)((HScale)o).Value);
        }

        private class ImageArea : DrawingArea
        {
            private Image _image;

            private Color _background = new Color { Red = 0, Green = 0, Blue = 0, Alpha = 255 };

            public Action<EventMotion> MouseMoveAction { get; set; }
            public bool Grayscale { get; set; } = true;
            public bool Invert { get; set; } = false;

            public ImageArea(Image image)
            {
                _image = image;
            }

            protected override bool OnMotionNotifyEvent(EventMotion ev)
            {
                MouseMoveAction(ev);
                return true;
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
                IEnumerable<Pixel> pixels = _image.ToPixelEnumerable();
                if (Grayscale)
                {
                    pixels = pixels.Grayscale();
                } 
                if (Invert)
                {
                    pixels = pixels.Invert();
                } 
                    
                foreach (var pixel in pixels)
                {
                    var alpha = pixel.Color.Alpha + (_background.Alpha * (255 - pixel.Color.Alpha) / 255);
                    var red = (pixel.Color.Red * pixel.Color.Alpha + _background.Red * _background.Alpha * (255 - pixel.Color.Alpha) / 255) / alpha;
                    var green = (pixel.Color.Green * pixel.Color.Alpha + _background.Green * _background.Alpha * (255 - pixel.Color.Alpha) / 255) / alpha;
                    var blue = (pixel.Color.Blue * pixel.Color.Alpha + _background.Blue * _background.Alpha * (255 - pixel.Color.Alpha) / 255) / alpha;
                    
                    c.SetSourceRGBA(red / 255.0, green / 255.0, blue / 255.0, alpha / 255.0);
                    c.Rectangle(pixel.X, pixel.Y, 1, 1);
                    c.Fill();
                }
                return true;
            }
        }
    }
}