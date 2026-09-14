using Avalonia;
using Avalonia.Controls;
using Semi.Avalonia;

internal class Program
{
    public static void Main(string[] args)
    {
        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .Start(AppMain, args);
    }

    private static void AppMain(Application app, string[] args)
    {
        app.Styles.Add(new SemiTheme());

        var window = new Window
        {
            Title = "Hello from Code",
            Width = 400,
            Height = 300,
            Content = new WrapPanel
            {
                ItemSpacing = 10,
                LineSpacing = 10,
                Children =
                {
                    new TextBlock { Text = "Hello, StackPanel!" },
                    new TextBlock { Text = "Hello, StackPanel!" },
                    new TextBlock { Text = "Hello, StackPanel!" }
                }
            }
        };

        window.Show();
        app.Run(window);
    }
}