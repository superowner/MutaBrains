using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Import;
using MutaBrains.Config;

namespace MutaBrains.Core.GUI
{
    class Button : Component
    {
        public Button(Vector2 startPosition, ComponentOrigin origin = ComponentOrigin.Center)
        {
            // AssetImporter.LoadTexture("gui_buttons_default");
            // texture = AssetImporter.GetTexture("gui_buttons_default");
            // this.origin = origin;
            // Initialize(texture.Size, new Vector3(startPosition));

            Image<Rgba32> image = Image.Load<Rgba32>(Navigator.TexturePath("gui_buttons_default"));
            FontCollection collection = new FontCollection();
            FontFamily family = collection.Install(@"assets/fonts/library-3-am.3amsoft.ttf");
            Font font = family.CreateFont(18, FontStyle.Regular);
            string text = "Закрыть";
            TextGraphicsOptions options = new TextGraphicsOptions()
            {
                TextOptions = new TextOptions()
                {
                    ApplyKerning = true,
                    TabWidth = 8, // a tab renders as 8 spaces wide
                    WrapTextWidth = image.Width, // greater than zero so we will word wrap at 100 pixels wide
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            };
            image.Mutate(x => x.DrawText(options, text, font, Color.AntiqueWhite, new PointF(0, image.Height / 2.0f)));

            texture = Texture.fromImage(image);
            this.origin = origin;
            Initialize(texture.Size, new Vector3(startPosition));
        }
    }
}