using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.GUI;

namespace MutaBrains.Core.Managers
{
    static class GUIManager
    {
        private static List<Component> components = new List<Component>();
        public static void Initialize()
        {
            TextDrawer.Initialize();

            Form testForm = new Form("DRAG'n'DROP ME!", "Just try!", new Vector2(20));
            Button closeBtn = new Button("CLOSE", new Vector2(testForm.size.X / 2, testForm.size.Y - 24), ComponentOrigin.Center);
            closeBtn.OnMouseClick += (o, a) => { testForm.Hide(); };
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
