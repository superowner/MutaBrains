using OpenTK.Mathematics;

namespace MutaBrains.Core.Import.AssimpLoader
{
    class AssimpAnimatedModel : AssimpModel
    {
        public AssimpAnimatedModel(string name, string path, Vector3 startPosition) : base(name, path, startPosition)
        {
        }
    }
}
