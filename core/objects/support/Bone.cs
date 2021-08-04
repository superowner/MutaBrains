using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;

namespace MutaBrains.Core.Objects.Support
{
    struct KeyPosition
    {
        public Vector3 position;
        public double timeStamp;
    }

    struct KeyRotation
    {
        public OpenTK.Mathematics.Quaternion orientation;
        public double timeStamp;
    }

    struct KeyScale
    {
        public Vector3 scale;
        public double timeStamp;
    }

    class Bone
    {
        public int m_NumPositions;
        public int m_NumRotations;
        public int m_NumScalings;

        public Matrix4 m_LocalTransform;

        public List<KeyPosition> m_Positions;
        public List<KeyRotation> m_Rotations;
        public List<KeyScale> m_Scales;
        
        public string m_Name;
        public int m_ID;

        public Bone(string name, int id, NodeAnimationChannel channel)
        {
            m_Name = name;
            m_ID = id;

            m_LocalTransform = Matrix4.Identity;

            m_Positions = new List<KeyPosition>();
            m_Rotations = new List<KeyRotation>();
            m_Scales = new List<KeyScale>();

            m_NumPositions = channel.PositionKeyCount;
            for (int positionIndex = 0; positionIndex < m_NumPositions; ++positionIndex)
            {
                KeyPosition keyPositionData = new KeyPosition() {
                    position = GLConverter.FromVector3(channel.PositionKeys[positionIndex].Value),
                    timeStamp = (float)channel.PositionKeys[positionIndex].Time
                };
                m_Positions.Add(keyPositionData);
            }

            m_NumRotations = channel.RotationKeyCount;
            for (int rotationIndex = 0; rotationIndex < m_NumRotations; ++rotationIndex)
            {
                KeyRotation keyRotationData = new KeyRotation()
                {
                    orientation = GLConverter.FromQuaternion(channel.RotationKeys[rotationIndex].Value),
                    timeStamp = (float)channel.RotationKeys[rotationIndex].Time
                };
                m_Rotations.Add(keyRotationData);
            }

            m_NumScalings = channel.ScalingKeyCount;
            for (int scaleIndex = 0; scaleIndex < m_NumScalings; ++scaleIndex)
            {
                KeyScale keyScaleData = new KeyScale()
                {
                    scale = GLConverter.FromVector3(channel.ScalingKeys[scaleIndex].Value),
                    timeStamp = (float)channel.ScalingKeys[scaleIndex].Time
                };
                m_Scales.Add(keyScaleData);
            }
        }

        public int GetPositionIndex(double animationTime)
        {
            for (int index = 0; index < m_NumPositions - 1; ++index)
            {
                if (animationTime < m_Positions[index + 1].timeStamp)
                    return index;
            }

            return 0;
        }

        public int GetRotationIndex(double animationTime)
        {
            for (int index = 0; index < m_NumRotations - 1; ++index)
            {
                if (animationTime < m_Rotations[index + 1].timeStamp)
                    return index;
            }

            return 0;
        }

        public int GetScaleIndex(double animationTime)
        {
            for (int index = 0; index < m_NumScalings - 1; ++index)
            {
                if (animationTime < m_Scales[index + 1].timeStamp)
                    return index;
            }

            return 0;
        }

        private double GetScaleFactor(double lastTimeStamp, double nextTimeStamp, double animationTime)
        {
            double midWayLength = animationTime - lastTimeStamp;
            double framesDiff = nextTimeStamp - lastTimeStamp;
            double scaleFactor = midWayLength / framesDiff;
            
            return scaleFactor;
        }

        private Matrix4 InterpolatePosition(double animationTime)
        {
            if (1 == m_NumPositions)
            {
                return Matrix4.CreateTranslation(m_Positions[0].position);
            }

            int p0Index = GetPositionIndex(animationTime);
            int p1Index = p0Index + 1;
            double scaleFactor = GetScaleFactor(m_Positions[p0Index].timeStamp, m_Positions[p1Index].timeStamp, animationTime);
            Vector3 finalPosition = Vector3.Lerp(m_Positions[p0Index].position, m_Positions[p1Index].position, (float)scaleFactor);

            return Matrix4.CreateTranslation(finalPosition);
        }

        private Matrix4 InterpolateRotation(double animationTime)
        {
            if (1 == m_NumRotations)
            {
                return Matrix4.CreateFromQuaternion(m_Rotations[0].orientation.Normalized());
            }

            int p0Index = GetRotationIndex(animationTime);
            int p1Index = p0Index + 1;
            double scaleFactor = GetScaleFactor(m_Rotations[p0Index].timeStamp, m_Rotations[p1Index].timeStamp, animationTime);
            OpenTK.Mathematics.Quaternion finalRotation = OpenTK.Mathematics.Quaternion.Slerp(m_Rotations[p0Index].orientation, m_Rotations[p1Index].orientation, (float)scaleFactor);

            return Matrix4.CreateFromQuaternion(finalRotation.Normalized());

        }

        private Matrix4 InterpolateScaling(double animationTime)
        {
            if (1 == m_NumScalings)
            {
                return Matrix4.CreateScale(m_Scales[0].scale);
            }

            int p0Index = GetScaleIndex(animationTime);
            int p1Index = p0Index + 1;
            double scaleFactor = GetScaleFactor(m_Scales[p0Index].timeStamp, m_Scales[p1Index].timeStamp, animationTime);
            Vector3 finalScale = Vector3.Lerp(m_Scales[p0Index].scale, m_Scales[p1Index].scale, (float)scaleFactor);

            return Matrix4.CreateScale(finalScale);
        }

        public void Update(double animationTime)
        {
            Matrix4 translation = InterpolatePosition(animationTime);
            Matrix4 rotation = InterpolateRotation(animationTime);
            Matrix4 scale = InterpolateScaling(animationTime);

            m_LocalTransform = translation * rotation * scale;
        }
    }
}
