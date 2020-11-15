using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using MutaBrains.Core.Textures;
using MutaBrains.Config;

namespace MutaBrains.Core.GUI
{
    class Form : Component
    {
        protected Button closeBtn;

        public Form(string title, Vector2 startPosition)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(Navigator.TexturePath("gui_forms_mini"));
            FontCollection collection = new FontCollection();
            FontFamily family = collection.Install(@"assets/fonts/library-3-am.3amsoft.ttf");
            Font font = family.CreateFont(24, FontStyle.Regular);
            TextGraphicsOptions options = new TextGraphicsOptions()
            {
                TextOptions = new TextOptions()
                {
                    ApplyKerning = true,
                    TabWidth = 8,
                    WrapTextWidth = image.Width,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                }
            };
            image.Mutate(x => x.DrawText(options, title, font, Color.BlanchedAlmond, new PointF(0, 14)));

            texture = Texture.fromImage(image);
            origin = ComponentOrigin.TopLeft;
            collisionCheckEnabled = true;
            dragType = DragType.Full;

            Initialize(texture.Size, new Vector3(startPosition));

            closeBtn = new Button("Close", new Vector2(texture.Size.X / 2, texture.Size.Y - 20), ComponentOrigin.Center);
            addChild(closeBtn);
        }
    }
}