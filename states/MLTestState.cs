using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;

namespace MutaBrains.States
{
    public class MLTestState : State
    {
        Pointer pointer;

        public MLTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            pointer = new Pointer(window.MousePosition);
        }

        public override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            CameraManager.WindowResize(window.ClientSize.ToVector2());
            pointer.WindowResize(window.ClientSize.ToVector2());
        }

        public override void OnUpdate(FrameEventArgs args)
        {
            base.OnUpdate(args);

            pointer.Update(args.Time, window.MousePosition);
            if (window.KeyboardState.IsKeyDown(Keys.Escape))
            {
                window.Close();
            }
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            pointer.Draw(args.Time);
        }

        public override void Dispose()
        {
            base.Dispose();

            pointer.Dispose();
        }
    }
}