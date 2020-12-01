using System;
using System.IO;

namespace MutaBrains.Core.Import
{
    public static class Navigator
    {
        public static string RootDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string AssetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
        public static string ShadersDir = Path.Combine(AssetsDir, "shaders");
        public static string TexturesDir = Path.Combine(AssetsDir, "textures");
        public static string MeshesDir = Path.Combine(AssetsDir, "meshes");
        public static string FontsDir = Path.Combine(AssetsDir, "fonts");
    }
}