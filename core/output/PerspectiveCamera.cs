using OpenTK.Mathematics;

namespace MutaBrains.Core.Output
{
    class PerspectiveCamera : BasicCamera
    {
        public PerspectiveCamera(Vector2 size, Vector3 cameraPosition)
        {
            nearPlane = 0.1f;
            farPlane = 10000.0f;

            Initialize(size, cameraPosition);
        }

        public void Autofocus()
        {
            
        }

        protected override void UpdateProjectionMatrix()
        {
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }

        protected override void UpdateViewMatrix()
        {
            viewMatrix = Matrix4.LookAt(position, position + front, up);
        }
    }
}
