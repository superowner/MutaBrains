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
        protected Vector3 objectSize;

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

        protected override void ProcessMeshes()
        {
            meshes = new List<MeshObject>();
            objectSize = new Vector3(float.MinValue);

            foreach (Assimp.Mesh mesh in scene.Meshes)
            {
                Material material = scene.Materials[mesh.MaterialIndex];

                MeshObject meshObject = new MeshObject(mesh);
                meshObject.ParseMesh(material, path);

                List<float> verticesList = new List<float>();
                if (vertices != null)
                {
                    verticesList.AddRange(vertices);
                }
                verticesList.AddRange(meshObject.vertices);
                vertices = verticesList.ToArray();
                meshes.Add(meshObject);

                float x_size = mesh.BoundingBox.Max.X - mesh.BoundingBox.Min.X;
                float y_size = mesh.BoundingBox.Max.Y - mesh.BoundingBox.Min.Y;
                float z_size = mesh.BoundingBox.Max.Z - mesh.BoundingBox.Min.Z;

                if (x_size > objectSize.X) objectSize.X = x_size;
                if (y_size > objectSize.Y) objectSize.Y = y_size;
                if (z_size > objectSize.Z) objectSize.Z = z_size;
            }

            objectSize.X *= scale.X;
            objectSize.Y *= scale.Y;
            objectSize.Z *= scale.Z;
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
