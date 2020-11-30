using OpenTK.Windowing.Desktop;
using MutaBrains.Windows;

namespace MutaBrains
{
    class Program
    {
        static void Main(string[] args)
        {
            GameWindowSettings gwSettings = new GameWindowSettings {
                IsMultiThreaded = false,
            };

            NativeWindowSettings nwSettings = new NativeWindowSettings {
                Size = new OpenTK.Mathematics.Vector2i(1280, 720),
                // WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen,
                NumberOfSamples = 4,
                Title = "MutaBrains",
                WindowBorder = OpenTK.Windowing.Common.WindowBorder.Fixed,
                IsEventDriven = false,
            };

            MainWindow window = new MainWindow(gwSettings, nwSettings);
            // window.VSync = OpenTK.Windowing.Common.VSyncMode.On;
            window.Run();
        }
    }
}
