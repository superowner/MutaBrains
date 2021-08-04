using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects.Support
{
    struct BoneInfo
    {
        public int ID; // index vershiny v VertexInfos
        public string Name;
        public Matrix4 OffsetMatrix;
    }

    struct VertexInfo
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public Vector4 BoneIDs;
        public Vector4 BoneWeights;
    }

    struct NodeData
    {
        public Matrix4 Transformation;
        public string Name;
        public int ChildrensCount;
        public List<NodeData> Childrens;
    }

    class AnimatedMeshObject : MeshObject
    {
        public List<Matrix4> FinalBonesTransformations;
        public List<VertexInfo> VertexInfos;
        public List<BoneInfo> BoneInfos;

        private List<Assimp.Animation> animations;
        private Node rootNode;
        private double animationCurrentTime = 0;
        private Assimp.Animation currentAnimation;
        private NodeData rootNodeData = new NodeData();
        public List<Bone> Bones;

        public AnimatedMeshObject(Mesh mesh, List<Assimp.Animation> animations, Node rootNode) : base(mesh)
        {
            this.animations = animations;
            this.rootNode = rootNode;

            currentAnimation = this.animations[0];

            prefillBoneTransformations();
            ReadNodesHierarchy(ref rootNodeData, rootNode);
            SetupBones(currentAnimation);
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
            VertexInfos = new List<VertexInfo>();
            BoneInfos = new List<BoneInfo>();

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
                    Vector4 boneIDs = Vector4.Zero;
                    Vector4 boneWeights = Vector4.Zero;

                    fillBoneData(faceIndex, ref boneIDs, ref boneWeights);

                    VertexInfos.Add(new VertexInfo { Position = position, Normal = normal, Texture = texture, BoneIDs = boneIDs, BoneWeights = boneWeights });

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

        private void fillBoneData(int faceIndex, ref Vector4 boneIDs, ref Vector4 boneWeights)
        {
            boneIDs = new Vector4(-1);
            boneWeights = new Vector4(0);

            int idsCounter = 0;

            foreach (Assimp.Bone bone in mesh.Bones)
            {
                List<VertexWeight> boneVertWeights = bone.VertexWeights.FindAll(vw => vw.VertexID == faceIndex);

                for (int i = 0; i < boneVertWeights.Count; i++)
                {
                    if (idsCounter < 4)
                    {
                        BoneInfos.Add(new BoneInfo { ID = faceIndex, Name = bone.Name, OffsetMatrix = GLConverter.FromMatrix(bone.OffsetMatrix) });

                        switch (idsCounter)
                        {
                            case 0:
                                boneIDs.X = boneVertWeights[i].VertexID;
                                boneWeights.X = boneVertWeights[i].Weight;
                                break;
                            case 1:
                                boneIDs.Y = boneVertWeights[i].VertexID;
                                boneWeights.Y = boneVertWeights[i].Weight;
                                break;
                            case 2:
                                boneIDs.Z = boneVertWeights[i].VertexID;
                                boneWeights.Z = boneVertWeights[i].Weight;
                                break;
                            case 3:
                                boneIDs.W = boneVertWeights[i].VertexID;
                                boneWeights.W = boneVertWeights[i].Weight;
                                break;
                        }

                        idsCounter++;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        public override void Update(double time)
        {
            animationCurrentTime += currentAnimation.TicksPerSecond * time;
            animationCurrentTime = animationCurrentTime % currentAnimation.DurationInTicks;

            CalculateBoneTransform(rootNodeData, Matrix4.Identity);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Animation time: " + Math.Round(animationCurrentTime, 2));
        }

        private void ReadNodesHierarchy(ref NodeData rootNodeData, Node rootNode)
        {
            rootNodeData.Name = rootNode.Name;
            rootNodeData.ChildrensCount = rootNode.ChildCount;
            rootNodeData.Transformation = GLConverter.FromMatrix(rootNode.Transform);

            if (rootNodeData.Childrens == null)
            {
                rootNodeData.Childrens = new List<NodeData>();
            }

            foreach (Node children in rootNode.Children)
            {
                NodeData newNodeData = new NodeData();
                ReadNodesHierarchy(ref newNodeData, children);
                rootNodeData.Childrens.Add(newNodeData);
            }
        }

        private void SetupBones(Assimp.Animation animation)
        {
            Bones = new List<Bone>();
            foreach (NodeAnimationChannel channel in animation.NodeAnimationChannels)
            {
                Bones.Add(new Bone(channel.NodeName, channel));
            }
        }

        private void CalculateBoneTransform(NodeData node, Matrix4 parentTransform)
        {
            Matrix4 nodeTransform = node.Transformation;

            Bone Bone = Bones.Find(b => b.name == node.Name);
            if (Bone != null)
            {
                Bone.Update(animationCurrentTime);
                nodeTransform = Bone.localTransform;
            }

            Matrix4 globalTransformation = nodeTransform;

            for (int i = 0; i < 100; i++)
            {
                FinalBonesTransformations[i] = globalTransformation;
            }

            // if (currentAnimation.model.BoneInfoMap.ContainsKey(nodeName))
            // {
            //     int index = currentAnimation.model.BoneInfoMap[nodeName].id;
            //     Matrix4 offset = currentAnimation.model.BoneInfoMap[nodeName].offset;
            //     Transforms[index] = globalTransformation * offset;                    // TRANSFORMS
            // }

            foreach (NodeData child in node.Childrens)
            {
                CalculateBoneTransform(child, globalTransformation);
            }
        }
    }
}