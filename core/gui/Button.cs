using OpenTK.Mathematics;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.GUI
{
    class Button : Component
    {
        public Button(string title, Vector2 startPosition, ComponentOrigin origin = ComponentOrigin.Center)
        {
            texture = Texture.fromImage(TextDrawer.DrawOnTexture("gui_buttons_default", title, 0, 5, 18));
            this.origin = origin;
            collisionCheckEnabled = true;
            Initialize(texture.Size, new Vector3(startPosition));
        }
    }
}