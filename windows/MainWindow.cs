using OpenTK.Windowing.Desktop;
using MutaBrains.States;

namespace MutaBrains.Windows
{
    public class MainWindow : MBWindow
    {
        public MainWindow(GameWindowSettings gwSettings, NativeWindowSettings nwSettings) : base(gwSettings, nwSettings)
        {
            AddState(new MainMenuState("main_menu", this));
            SelectState("main_menu");
        }
    }
}
