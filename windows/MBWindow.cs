using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using MutaBrains.States;
using MutaBrains.Core.FPS;

namespace MutaBrains.Windows
{
    public class MBWindow : GameWindow
    {
        private List<State> states = new List<State>();
        private State selectedState;

        public MBWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

        protected virtual void AddState(State state)
        {
            if (!states.Contains(state))
            {
                states.Add(state);
            }
        }

        protected virtual void RemoveState(State state)
        {
            if (states.Contains(state))
            {
                states.Remove(state);
            }
        }

        protected virtual State GetState(string name)
        {
            return states.Find(s => s.Name == name.ToLower());
        }

        protected virtual void SelectState(string name)
        {
            selectedState = states.Find(s => s.Name == name.ToLower());
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ClearColor(Color4.Black);

            if (selectedState != null) {
                selectedState.OnLoad();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            if (selectedState != null) {
                selectedState.OnResize(e);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (!IsFocused || selectedState == null) return;

            selectedState.OnUpdate(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            FPSCounter.Calculate(args.Time);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            if (selectedState != null) {
                selectedState.OnDraw(args);
            }
            SwapBuffers();
            Title = $"MutaBrains :: FPS now: { FPSCounter.FPS } max: { FPSCounter.Max } min: { FPSCounter.Min }";
        }
    }
}
