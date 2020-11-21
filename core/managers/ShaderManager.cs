using MutaBrains.Core.Shaders;

namespace MutaBrains.Core.Managers
{
    static class ShaderManager
    {
        public static Shader guiShader;
        public static Shader simpleMeshShader;

        public static void Initialize()
        {
            guiShader = new Shader("gui");
            simpleMeshShader = new Shader("simple_mesh");
        }
    }
}