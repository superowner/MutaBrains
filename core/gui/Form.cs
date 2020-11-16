using OpenTK.Mathematics;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.GUI
{
    class Form : Component
    {
        protected Button closeBtn;

        public Form(string title, Vector2 startPosition)
        {
            texture = Texture.fromImage(TextDrawer.DrawOnTexture("gui_forms_mini", title, 0, 14));
            origin = ComponentOrigin.TopLeft;
            collisionCheckEnabled = true;
            dragType = DragType.Full;

            Initialize(texture.Size, new Vector3(startPosition));
        }
    }
}