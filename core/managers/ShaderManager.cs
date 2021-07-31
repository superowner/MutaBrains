using MutaBrains.Core.Shaders;

namespace MutaBrains.Core.Managers
{
    static class ShaderManager
    {
        public static Shader guiShader;
        public static Shader simpleMeshShader;
        public static Shader simpleAnimationShader;

        public static void Initialize()
        {
            guiShader = new Shader("gui");
            simpleMeshShader = new Shader("simple_mesh");
            simpleAnimationShader = new Shader("simple_animation");
        }
    }
}