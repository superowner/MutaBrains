using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MutaBrains.Core.Objects.Support
{
    class Animator
    {
        public List<Matrix4> Transforms;
        Animation currentAnimation;
        float currentTime;
        float deltaTime;

        public Animator(Animation current)
        {
            currentAnimation = current;
            currentTime = 0.0f;
            Transforms = new List<Matrix4>(100);

            for (int i = 0; i < 100; i++)
            {
                Transforms.Add(Matrix4.Identity);
            }
        }

        public void UpdateAnimation(float dt)
        {
            deltaTime = dt;
            if (currentAnimation != null)
            {
                currentTime += currentAnimation.ticksPerSecond * dt;
                currentTime = currentTime % currentAnimation.duration;
                CalculateBoneTransform(currentAnimation.rootNode, Matrix4.Identity);

                Console.WriteLine(currentTime);
            }
        }

        public void PlayAnimation(Animation animation)
        {
            currentAnimation = animation;
            currentTime = 0.0f;
        }

        private void CalculateBoneTransform(AssimpNodeData node, Matrix4 parentTransform)
        {
            string nodeName = node.name;
            Matrix4 nodeTransform = node.transformation;

            Bone Bone = currentAnimation.Bones.Find(b => b.name == nodeName);

            if (Bone != null)
            {
                Bone.Update(currentTime);
                nodeTransform = Bone.localTransform;
            }

            Matrix4 globalTransformation = parentTransform * nodeTransform;

            // if (currentAnimation.model.BoneInfoMap.ContainsKey(nodeName))
            // {
            //     int index = currentAnimation.model.BoneInfoMap[nodeName].id;
            //     Matrix4 offset = currentAnimation.model.BoneInfoMap[nodeName].offset;
            //     Transforms[index] = globalTransformation * offset;
            // }

            for (int i = 0; i < node.childrenCount; i++)
            {
                CalculateBoneTransform(node.children[i], globalTransformation);
            }
        }
    }
}
