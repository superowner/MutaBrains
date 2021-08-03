using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;

namespace MutaBrains.Core.Objects.Support
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
            dest.transformation = GLConverter.FromMatrix(src.Transform);
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

            // for (int i = 0; i < size; i++)
            // {
            //     NodeAnimationChannel channel = animation.NodeAnimationChannels[i];
            //     string boneName = channel.NodeName;

            //     if (!model.BoneInfoMap.ContainsKey(boneName))
            //     {
            //         BoneInfo boneInfo = new BoneInfo() { id = model.BoneCounter };

            //         model.BoneInfoMap.Add(boneName, boneInfo);
            //         model.BoneCounter++;
            //     }

            //     Bones.Add(new Bone(channel.NodeName, model.BoneInfoMap[channel.NodeName].id, channel));
            // }
        }
    }
}
