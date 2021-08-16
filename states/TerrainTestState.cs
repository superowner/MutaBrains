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
    public class TerrainTestState : State
    {
        Background background;
        Pointer pointer;

        Terrain3D terrain;

        public TerrainTestState(string name, MBWindow window) : base(name, window) { }

        public override void OnLoad()
        {
            window.CursorVisible = false;

            background = new Background(Path.Combine(Navigator.TexturesDir, "gui", "gui_background.png"), window.ClientSize);
            pointer = new Pointer(window.MousePosition);

            terrain = new Terrain3D("terrain", "", new Vector3(-50, 0, -50), new Vector3(1.0f));

            CameraManager.Perspective.Position = new Vector3(0, 2, 6);

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

            pointer.Update(args.Time, window.MousePosition);

            terrain.Update(args.Time);

            CameraManager.Perspective.Update(args.Time, window.MouseState, window.KeyboardState);
        }

        public override void OnDraw(FrameEventArgs args)
        {
            base.OnDraw(args);

            background.Draw(args.Time);

            terrain.Draw(args.Time);

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