using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;

namespace MutaBrains.Core.Output
{
    class PerspectiveCamera : BasicCamera
    {
        private bool drawLines = false;

        public PerspectiveCamera(Vector2 size, Vector3 cameraPosition)
        {
            nearPlane = 0.1f;
            farPlane = 10000.0f;

            Initialize(size, cameraPosition);
        }

        protected override void UpdateProjectionMatrix()
        {
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }

        protected override void UpdateViewMatrix()
        {
            viewMatrix = Matrix4.LookAt(position, position + front, up);
        }

        public override void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            if (keyboardState.IsKeyReleased(Keys.G))
            {
                drawLines = !drawLines;
                
                if (drawLines)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                else
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }

            Vector2 mousePos = mouseState.Position;
            Vector2 offset = mousePos - mouseState.PreviousPosition;
            float angleX = -offset.Y / 300;
            float angleY = -offset.X / 300;
            Vector3 axis = Vector3.Cross(front, up).Normalized();

            front = Vector3.TransformVector(front, Matrix4.CreateFromAxisAngle(axis, angleX));
            front = Vector3.TransformVector(front, Matrix4.CreateFromAxisAngle(up, angleY));

            if (mouseState.ScrollDelta.Y != 0)
            {
                position += front * mouseState.ScrollDelta.Y;
            }

            UpdateViewMatrix();
        }
    }
}
