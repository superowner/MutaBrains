namespace MutaBrains.Core.Shaders
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