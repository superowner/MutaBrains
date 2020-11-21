using OpenTK.Mathematics;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.GUI
{
    class Form : Component
    {
        public Form(string title, string content, Vector2 startPosition, ComponentOrigin origin = ComponentOrigin.TopLeft)
        {
            var title_image = TextDrawer.DrawOnTexture("gui_forms_mini", title, 0, 14);
            var content_image = TextDrawer.DrawOnTexture(title_image, content, 0, 60, 16);
            texture = Texture.fromImage(content_image);
            this.origin = origin;
            collisionCheckEnabled = true;
            dragType = DragType.Full;

            Initialize(texture.Size, new Vector3(startPosition));
        }
    }
}