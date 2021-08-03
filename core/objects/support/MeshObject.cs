using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects.Support
{
    class MeshObject
    {
        public float[] vertices;
        public Texture diffuseTexture = null;
        public Texture specularTexture = null;
        protected Mesh mesh;

        public MeshObject(Mesh mesh)
        {
            this.mesh = mesh;
        }

        public virtual void ParseMesh(Material material, string path)
        {
            int diff_texture_index = material.TextureDiffuse.TextureIndex;
            List<Vector3D> textures = mesh.TextureCoordinateChannels[diff_texture_index];

            if (material.HasTextureDiffuse)
            {
                diffuseTexture = Texture.LoadTexture(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), material.TextureDiffuse.FilePath));
            }
            if (material.HasTextureSpecular)
            {
                specularTexture = Texture.LoadTexture(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), material.TextureSpecular.FilePath));
            }

            List<float> meshVertexList = new List<float>();

            foreach (Face face in mesh.Faces)
            {
                foreach (int faceIndex in face.Indices)
                {
                    Vector3 position = GLConverter.FromVector3(mesh.Vertices[faceIndex]);
                    Vector3 normal = GLConverter.FromVector3(mesh.Normals[faceIndex]);
                    Vector2 texture = Vector2.Zero;
                    
                    if (mesh.HasTextureCoords(diff_texture_index))
                    {
                        texture = GLConverter.FromVector3(textures[faceIndex]).Xy;
                    }

                    float[] faceIndexArray = new float[] {
                        position.X, position.Y, position.Z,
                        normal.X, normal.Y, normal.Z,
                        texture.X, texture.Y
                    };

                    meshVertexList.AddRange(faceIndexArray);
                }
            }

            vertices = meshVertexList.ToArray();
        }

        public virtual void Update(double time)
        {

        }
    }
}