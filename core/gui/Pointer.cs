using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Import;

namespace MutaBrains.Core.GUI
{
    class Pointer : Component
    {
        public Pointer(Vector2 startPosition)
        {
            texture = AssetImporter.LoadTexture(Path.Combine(Navigator.TexturesDir, "pointers", "pointer_default.png"));

            Initialize(texture.Size, new Vector3(startPosition));
        }

        public override void Update(double time, Vector2 mousePosition, MouseState mouseStat = null, KeyboardState keyboardState = null)
        {
            position = new Vector3(mousePosition);
            RefreshVertexBuffer();
        }
    }
}