using OpenTK.Mathematics;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.GUI
{
    class Background : Component
    {
        public Background(string path, Vector2i clientSize)
        {
            texture = Texture.LoadTexture(path);

            Initialize(new Vector2(clientSize.X, clientSize.Y), new Vector3(clientSize.X / 2.0f, clientSize.Y / 2.0f, 0));
        }

        public override void WindowResize(Vector2 newSize)
        {
            size = newSize;
            position = new Vector3(size/2);
            base.WindowResize(newSize);
        }
    }
}