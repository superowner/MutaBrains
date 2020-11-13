using OpenTK.Mathematics;
using MutaBrains.Core.Import;

namespace MutaBrains.Core.GUI
{
    class Pointer : Component
    {
        public Pointer(Vector2 startPosition)
        {
            AssetImporter.LoadTexture("pointer_default");
            texture = AssetImporter.GetTexture("pointer_default");

            Initialize(texture.Size, new Vector3(400, 300, 0));
        }

        public override void Update(double time, Vector2 newPosition, bool updateInput = true)
        {
            position = new Vector3(newPosition);
            RefreshVertexBuffer();
        }
    }
}