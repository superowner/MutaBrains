using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Import;
using MutaBrains.Config;
using OpenTK.Windowing.Common;
using System;

namespace MutaBrains.Core.GUI
{
    class Form : Component
    {
        protected Button closeBtn;

        public Form(Vector2 startPosition)
        {
            // AssetImporter.LoadTexture("gui_forms_mini");
            // texture = AssetImporter.GetTexture("gui_forms_mini");

            Image<Rgba32> image = Image.Load<Rgba32>(Navigator.TexturePath("gui_forms_mini"));
            FontCollection collection = new FontCollection();
            FontFamily family = collection.Install(@"assets/fonts/library-3-am.3amsoft.ttf");
            Font font = family.CreateFont(24, FontStyle.Regular);
            string text = "ТЕСТ ФОРМА";
            TextGraphicsOptions options = new TextGraphicsOptions()
            {
                TextOptions = new TextOptions()
                {
                    ApplyKerning = true,
                    TabWidth = 8, // a tab renders as 8 spaces wide
                    WrapTextWidth = image.Width, // greater than zero so we will word wrap at 100 pixels wide
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                }
            };
            image.Mutate(x => x.DrawText(options, text, font, Color.BlanchedAlmond, new PointF(0, 14)));

            texture = Texture.fromImage(image);

            origin = ComponentOrigin.TopLeft;
            collisionCheckEnabled = true;
            Initialize(texture.Size, new Vector3(startPosition));

            closeBtn = new Button(new Vector2(texture.Size.X / 2, texture.Size.Y - 20), ComponentOrigin.Center);
            addChild(closeBtn);

            OnMouseHover += Form_OnMouseHover;
            OnMouseLeave += Form_OnMouseLeave;
            OnMouseDown += Form_OnMouseDown;
            OnMouseUp += Form_OnMouseUp;
            OnMouseClick += Form_OnMouseClick;
        }

        private void Form_OnMouseClick(object sender, MouseButtonEventArgs args)
        {
            Console.WriteLine("CLICK");
        }

        private void Form_OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            Console.WriteLine("UP");
        }

        private void Form_OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            Console.WriteLine("DOWN");
        }

        private void Form_OnMouseLeave(object sender, MouseButtonEventArgs args)
        {
            Console.WriteLine("LEAVE");
        }

        private void Form_OnMouseHover(object sender, MouseButtonEventArgs args)
        {
            Console.WriteLine("HOVER");
        }
    }
}