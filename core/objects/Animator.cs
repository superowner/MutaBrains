using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Assimp;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects
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

            if (currentAnimation.model.BoneInfoMap.ContainsKey(nodeName))
            {
                int index = currentAnimation.model.BoneInfoMap[nodeName].id;
                Matrix4 offset = currentAnimation.model.BoneInfoMap[nodeName].offset;
                Transforms[index] = globalTransformation * offset;
            }

            for (int i = 0; i < node.childrenCount; i++)
            {
                CalculateBoneTransform(node.children[i], globalTransformation);
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
