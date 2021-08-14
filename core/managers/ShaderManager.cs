using MutaBrains.Core.Shaders;

namespace MutaBrains.Core.Managers
{
    static class ShaderManager
    {
        public static Shader guiShader;
        public static Shader meshShader;
        public static Shader animationShader;
        public static Shader heightMapShader;

        public static void Initialize()
        {
            guiShader = new Shader("gui");
            meshShader = new Shader("mesh");
            animationShader = new Shader("animation");
            heightMapShader = new Shader("height_map");
        }
    }
}