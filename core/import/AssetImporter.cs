using System.Collections.Generic;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Import
{
    public static class AssetImporter
    {
        private static List<Texture> textureList = new List<Texture>();
        
        public static Texture LoadTexture(string path)
        {
            Texture texture = textureList.Find(t => t.Path == path);
            if (texture == null) {
                texture = new Texture(path);
                textureList.Add(texture);
            }

            return texture;
        }
    }
}