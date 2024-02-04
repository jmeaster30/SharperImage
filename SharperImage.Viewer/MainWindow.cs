using System;
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
    internal class MainWindow : Window
    {
        private readonly ImageArea _myImageArea;

        public MainWindow(string filename, FileFormat format) : this(Image.Decode(filename, format)) { }
        public MainWindow(Image image)  : base("SharperImage.Viewer")
        {
            DeleteEvent += delegate { Application.Quit(); };
            SetDefaultSize(800, 600);
            
            Console.WriteLine($"Image {image.Width} x {image.Height}");

            var hoverPixelX = new Label("X:");
            var hoverPixelY = new Label("Y:");
            var hoverPixelColorRed = new Label("Red:");
            var hoverPixelColorGreen = new Label("Green:");
            var hoverPixelColorBlue = new Label("Blue:");
            var hoverPixelColorAlpha = new Label("Alpha:");
            
            _myImageArea = new ImageArea(image);
            _myImageArea.AddEvents((int)EventMask.PointerMotionMask);
            _myImageArea.MouseMoveAction += delegate(EventMotion motion)
            {
                var x = (int)Math.Clamp(motion.X, 0, image.Width - 1);
                var y = (int)Math.Clamp(motion.Y, 0, image.Height - 1);
                try
                {
                    var pix = image.PixelData[x, y];
                    var composite = _myImageArea.BackgroundColor.Composite(pix.Color);
                    hoverPixelX.Text = $"X: {x}";
                    hoverPixelY.Text = $"Y: {y}";
                    hoverPixelColorRed.Text = $"Red: {pix.Color.RedByte} ({composite.RedByte})";
                    hoverPixelColorGreen.Text = $"Green: {pix.Color.GreenByte} ({composite.GreenByte})";
                    hoverPixelColorBlue.Text = $"Blue: {pix.Color.BlueByte} ({composite.BlueByte})";
                    hoverPixelColorAlpha.Text = $"Alpha: {pix.Color.AlphaByte} ({composite.AlphaByte})";
                }
                catch
                {
                    Console.WriteLine($"X {x} Y {y}");
                    throw;
                }
            };
            
            var swin = new ScrolledWindow();
            swin.Add(_myImageArea);
            
            var widthLabel = new Label($"Width: {image.Width}");
            var heightLabel = new Label($"Height {image.Height}");

            var vboxLabels = new Box(Orientation.Vertical, 0);
            vboxLabels.Add(widthLabel);
            vboxLabels.Add(heightLabel);
            vboxLabels.Add(hoverPixelX);
            vboxLabels.Add(hoverPixelY);
            vboxLabels.Add(hoverPixelColorRed);
            vboxLabels.Add(hoverPixelColorGreen);
            vboxLabels.Add(hoverPixelColorBlue);
            vboxLabels.Add(hoverPixelColorAlpha);
            
            var hbox = new Box(Orientation.Horizontal, 0);
            hbox.Add(vboxLabels);

            var backgroundSliders = new Box(Orientation.Vertical, 0);
            var grayscaleOption = new CheckButton("Grayscale");
            grayscaleOption.Toggled += GrayscaleOnToggle;
            var invertOption = new CheckButton("Invert");
            invertOption.Toggled += InvertOnToggle;
            
            var redSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var redScale = new Scale(Orientation.Horizontal, redSlider);
            redSlider.ValueChanged += RedScaleOnChangeValue;
            var greenSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var greenScale = new Scale(Orientation.Horizontal, greenSlider);
            greenSlider.ValueChanged += GreenScaleOnChangeValue;
            var blueSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var blueScale = new Scale(Orientation.Horizontal, blueSlider);
            blueSlider.ValueChanged += BlueScaleOnChangeValue;
            var alphaSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var alphaScale = new Scale(Orientation.Horizontal, alphaSlider);
            alphaSlider.ValueChanged += AlphaScaleOnChangeValue;
            backgroundSliders.Add(grayscaleOption);
            backgroundSliders.Add(invertOption);
            backgroundSliders.Add(redScale);
            backgroundSliders.Add(greenScale);
            backgroundSliders.Add(blueScale);
            backgroundSliders.Add(alphaScale);
            hbox.Add(backgroundSliders);
            
            var vbox = new Paned(Orientation.Vertical);
            vbox.BorderWidth = 12;
            vbox.Position = 900;
            vbox.Pack1(swin, false, false);
            vbox.Pack2(hbox, false, false);
            Add(vbox);
            ShowAll();
        }

        private void RedScaleOnChangeValue(object o, EventArgs args)
        {
            _myImageArea.SetBackground((byte)((HScale)o).Value, null, null, null);
        }
        
        private void GreenScaleOnChangeValue(object o, EventArgs args)
        {
            _myImageArea.SetBackground(null, (byte)((HScale)o).Value, null, null);
        }
        
        private void BlueScaleOnChangeValue(object o, EventArgs args)
        {
            _myImageArea.SetBackground(null, null, (byte)((HScale)o).Value, null);
        }
        
        private void AlphaScaleOnChangeValue(object o, EventArgs args)
        {
            _myImageArea.SetBackground(null, null, null, (byte)((HScale)o).Value);
        }
        
        private void GrayscaleOnToggle(object o, EventArgs args)
        {
            _myImageArea.ToggleGrayscale();
        }
        
        private void InvertOnToggle(object o, EventArgs args)
        {
            _myImageArea.ToggleInvert();
        }

        private class ImageArea : DrawingArea
        {
            private readonly Image _image;

            public Color BackgroundColor = Color.Clear;

            public Action<EventMotion> MouseMoveAction { get; set; }
            private bool UseGrayscale { get; set; }
            private bool UseInvert { get; set; }

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
                BackgroundColor.Red = red != null ? red.Value / 255.0 : BackgroundColor.Red;
                BackgroundColor.Green = green != null ? green.Value / 255.0 : BackgroundColor.Green;
                BackgroundColor.Blue = blue != null ? blue.Value / 255.0 : BackgroundColor.Blue;
                BackgroundColor.Alpha = alpha != null ? alpha.Value / 255.0 : BackgroundColor.Alpha;
                QueueDraw();
            }

            public void ToggleGrayscale()
            {
                UseGrayscale = !UseGrayscale;
                QueueDraw();
            }

            public void ToggleInvert()
            {
                UseInvert = !UseInvert;
                QueueDraw();
            }

            protected override bool OnDrawn(Context c)
            {
                // TODO reduce calls to this function
                var pixels = _image.ToPixelEnumerable()
                    .ConditionalApply(UseGrayscale, e => e.Grayscale())
                    .ConditionalApply(UseInvert, e => e.Invert());
                    
                foreach (var pixel in pixels)
                {
                    var composite = BackgroundColor.Composite(pixel.Color);
                    c.SetSourceRGBA(composite.Red, composite.Green, composite.Blue, composite.Alpha);
                    c.Rectangle(pixel.X, pixel.Y, 1, 1);
                    c.Fill();
                }
                return true;
            }
        }
    }
}