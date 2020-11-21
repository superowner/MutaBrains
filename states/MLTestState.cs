using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;
using MutaBrains.Core.Import;
using MutaBrains.Core.Mesh;
using OpenTK.Mathematics;

namespace MutaBrains.States
{
    public class MLTestState : State
    {
        Background background;
        Pointer pointer;
        Object3D brain;

        public MLTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background("gui_background", window.ClientSize);
            pointer = new Pointer(window.MousePosition);
            brain = new Object3D(AssetImporter.GetSimpleMesh("brain-simple-mesh"), new Vector3(0.0f, 0.0f, 0.0f));

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

            if (window.KeyboardState.IsKeyDown(Keys.Escape))
            {
                window.Close();
            }

            pointer.Update(args.Time, window.MousePosition);
            brain.Update(args.Time, window.MouseState, window.KeyboardState);
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);
            brain.Draw(args.Time);
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