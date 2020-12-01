using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

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
            family = collection.Install(Path.Combine(Navigator.FontsDir, "OpenSans-Bold.ttf"));
        }

        public static Image<Rgba32> DrawOnTexture(string path, string text, float x = 0, float y = 0, float font_size = 24, FontStyle style = FontStyle.Regular)
        {
            font = family.CreateFont(font_size, style);
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            return DrawOnTexture(image, text, x, y, font_size, style);
        }

        public static Image<Rgba32> DrawOnTexture(Image<Rgba32> image, string text, float x = 0, float y = 0, float font_size = 24, FontStyle style = FontStyle.Regular)
        {
            font = family.CreateFont(font_size, style);
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