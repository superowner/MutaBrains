using System.IO;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;

namespace MutaBrains.States
{
    public class MainMenuState : State
    {
        Background background;
        Pointer pointer;

        public MainMenuState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background(Path.Combine(Navigator.TexturesDir, "gui", "gui_background.png"), window.ClientSize);
            pointer = new Pointer(window.MousePosition);

            base.OnLoad();
        }

        public override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            CameraManager.WindowResize(window.ClientSize.ToVector2());
            pointer.WindowResize(window.ClientSize.ToVector2());
            background.WindowResize(window.ClientSize.ToVector2());
        }

        public override void OnUpdate(FrameEventArgs args)
        {
            base.OnUpdate(args);

            pointer.Update(args.Time, window.MousePosition);
            if (window.KeyboardState.IsKeyReleased(Keys.Escape))
            {
                window.Close();
            }
            GUIManager.Update(args.Time, window.MousePosition, window.MouseState, window.KeyboardState);
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);
            GUIManager.Draw(args.Time);
            pointer.Draw(args.Time);
        }

        public override void Dispose()
        {
            base.Dispose();

            background.Dispose();
            pointer.Dispose();
        }
    }
}