using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MutaBrains.Core.Output
{
    class BasicCamera
    {
        protected float windowWidth, windowHeight, fov, aspectRatio, nearPlane, farPlane;
        protected Matrix4 projectionMatrix, viewMatrix;
        protected Vector3 position, front, up;

        public Vector3 Position
        {
            get => position;
            set {
                position = value;
                UpdateViewMatrix();
            }
        }

        public Vector3 Front
        {
            get => front;
        }

        public virtual void Initialize(Vector2 size, Vector3 position)
        {
            windowWidth = size.X;
            windowHeight = size.Y;
            this.position = position;

            fov = MathHelper.PiOver4;
            up = Vector3.UnitY;
            front = -Vector3.UnitZ;
            aspectRatio = windowWidth / (float)windowHeight;
        }

        public virtual void WindowResize(Vector2 newSize)
        {
            windowWidth = newSize.X;
            windowHeight = newSize.Y;
            aspectRatio = windowWidth / (float)windowHeight;
        }

        protected virtual void UpdateProjectionMatrix() { }

        protected virtual void UpdateViewMatrix() { }

        public virtual Matrix4 GetProjectionMatrix()
        {
            UpdateProjectionMatrix();
            return projectionMatrix;
        }

        public virtual Matrix4 GetViewMatrix()
        {
            UpdateViewMatrix();
            return viewMatrix;
        }

        public virtual void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            
        }
    }
}
