using MutaBrains.Core.Shaders;

namespace MutaBrains.Core.Managers
{
    static class ShaderManager
    {
        public static Shader guiShader;

        public static void Initialize()
        {
            guiShader = new Shader("gui");
        }
    }
}