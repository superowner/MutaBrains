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

        List<PhysicalObject3D> modelsList;

        public PhysicsTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background(Path.Combine(Navigator.TexturesDir, "gui", "gui_background.png"), window.ClientSize);
            pointer = new Pointer(window.MousePosition);
            PhysicsManager.simulation.Statics.Add(
                new StaticDescription(
                    new System.Numerics.Vector3(0, -1.5f, 0),
                    new CollidableDescription(PhysicsManager.simulation.Shapes.Add(new Box(100, 2, 100)), 0.1f)
                )
            );

            CameraManager.Perspective.Position = new Vector3(0, 2, 12);

            modelsList = new List<PhysicalObject3D>();
            Random rnd = new Random();
            for (int i = 0; i < 10; i++) {
                Vector3 pos = new Vector3(rnd.Next(-4,4) * (float)rnd.NextDouble(), (float)rnd.NextDouble() * rnd.Next(2,20) + 8, rnd.Next(-4,4) * (float)rnd.NextDouble());
                modelsList.Add(new PhysicalObject3D("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), pos, Vector3.One));
            }
            for (int i = 0; i < 10; i++) {
                Vector3 pos = new Vector3(rnd.Next(-4,4) * (float)rnd.NextDouble(), (float)rnd.NextDouble() * rnd.Next(2,20) + 10, rnd.Next(-4,4) * (float)rnd.NextDouble());
                modelsList.Add(new PhysicalObject3D("brain", Path.Combine(Navigator.MeshesDir, "brain", "brain.obj"), pos, new Vector3(0.2f)));
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
            PhysicsManager.simulation.Timestep((float)args.Time);
            Random rnd = new Random();
            Vector3 pos = Vector3.One;

            if (window.KeyboardState.IsKeyReleased(Keys.Escape))
            {
                window.SelectState("main_menu");
            }

            if (window.KeyboardState.IsKeyReleased(Keys.Space) || window.KeyboardState.IsKeyReleased(Keys.Enter)) {
                for (int i = 0; i < 10; i++) {
                    pos = new Vector3(rnd.Next(-4,4) * (float)rnd.NextDouble(), (float)rnd.NextDouble() * rnd.Next(2,20) + 8, rnd.Next(-4,4) * (float)rnd.NextDouble());
                    modelsList.Add(new PhysicalObject3D("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), pos, Vector3.One));
                }
            }

            if (rnd.NextDouble() > 0.99) {
                pos = new Vector3(rnd.Next(-4,4) * (float)rnd.NextDouble(), (float)rnd.NextDouble() * rnd.Next(2,20) + 8, rnd.Next(-4,4) * (float)rnd.NextDouble());
                modelsList.Add(new PhysicalObject3D("book", Path.Combine(Navigator.MeshesDir, "book", "book.obj"), pos, Vector3.One));
            }

            pointer.Update(args.Time, window.MousePosition);
            foreach (PhysicalObject3D model in modelsList)
            {
                model.Update(args.Time, window.MouseState, window.KeyboardState);
            }
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);
            foreach (PhysicalObject3D model in modelsList)
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