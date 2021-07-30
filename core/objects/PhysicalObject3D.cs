using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Assimp;
using BepuPhysics;
using BepuPhysics.Collidables;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Managers;

namespace MutaBrains.Core.Objects
{
    class PhysicalObject3D : Object3D
    {
        protected BodyHandle physicalBodyHandle;
        protected OpenTK.Mathematics.Quaternion quaternion = OpenTK.Mathematics.Quaternion.Identity;
        protected Vector3 objectSize;

        public PhysicalObject3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
            Initialize(name, path, position, scale);

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

        protected override void InitializeVertices()
        {
            List<float> vertList = new List<float>();

            objectSize = new Vector3(float.MinValue);

            foreach (Assimp.Mesh mesh in scene.Meshes)
            {
                Material material = scene.Materials[mesh.MaterialIndex];
                int diff_texture_index = material.TextureDiffuse.TextureIndex;
                List<Vector3D> textures = mesh.TextureCoordinateChannels[diff_texture_index];
                texture = Texture.LoadTexture(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), material.TextureDiffuse.FilePath));

                foreach (Face face in mesh.Faces)
                {
                    int vert_index_1 = face.Indices[0];
                    int vert_index_2 = face.Indices[1];
                    int vert_index_3 = face.Indices[2];

                    Vector3D vertex_1 = mesh.Vertices[vert_index_1];
                    Vector3D vertex_2 = mesh.Vertices[vert_index_2];
                    Vector3D vertex_3 = mesh.Vertices[vert_index_3];

                    Vector3D normal_1 = mesh.Normals[vert_index_1];
                    Vector3D normal_2 = mesh.Normals[vert_index_2];
                    Vector3D normal_3 = mesh.Normals[vert_index_3];

                    Vector3D texture_1 = new Vector3D(0);
                    Vector3D texture_2 = new Vector3D(0);
                    Vector3D texture_3 = new Vector3D(0);

                    if (mesh.HasTextureCoords(diff_texture_index))
                    {
                        texture_1 = textures[vert_index_1];
                        texture_2 = textures[vert_index_2];
                        texture_3 = textures[vert_index_3];
                    }

                    float[] array = new float[] {
                        vertex_1.X, vertex_1.Y, vertex_1.Z, normal_1.X, normal_1.Y, normal_1.Z, texture_1.X, texture_1.Y,
                        vertex_2.X, vertex_2.Y, vertex_2.Z, normal_2.X, normal_2.Y, normal_2.Z, texture_2.X, texture_2.Y,
                        vertex_3.X, vertex_3.Y, vertex_3.Z, normal_3.X, normal_3.Y, normal_3.Z, texture_3.X, texture_3.Y
                    };

                    vertList.AddRange(array);
                }

                float x_size = mesh.BoundingBox.Max.X - mesh.BoundingBox.Min.X;
                float y_size = mesh.BoundingBox.Max.Y - mesh.BoundingBox.Min.Y;
                float z_size = mesh.BoundingBox.Max.Z - mesh.BoundingBox.Min.Z;

                if (x_size > objectSize.X) objectSize.X = x_size;
                if (y_size > objectSize.Y) objectSize.Y = y_size;
                if (z_size > objectSize.Z) objectSize.Z = z_size;
            }

            vertices = vertList.ToArray();

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
