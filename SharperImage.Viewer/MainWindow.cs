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
                    var composite = MyImageArea.BackgroundColor.Composite(pix.Color);
                    HoverPixelX.Text = $"X: {x}";
                    HoverPixelY.Text = $"Y: {y}";
                    HoverPixelColorRed.Text = $"Red: {pix.Color.RedByte} ({composite.RedByte})";
                    HoverPixelColorGreen.Text = $"Green: {pix.Color.GreenByte} ({composite.GreenByte})";
                    HoverPixelColorBlue.Text = $"Blue: {pix.Color.BlueByte} ({composite.BlueByte})";
                    HoverPixelColorAlpha.Text = $"Alpha: {pix.Color.AlphaByte} ({composite.AlphaByte})";
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
            var grayscaleOption = new CheckButton("Grayscale");
            grayscaleOption.Toggled += GrayscaleOnToggle;
            var invertOption = new CheckButton("Invert");
            invertOption.Toggled += InvertOnToggle;
            
            var redSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var redScale = new HScale(redSlider);
            redScale.ValueChanged += RedScaleOnChangeValue;
            var greenSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var greenScale = new HScale(greenSlider);
            greenSlider.ValueChanged += GreenScaleOnChangeValue;
            var blueSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var blueScale = new HScale(blueSlider);
            blueScale.ValueChanged += BlueScaleOnChangeValue;
            var alphaSlider = new Adjustment(0, 0, 255, 1, 15, 15);
            var alphaScale = new HScale(alphaSlider);
            alphaScale.ValueChanged += AlphaScaleOnChangeValue;
            backgroundSliders.Add(grayscaleOption);
            backgroundSliders.Add(invertOption);
            backgroundSliders.Add(redScale);
            backgroundSliders.Add(greenScale);
            backgroundSliders.Add(blueScale);
            backgroundSliders.Add(alphaScale);
            hbox.Add(backgroundSliders);
            
            var vbox = new VPaned();
            vbox.BorderWidth = 12;
            vbox.Position = 900;
            vbox.Pack1(swin, false, false);
            vbox.Pack2(hbox, false, false);
            Add(vbox);
            ShowAll();
        }

        private void RedScaleOnChangeValue(object o, EventArgs args)
        {
            MyImageArea.SetBackground((byte)((HScale)o).Value, null, null, null);
        }
        
        private void GreenScaleOnChangeValue(object o, EventArgs args)
        {
            MyImageArea.SetBackground(null, (byte)((HScale)o).Value, null, null);
        }
        
        private void BlueScaleOnChangeValue(object o, EventArgs args)
        {
            MyImageArea.SetBackground(null, null, (byte)((HScale)o).Value, null);
        }
        
        private void AlphaScaleOnChangeValue(object o, EventArgs args)
        {
            MyImageArea.SetBackground(null, null, null, (byte)((HScale)o).Value);
        }
        
        private void GrayscaleOnToggle(object o, EventArgs args)
        {
            MyImageArea.ToggleGrayscale();
        }
        
        private void InvertOnToggle(object o, EventArgs args)
        {
            MyImageArea.ToggleInvert();
        }

        private class ImageArea : DrawingArea
        {
            private readonly Image _image;

            public Color BackgroundColor = Color.Clear;

            public Action<EventMotion> MouseMoveAction { get; set; }
            private bool UseGrayscale { get; set; } = false;
            private bool UseInvert { get; set; } = false;

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