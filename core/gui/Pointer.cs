using OpenTK.Mathematics;
using MutaBrains.Core.Import;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

        public override void Update(double time, Vector2 mousePosition, MouseState mouseStat = null, KeyboardState keyboardState = null, bool updateInput = true)
        {
            position = new Vector3(mousePosition);
            RefreshVertexBuffer();
        }
    }
}