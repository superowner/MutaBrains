using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects.Support
{
    class AnimatedMeshObject : MeshObject
    {
        public AnimatedMeshObject(Mesh mesh) : base(mesh)
        {
        }
    }
}