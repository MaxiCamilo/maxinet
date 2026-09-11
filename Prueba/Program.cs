using Avalonia;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;

class Program
{
    public static void Main(string[] args)
    {

        AppBuilder.Configure<Application>()
                  .UsePlatformDetect()
                  .Start(AppMain, args);
    }

    static void AppMain(Application app, string[] args)
    {

        app.Styles.Add(new Semi.Avalonia.SemiTheme());

        var window = new Window
        {
            Title = "Hello from Code",
            Width = 400,
            Height = 300,
            Content = new DockPanel()
            {
                
            }
        };

        window.Show();
        app.Run(window);
    }
}
