using Gtk;
using SharperImage.Formats;

namespace SharperImage.Viewer;

public class Viewer
{
    public static void Open(string filename, FileFormat format)
    {
        Application.Init();

        var app = new Application("org.SharperImage.Viewer.SharperImage.Viewer", GLib.ApplicationFlags.None);
        app.Register(GLib.Cancellable.Current);

        var win = new MainWindow(filename, format);
        app.AddWindow(win);

        win.Show();
        Application.Run();
    }
    
    public static void Open(Image image)
    {
        Application.Init();

        var app = new Application("org.SharperImage.Viewer.SharperImage.Viewer", GLib.ApplicationFlags.None);
        app.Register(GLib.Cancellable.Current);

        var win = new MainWindow(image);
        app.AddWindow(win);

        win.Show();
        Application.Run();
    }
}