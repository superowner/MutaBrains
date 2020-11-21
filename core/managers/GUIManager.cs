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
            Button closeBtn = new Button("BRAINS!", new Vector2(testForm.size.X / 2, testForm.size.Y - 24), ComponentOrigin.Center);
            closeBtn.OnMouseClick += (o, a) => { window.SelectState("ml_test"); };
            testForm.addChild(closeBtn);
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
