using System;
using System.IO;

namespace MutaBrains.Config
{
    public static class Navigator
    {
        public static string RootDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string AssetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
        public static string ShadersDir = Path.Combine(AssetsDir, "shaders");
        public static string TexturesDir = Path.Combine(AssetsDir, "textures");

        public static string VertSahderPath(string name)
        {
            name = Path.Combine(name, name + ".vert");
            return Path.Combine(ShadersDir, name);
        }

        public static string FragSahderPath(string name)
        {
            name = Path.Combine(name, name + ".frag");
            return Path.Combine(ShadersDir, name);
        }

        public static string TexturePath(string name)
        {
            string[] files = Directory.GetFiles(TexturesDir, name + ".*", SearchOption.AllDirectories);
            if (files.Length > 0) {
                return files[0];
            }

            throw new FileNotFoundException("Can't load texture. File not found.", name);
        }
    }
}