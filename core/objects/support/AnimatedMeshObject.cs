using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects.Support
{
    struct BonedNormalTextureVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;

        public int[] BoneIDs;
        public float[] BoneWeights;
    }

    struct BoneInfo
    {
        // id is index in finalBoneMatrices
        public int id;
        // offset matrix transforms vertex from model space to bone space
        public Matrix4 offset;
    }

    class AnimatedMeshObject : MeshObject
    {
        public Dictionary<string, BoneInfo> BoneInfoMap = new Dictionary<string, BoneInfo>();
        public int BoneCounter = 0;
        public List<Matrix4> FinalBonesTransformations;

        public AnimatedMeshObject(Mesh mesh) : base(mesh)
        {
            prefillBoneTransformations();
        }

        private void prefillBoneTransformations()
        {
            FinalBonesTransformations = new List<Matrix4>(100);

            for (int i = 0; i < 100; i++)
            {
                FinalBonesTransformations.Add(Matrix4.Identity);
            }
        }

        public override void ParseMesh(Material material, string path)
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

                    Vector4 boneIDs = new Vector4(-1);
                    Vector4 boneWeights = Vector4.Zero;

                    if (mesh.HasTextureCoords(diff_texture_index))
                    {
                        texture = GLConverter.FromVector3(textures[faceIndex]).Xy;
                    }

                    float[] faceIndexArray = new float[] {
                        position.X, position.Y, position.Z,
                        normal.X, normal.Y, normal.Z,
                        texture.X, texture.Y,
                        boneIDs.X, boneIDs.Y, boneIDs.Z, boneIDs.W,
                        boneWeights.X, boneWeights.Y, boneWeights.Z, boneWeights.W
                    };

                    meshVertexList.AddRange(faceIndexArray);
                }
            }

            vertices = meshVertexList.ToArray();
        }

        private void SetVertexBoneDataToDefault(ref BonedNormalTextureVertex vertex)
        {
            vertex.BoneIDs = new int[4];
            vertex.BoneWeights = new float[4];

            for (int i = 0; i < 4; i++)
            {
                vertex.BoneIDs[i] = -1;
                vertex.BoneWeights[i] = 0.0f;
            }
        }

        private void SetVertexBoneData(BonedNormalTextureVertex vertex, int boneID, float weight)
        {
            for (int i = 0; i < 4; i++)
            {
                if (vertex.BoneIDs[i] < 0)
                {
                    vertex.BoneIDs[i] = boneID;
                    vertex.BoneWeights[i] = weight;
                    break;
                }
            }
        }

        private void ExtractBoneWeightForVertices(List<BonedNormalTextureVertex> vertices, Mesh mesh)
        {
            for (int boneIndex = 0; boneIndex < mesh.BoneCount; boneIndex++)
            {
                int boneID;
                string boneName = mesh.Bones[boneIndex].Name;

                if (BoneInfoMap.ContainsKey(boneName))
                {
                    boneID = BoneInfoMap[boneName].id;
                }
                else
                {
                    BoneInfo newBoneInfo = new BoneInfo()
                    {
                        id = BoneCounter,
                        offset = GLConverter.FromMatrix(mesh.Bones[boneIndex].OffsetMatrix)
                    };
                    BoneInfoMap.Add(boneName, newBoneInfo);
                    boneID = BoneCounter;

                    BoneCounter++;
                }

                List<VertexWeight> weights = mesh.Bones[boneIndex].VertexWeights;
                int numWeights = mesh.Bones[boneIndex].VertexWeightCount;

                for (int weightIndex = 0; weightIndex < numWeights; ++weightIndex)
                {
                    int vertexID = weights[weightIndex].VertexID;
                    float weight = weights[weightIndex].Weight;

                    SetVertexBoneData(vertices[vertexID], boneID, weight);
                }
            }
        }
    }
}