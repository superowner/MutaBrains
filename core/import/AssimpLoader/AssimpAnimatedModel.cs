using BepuPhysics;
using OpenTK.Mathematics;

namespace MutaBrains.Core.Import.AssimpLoader
{
    class AssimpAnimatedModel : AssimpModel
    {
        public AssimpAnimatedModel(string name, string path, Vector3 startPosition, Simulation simulation) : base(name, path, startPosition, simulation)
        {
        }
    }
}
