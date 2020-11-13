using System.Collections.Generic;
using MutaBrains.Config;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Import
{
    public static class AssetImporter
    {
        private static List<Texture> textureList = new List<Texture>();
        
        public static void LoadTexture(string name, bool fillable = false)
        {
            name = name.ToLower();
            string path = Navigator.TexturePath(name);
            Texture texture = textureList.Find(t => t.Path == path);
            if (texture == null) {
                textureList.Add(new Texture(path));
            }
        }

        public static Texture GetTexture(string name)
        {
            name = name.ToLower();
            string path = Navigator.TexturePath(name);
            return textureList.Find(t => t.Path == path);
        }
    }
}