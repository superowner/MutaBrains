using OpenTK.Windowing.Common;
using MutaBrains.Windows;

namespace MutaBrains.States
{
    public class State
    {
        public string Name;
        public MBWindow window;
        public bool IsLoaded = false;

        public State(string name, MBWindow window)
        {
            Name = name;
            this.window = window;
        }

        public virtual void OnLoad() {
            IsLoaded = true;
        }

        public virtual void OnResize(ResizeEventArgs e) {
            if (!IsLoaded) { OnLoad(); return; }
        }

        public virtual void OnUpdate(FrameEventArgs args) {
            if (!IsLoaded) { OnLoad(); return; }
        }

        public virtual void OnDraw(FrameEventArgs args) {
            if (!IsLoaded) { OnLoad(); return; }
        }

        public virtual void Dispose() { }
    }
}