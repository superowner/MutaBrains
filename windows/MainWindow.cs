using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using MutaBrains.Core.Shaders;
using MutaBrains.Core.GUI;
using MutaBrains.Core.Output;

namespace MutaBrains.Windows
{
    public class MainWindow : GameWindow
    {
        Background background;
        Pointer pointer;
        Form testForm;

        public MainWindow(GameWindowSettings gwSettings, NativeWindowSettings nwSettings) : base(gwSettings, nwSettings)
        {
            CursorVisible = false;
        }

        protected override void OnLoad()
        {
            ShaderManager.Initialize();
            CameraManager.Initialize(ClientSize.ToVector2(), Vector3.UnitZ * 10);

            background = new Background("gui_background", ClientSize);
            pointer = new Pointer(MousePosition);
            testForm = new Form(new Vector2(50));

            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ClearColor(Color4.Black);

            base.OnLoad();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            CameraManager.WindowResize(ClientSize.ToVector2());
            pointer.WindowResize(ClientSize.ToVector2());
            background.WindowResize(ClientSize.ToVector2());

            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (!IsFocused) return;

            pointer.Update(args.Time, MousePosition);

            if (KeyboardState.IsKeyDown(Keys.Escape)) {
                Close();
            }

            testForm.Update(args.Time, MousePosition);

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            background.Draw(args.Time);
            testForm.Draw(args.Time);
            pointer.Draw(args.Time);

            SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
