using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Assimp;
using BepuPhysics;
using BepuPhysics.Collidables;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Objects.Support;

namespace MutaBrains.Core.Objects
{
    class PhysicalObject3D : Object3D
    {
        protected BodyHandle physicalBodyHandle;
        protected OpenTK.Mathematics.Quaternion quaternion = OpenTK.Mathematics.Quaternion.Identity;

        public PhysicalObject3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
            Box shape = new Box(objectSize.X, objectSize.Y, objectSize.Z);
            shape.ComputeInertia(1, out BodyInertia inertia);
            TypedIndex index = PhysicsManager.simulation.Shapes.Add(shape);
            physicalBodyHandle = PhysicsManager.simulation.Bodies.Add(
                BodyDescription.CreateDynamic(
                    new System.Numerics.Vector3(position.X, position.Y, position.Z),
                    inertia,
                    new CollidableDescription(index, 0.1f),
                    new BodyActivityDescription(0.01f)
                )
            );
        }

        protected override void RefreshMatrices()
        {
            rotationMatrix = Matrix4.CreateFromQuaternion(quaternion);
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        public override void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            BodyReference bodyReference = PhysicsManager.simulation.Bodies.GetBodyReference(physicalBodyHandle);
            position = new Vector3(bodyReference.Pose.Position.X, bodyReference.Pose.Position.Y, bodyReference.Pose.Position.Z);
            quaternion = new OpenTK.Mathematics.Quaternion(bodyReference.Pose.Orientation.X, bodyReference.Pose.Orientation.Y, bodyReference.Pose.Orientation.Z, bodyReference.Pose.Orientation.W);

            RefreshMatrices();
        }
    }
}
