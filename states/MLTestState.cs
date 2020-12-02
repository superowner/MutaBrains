using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;
using MutaBrains.Core.Import.AssimpLoader;
using BepuPhysics;
using MutaBrains.Core.Physics;
using BepuUtilities.Memory;

namespace MutaBrains.States
{
    public class MLTestState : State
    {
        Background background;
        Pointer pointer;
        AssimpModel brain;
        AssimpModel brain1;
        AssimpModel brain2;
        AssimpModel brain3;
        Simulation simulation;
        BufferPool bufferPool;

        public MLTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background(Path.Combine(Navigator.TexturesDir, "gui", "gui_background.png"), window.ClientSize);
            pointer = new Pointer(window.MousePosition);
            bufferPool = new BufferPool();
            simulation = Simulation.Create(bufferPool, new NarrowPhaseCallback(), new PoseIntegratorCallback(new System.Numerics.Vector3(0, -6, 0)), new PositionFirstTimestepper());
            brain = new AssimpModel("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), new Vector3(0, 0, 0), simulation);
            brain1 = new AssimpModel("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), new Vector3(0.5f, 2, 0), simulation);
            brain2 = new AssimpModel("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), new Vector3(0.5f, 1, 0), simulation);
            brain3 = new AssimpModel("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), new Vector3(0, 3, 0), simulation);

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

            if (window.KeyboardState.IsKeyReleased(Keys.Escape))
            {
                window.SelectState("main_menu");
            }

            simulation.Timestep((float)args.Time);

            pointer.Update(args.Time, window.MousePosition);
            brain.Update(args.Time, window.MouseState, window.KeyboardState);
            brain1.Update(args.Time, window.MouseState, window.KeyboardState);
            brain2.Update(args.Time, window.MouseState, window.KeyboardState);
            brain3.Update(args.Time, window.MouseState, window.KeyboardState);
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);
            brain.Draw(args.Time);
            brain1.Draw(args.Time);
            brain2.Draw(args.Time);
            brain3.Draw(args.Time);
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