using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.GUI;
using MutaBrains.Windows;

namespace MutaBrains.Core.Managers
{
    static class GUIManager
    {
        private static List<Component> components;
        private static MBWindow window;
        public static void Initialize(MBWindow window)
        {
            GUIManager.window = window;
            TextDrawer.Initialize();

            components = new List<Component>();

            Form testForm = new Form("DRAG'n'DROP ME!", "Just try!", window.ClientSize.ToVector2()/2, ComponentOrigin.Center);
            Button physicsBtn = new Button("Physics test", new Vector2(testForm.size.X / 2, testForm.size.Y - 250), ComponentOrigin.Center);
            Button animationBtn = new Button("Animation test", new Vector2(testForm.size.X / 2, testForm.size.Y - 200), ComponentOrigin.Center);
            Button terrainBtn = new Button("Terrain test", new Vector2(testForm.size.X / 2, testForm.size.Y - 150), ComponentOrigin.Center);
            physicsBtn.OnMouseClick += (o, a) => { window.SelectState("physics_test"); };
            animationBtn.OnMouseClick += (o, a) => { window.SelectState("animation_test"); };
            terrainBtn.OnMouseClick += (o, a) => { window.SelectState("terrain_test"); };
            testForm.addChild(physicsBtn);
            testForm.addChild(animationBtn);
            testForm.addChild(terrainBtn);
            components.Add(testForm);
        }

        public static void Update(double time, Vector2 mousePosition, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            foreach (Component component in components)
            {
                component.Update(time, mousePosition, mouseState, keyboardState);
            }
        }

        public static void Draw(double time)
        {
            foreach (Component component in components)
            {
                component.Draw(time);
            }
        }
    }
}
