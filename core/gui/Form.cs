using System.IO;
using OpenTK.Mathematics;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Import;

namespace MutaBrains.Core.GUI
{
    class Form : Component
    {
        public Form(string title, string content, Vector2 startPosition, ComponentOrigin origin = ComponentOrigin.TopLeft)
        {
            var title_image = TextDrawer.DrawOnTexture(Path.Combine(Navigator.TexturesDir, "gui", "forms", "gui_forms_mini.jpg"), title, 0, 14);
            var content_image = TextDrawer.DrawOnTexture(title_image, content, 0, 60, 16);
            texture = Texture.fromImage(content_image);
            this.origin = origin;
            collisionCheckEnabled = true;
            dragType = DragType.Full;

            Initialize(texture.Size, new Vector3(startPosition));
        }
    }
}