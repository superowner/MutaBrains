using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using MutaBrains.Core.GUI;
using MutaBrains.Core.FPS;
using MutaBrains.Core.Managers;

namespace MutaBrains.Windows
{
    public class MainWindow : GameWindow
    {
        Background background;
        Pointer pointer;

        public MainWindow(GameWindowSettings gwSettings, NativeWindowSettings nwSettings) : base(gwSettings, nwSettings)
        {
            CursorVisible = false;
        }

        protected override void OnLoad()
        {
            ShaderManager.Initialize();
            CameraManager.Initialize(ClientSize.ToVector2(), Vector3.UnitZ * 10);
            GUIManager.Initialize();

            background = new Background("gui_background", ClientSize);
            pointer = new Pointer(MousePosition);

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

            GUIManager.Update(args.Time, MousePosition, MouseState, KeyboardState);

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            FPSCounter.Calculate(args.Time);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            background.Draw(args.Time);
            GUIManager.Draw(args.Time);
            pointer.Draw(args.Time);

            SwapBuffers();
            base.OnRenderFrame(args);
            Title = $"MutaBrains :: FPS now: { FPSCounter.FPS } max: { FPSCounter.Max } min: { FPSCounter.Min }";
        }
    }
}
