using System.IO;
using OpenTK.Mathematics;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Import;

namespace MutaBrains.Core.GUI
{
    class Button : Component
    {
        public Button(string title, Vector2 startPosition, ComponentOrigin origin = ComponentOrigin.Center)
        {
            texture = Texture.fromImage(TextDrawer.DrawOnTexture(Path.Combine(Navigator.TexturesDir, "gui", "buttons", "gui_buttons_default.png"), title, 0, 5, 18));
            this.origin = origin;
            collisionCheckEnabled = true;
            Initialize(texture.Size, new Vector3(startPosition));
        }
    }
}