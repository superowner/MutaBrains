using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Assimp;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Objects.Support;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MutaBrains.Core.Objects
{
    class AnimatedObject3D : Object3D
    {
        public AnimatedObject3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
        }
    }
}
