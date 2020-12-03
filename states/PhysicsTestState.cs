using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Managers;
using MutaBrains.Windows;
using MutaBrains.Core.GUI;
using MutaBrains.Core.Objects;
using BepuPhysics;
using MutaBrains.Core.Physics;
using BepuUtilities.Memory;
using BepuPhysics.Collidables;

namespace MutaBrains.States
{
    public class PhysicsTestState : State
    {
        Background background;
        Pointer pointer;
        Simulation simulation;
        BufferPool bufferPool;

        List<StaticObject> modelsList;

        public PhysicsTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background(Path.Combine(Navigator.TexturesDir, "gui", "gui_background.png"), window.ClientSize);
            pointer = new Pointer(window.MousePosition);
            bufferPool = new BufferPool();
            simulation = Simulation.Create(bufferPool, new NarrowPhaseCallback(), new PoseIntegratorCallback(new System.Numerics.Vector3(0, -6, 0)), new PositionFirstTimestepper());
            simulation.Statics.Add(new StaticDescription(new System.Numerics.Vector3(0, -1.5f, 0), new CollidableDescription(simulation.Shapes.Add(new Box(100, 2, 100)), 0.1f)));

            CameraManager.Perspective.Position = new Vector3(0, 2, 12);

            modelsList = new List<StaticObject>();
            Random rnd = new Random();
            for (int i = 0; i < 500; i++) {
                Vector3 pos = new Vector3(rnd.Next(-4,4) * (float)rnd.NextDouble(), (float)rnd.NextDouble() * rnd.Next(2,20) + 8, rnd.Next(-4,4) * (float)rnd.NextDouble());
                modelsList.Add(new StaticObject("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), pos, BoundingBoxType.Box, simulation, Vector3.One));
            }
            for (int i = 0; i < 10; i++) {
                Vector3 pos = new Vector3(rnd.Next(-4,4) * (float)rnd.NextDouble(), (float)rnd.NextDouble() * rnd.Next(2,20) + 10, rnd.Next(-4,4) * (float)rnd.NextDouble());
                modelsList.Add(new StaticObject("brain", Path.Combine(Navigator.MeshesDir, "brain", "brain.obj"), pos, BoundingBoxType.Sphere, simulation, new Vector3(0.2f)));
            }

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
            foreach (StaticObject model in modelsList)
            {
                model.Update(args.Time, window.MouseState, window.KeyboardState);
            }
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);
            foreach (StaticObject model in modelsList)
            {
                model.Draw(args.Time);
            }
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