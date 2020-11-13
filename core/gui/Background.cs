using OpenTK.Mathematics;
using MutaBrains.Core.Import;

namespace MutaBrains.Core.GUI
{
    class Background : Component
    {
        public Background(string texture_name, Vector2i clientSize)
        {
            AssetImporter.LoadTexture(texture_name);
            texture = AssetImporter.GetTexture(texture_name);

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