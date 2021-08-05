using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects.Support
{
    struct Vertex
    {
        // position
        public Vector3 Position;
        // normal
        public Vector3 Normal;
        // texCoords
        public Vector2 TexCoords;
        //bone indexes which will influence this vertex
        public float[] m_BoneIDs;
        //weights from each bone
        public float[] m_Weights;
    }

    struct BoneInfo
    {
        /*id is index in finalBoneMatrices*/
        public int id;
        /*offset matrix transforms vertex from model space to bone space*/
        public Matrix4 offset;
    }

    struct AssimpNodeData
    {
        public Matrix4 transformation;
        public string name;
        public int childrenCount;
        public List<AssimpNodeData> children;
    }

    class AnimatedMeshObject : MeshObject
    {
        private Dictionary<string, BoneInfo> m_BoneInfoMap = new Dictionary<string, BoneInfo>();
        private int m_BoneCounter = 0;
        private double m_Duration;
        private double m_TicksPerSecond;
        private AssimpNodeData m_RootNode;
        private List<Bone> m_Bones;

        public List<Matrix4> m_FinalBoneMatrices;
        private Animation m_CurrentAnimation;
        private double m_CurrentTime;
        private Node rootNode;

        public AnimatedMeshObject(Mesh mesh, Animation animation, Node rootNode) : base(mesh)
        {
            m_Duration = animation.DurationInTicks;
            m_TicksPerSecond = animation.TicksPerSecond;

            this.rootNode = rootNode;

            m_CurrentTime = 0.0;
            m_CurrentAnimation = animation;
            m_FinalBoneMatrices = new List<Matrix4>(100);

            for (int i = 0; i < 100; i++)
            {
                m_FinalBoneMatrices.Add(Matrix4.Identity);
            }
        }

        public override void ParseMesh(Material material, string path, int offset)
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

            List<Vertex> m_vertices = new List<Vertex>();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();
                SetVertexBoneDataToDefault(ref vertex);
                vertex.Position = GLConverter.FromVector3(mesh.Vertices[i]);
                vertex.Normal = GLConverter.FromVector3(mesh.Normals[i]);
                vertex.TexCoords = mesh.HasTextureCoords(diff_texture_index) ? GLConverter.FromVector3(textures[i]).Xy : Vector2.Zero;

                m_vertices.Add(vertex);
            }

            List<uint> indicesList = new List<uint>();

            foreach (Face face in mesh.Faces)
            {
                for (int i = 0; i < face.IndexCount; i++)
                {
                    indicesList.Add((uint)(face.Indices[i] + offset));
                }
            }

            vertices = meshVertexList.ToArray();
            indices = indicesList.ToArray();

            ExtractBoneWeightForVertices(m_vertices);

            foreach (Vertex v in m_vertices)
            {
                float[] vertexInfoIndexedArray = new float[] {
                    v.Position.X, v.Position.Y, v.Position.Z,
                    v.Normal.X, v.Normal.Y, v.Normal.Z,
                    v.TexCoords.X, v.TexCoords.Y,
                    v.m_BoneIDs[0], v.m_BoneIDs[1], v.m_BoneIDs[2], v.m_BoneIDs[3],
                    v.m_Weights[0], v.m_Weights[1], v.m_Weights[2], v.m_Weights[3],
                };

                meshVertexList.AddRange(vertexInfoIndexedArray);
            }

            vertices = meshVertexList.ToArray();
            indices = indicesList.ToArray();

            ReadHeirarchyData(ref m_RootNode, rootNode);
            SetupBones(m_CurrentAnimation);
        }

        public void UpdateAnimation(double time)
        {
            if (m_CurrentAnimation != null)
            {
                m_CurrentTime += m_TicksPerSecond * time;
                m_CurrentTime = m_CurrentTime % m_Duration;

                Console.WriteLine("Animation time: " + Math.Round(m_CurrentTime, 2));

                CalculateBoneTransform(ref m_RootNode, Matrix4.Identity);
            }
        }

        private void CalculateBoneTransform(ref AssimpNodeData node, Matrix4 parentTransform)
        {
            string nodeName = node.name;
            Matrix4 nodeTransform = node.transformation;

            Bone Bone = FindBone(nodeName);

            if (Bone != null)
            {
                Bone.Update(m_CurrentTime);
                nodeTransform = Bone.m_LocalTransform;
            }

            Matrix4 globalTransformation = parentTransform * nodeTransform;

            if (m_BoneInfoMap.ContainsKey(nodeName))
            {
                int index = m_BoneInfoMap[nodeName].id;
                Matrix4 offset = m_BoneInfoMap[nodeName].offset;
                m_FinalBoneMatrices[index] = globalTransformation * offset;
            }

            for (int i = 0; i < node.childrenCount; i++)
            {
                AssimpNodeData tempNodeChildren = node.children[i];
                CalculateBoneTransform(ref tempNodeChildren, globalTransformation);
                node.children[i] = tempNodeChildren;
            }
        }

        private void ReadHeirarchyData(ref AssimpNodeData dest, Node src)
        {
            dest.name = src.Name;
            dest.transformation = GLConverter.FromMatrix(src.Transform);
            dest.childrenCount = src.ChildCount;

            if (dest.children == null)
            {
                dest.children = new List<AssimpNodeData>();
            }

            foreach (Node children in src.Children)
            {
                AssimpNodeData newData = new AssimpNodeData();
                ReadHeirarchyData(ref newData, children);
                dest.children.Add(newData);
            }
        }

        private void SetupBones(Animation animation)
        {
            m_Bones = new List<Bone>();

            foreach (NodeAnimationChannel channel in animation.NodeAnimationChannels)
            {
                string boneName = channel.NodeName;

                if (!m_BoneInfoMap.ContainsKey(boneName))
                {
                    BoneInfo boneInfo = new BoneInfo();
                    boneInfo.id = m_BoneCounter;
                    m_BoneInfoMap[boneName] = boneInfo;
                    m_BoneCounter++;
                }

                m_Bones.Add(new Bone(channel.NodeName, m_BoneInfoMap[channel.NodeName].id, channel));
            }
        }

        private Bone FindBone(string name)
        {
            return m_Bones.Find(b => b.m_Name == name);
        }

        private void SetVertexBoneDataToDefault(ref Vertex vertex)
        {
            vertex.Position = Vector3.Zero;
            vertex.Normal = Vector3.Zero;
            vertex.TexCoords = Vector2.Zero;
            vertex.m_BoneIDs = new float[4] { -1, -1, -1, -1 };
            vertex.m_Weights = new float[4] { 0, 0, 0, 0 };
        }

        private void ExtractBoneWeightForVertices(List<Vertex> vertices)
        {
            foreach (Assimp.Bone bone in mesh.Bones)
            {
                int boneID;
                string boneName = bone.Name;

                if (!m_BoneInfoMap.ContainsKey(boneName))
                {
                    BoneInfo newBoneInfo;
                    newBoneInfo.id = m_BoneCounter;
                    newBoneInfo.offset = GLConverter.FromMatrix(bone.OffsetMatrix);
                    m_BoneInfoMap[boneName] = newBoneInfo;
                    boneID = m_BoneCounter;
                    m_BoneCounter++;
                }
                else
                {
                    boneID = m_BoneInfoMap[boneName].id;
                }

                int numWeights = bone.VertexWeightCount;

                for (int weightIndex = 0; weightIndex < numWeights; ++weightIndex)
                {
                    int vertexId = bone.VertexWeights[weightIndex].VertexID;
                    float weight = bone.VertexWeights[weightIndex].Weight;

                    Vertex temVertex = vertices[vertexId];
                    SetVertexBoneData(ref temVertex, boneID, weight);
                    vertices[vertexId] = temVertex;
                }
            }
        }

        private void SetVertexBoneData(ref Vertex vertex, int boneID, float weight)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (vertex.m_BoneIDs[i] < 0)
                {
                    vertex.m_Weights[i] = weight;
                    vertex.m_BoneIDs[i] = boneID;
                    break;
                }
            }
        }
    }
}