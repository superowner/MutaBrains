using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;
using OpenTK.Windowing.Common;

namespace MutaBrains.States
{
    public class MainMenuState : State
    {
        Background background;
        Pointer pointer;

        public MainMenuState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            base.OnLoad();

            window.CursorVisible = false;

            ShaderManager.Initialize();
            CameraManager.Initialize(window.ClientSize.ToVector2(), Vector3.UnitZ * 10);
            GUIManager.Initialize();

            background = new Background("gui_background", window.ClientSize);
            pointer = new Pointer(window.MousePosition);
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
            if (window.KeyboardState.IsKeyDown(Keys.Escape))
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
    }
}