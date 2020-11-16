using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using MutaBrains.Config;

namespace MutaBrains.Core.GUI
{
    class TextDrawer
    {
        private static FontCollection collection;
        private static FontFamily family;
        private static Font font;

        public static void Initialize()
        {
            collection = new FontCollection();
            family = collection.Install(@"assets/fonts/library-3-am.3amsoft.ttf");
            font = family.CreateFont(24, FontStyle.Regular);
        }

        public static Image<Rgba32> DrawOnTexture(string texture_name, string text, float x = 0, float y = 0)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(Navigator.TexturePath(texture_name));
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

            image.Mutate(m => m.DrawText(options, text, font, Color.BlanchedAlmond, new PointF(x, y)));

            return image;
        }
    }
}