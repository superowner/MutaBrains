using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;

namespace MutaBrains.Core.Objects
{
    struct AssimpNodeData
    {
        public Matrix4 transformation;
        public string name;
        public int childrenCount;
        public List<AssimpNodeData> children;
    }

    class Animation
    {
        public float duration;
        public float ticksPerSecond;
        public List<Bone> Bones;
        public AssimpNodeData rootNode;
        public AnimatedObject3D model;

        public Animation(AnimatedObject3D model)
        {
            this.model = model;
            Assimp.Animation animation = model.scene.Animations[0];
            duration = (float)animation.DurationInTicks;
            ticksPerSecond = (float)animation.TicksPerSecond;
            Matrix4x4 globalTransformation = model.scene.RootNode.Transform;
            globalTransformation.Inverse();

            ReadHeirarchyData(ref rootNode, model.scene.RootNode);
            SetupBones(animation, model);
        }

        private void ReadHeirarchyData(ref AssimpNodeData dest, Node src)
        {
            dest.name = src.Name;
            dest.transformation = FromMatrix(src.Transform);
            dest.childrenCount = src.ChildCount;

            if (dest.children == null)
            {
                dest.children = new List<AssimpNodeData>();
            }

            for (int i = 0; i < src.ChildCount; i++)
            {
                AssimpNodeData newData = new AssimpNodeData();
                ReadHeirarchyData(ref newData, src.Children[i]);
                dest.children.Add(newData);
            }
        }

        private void SetupBones(Assimp.Animation animation, AnimatedObject3D model)
        {
            Bones = new List<Bone>();
            int size = animation.NodeAnimationChannelCount;

            for (int i = 0; i < size; i++)
            {
                NodeAnimationChannel channel = animation.NodeAnimationChannels[i];
                string boneName = channel.NodeName;

                if (!model.BoneInfoMap.ContainsKey(boneName))
                {
                    BoneInfo boneInfo = new BoneInfo() { id = model.BoneCounter };

                    model.BoneInfoMap.Add(boneName, boneInfo);
                    model.BoneCounter++;
                }

                Bones.Add(new Bone(channel.NodeName, model.BoneInfoMap[channel.NodeName].id, channel));
            }
        }

        private Matrix4 FromMatrix(Matrix4x4 mat)
        {
            Matrix4 m = new Matrix4();
            m.M11 = mat.A1;
            m.M12 = mat.A2;
            m.M13 = mat.A3;
            m.M14 = mat.A4;
            m.M21 = mat.B1;
            m.M22 = mat.B2;
            m.M23 = mat.B3;
            m.M24 = mat.B4;
            m.M31 = mat.C1;
            m.M32 = mat.C2;
            m.M33 = mat.C3;
            m.M34 = mat.C4;
            m.M41 = mat.D1;
            m.M42 = mat.D2;
            m.M43 = mat.D3;
            m.M44 = mat.D4;

            return m;
        }
    }
}
