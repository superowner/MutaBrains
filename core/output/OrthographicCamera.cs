using OpenTK.Mathematics;

namespace MutaBrains.Core.Output
{
    class OrthographicCamera : BasicCamera
    {
        public OrthographicCamera(Vector2 size)
        {
            nearPlane = -1.0f;
            farPlane = 1.0f;

            Initialize(size, Vector3.UnitZ);
        }

        protected override void UpdateProjectionMatrix()
        {
            projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, windowWidth, windowHeight, 0, nearPlane, farPlane);
        }

        protected override void UpdateViewMatrix()
        {
            viewMatrix = Matrix4.LookAt(position, position + front, up);
        }
    }
}
