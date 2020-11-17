using OpenTK.Windowing.Common;
using MutaBrains.Windows;

namespace MutaBrains.States
{
    public class State
    {
        public string Name;
        protected MBWindow window;

        public State(string name, MBWindow window)
        {
            Name = name;
            this.window = window;
        }

        public virtual void OnLoad() { }

        public virtual void OnResize(ResizeEventArgs e) { }

        public virtual void OnUpdate(FrameEventArgs args) { }

        public virtual void OnDraw(FrameEventArgs args) { }
    }
}