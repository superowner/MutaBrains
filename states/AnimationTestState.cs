using System;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;
using MutaBrains.Core.Objects;

namespace MutaBrains.States
{
    public class AnimationTestState : State
    {
        Background background;
        Pointer pointer;
        AnimatedObject3D rumba;

        public AnimationTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background(Path.Combine(Navigator.TexturesDir, "gui", "gui_background.png"), window.ClientSize);
            pointer = new Pointer(window.MousePosition);

            rumba = new AnimatedObject3D("animated", Path.Combine(Navigator.MeshesDir, "test", "parent-child-child-letter-f.dae"), new Vector3(0, 0, 0), new Vector3(1.0f));

            CameraManager.Perspective.Position = new Vector3(0, 0, 8);

            window.MouseWheel += Window_MouseWheel;

            base.OnLoad();
        }

        private void Window_MouseWheel(MouseWheelEventArgs obj)
        {
            Vector3 camPos = CameraManager.Perspective.Position;
            camPos.Z -= obj.OffsetY / 4;
            CameraManager.Perspective.Position = camPos;
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

            if (window.KeyboardState.IsKeyReleased(Keys.Escape))
            {
                window.SelectState("main_menu");
            }

            rumba.Update(args.Time, window.MouseState, window.KeyboardState);

            pointer.Update(args.Time, window.MousePosition);
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);

            rumba.Draw(args.Time);

            pointer.Draw(args.Time);
        }

        public override void Dispose()
        {
            base.Dispose();

            pointer.Dispose();
            background.Dispose();
        }
    }
}