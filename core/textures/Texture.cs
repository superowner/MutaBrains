using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace MutaBrains.Core.Textures
{
    public class Texture
    {
        public readonly int Handle;
        public string Path;
        public Vector2 Size;

        public Texture(string path)
        {
            Handle = GL.GenTexture();
            Use();

            using (Image<Rgba32> image = Image.Load<Rgba32>(path))
            {
                this.Size = new Vector2(image.Width, image.Height);
                
                image.Mutate(y => y.Flip(FlipMode.Vertical));
                var data = new uint[image.Width * image.Height];

                for (int x = 0; x < image.Width; x++) {
                    for (int y = 0; y < image.Height; y++) {
                        var color = image[x, y];
                        data[y * image.Width + x] = color.Rgba;
                    }
                }

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Path = path;
        }

        public Texture(Image<Rgba32> image) {
            Handle = GL.GenTexture();
            Use();

            using (image)
            {
                this.Size = new Vector2(image.Width, image.Height);
                
                image.Mutate(y => y.Flip(FlipMode.Vertical));
                var data = new uint[image.Width * image.Height];

                for (int x = 0; x < image.Width; x++) {
                    for (int y = 0; y < image.Height; y++) {
                        var color = image[x, y];
                        data[y * image.Width + x] = color.Rgba;
                    }
                }

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Path = string.Empty;
        }

        public static Texture fromImage(Image<Rgba32> image)
        {
            return new Texture(image);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}