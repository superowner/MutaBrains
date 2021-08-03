using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;

namespace MutaBrains.Core.Objects
{
    struct KeyPosition
    {
        public Vector3 position;
        public float timeStamp;
    }

    struct KeyRotation
    {
        public OpenTK.Mathematics.Quaternion orientation;
        public float timeStamp;
    }

    struct KeyScale
    {
        public Vector3 scale;
        public float timeStamp;
    }

    class Bone
    {
        public int numPositions;
        public int numRotations;
        public int numScalings;

        public Matrix4 localTransform;

        public List<KeyPosition> Positions;
        public List<KeyRotation> Rotations;
        public List<KeyScale> Scales;
        
        public string name;
        public int id;
        public NodeAnimationChannel channel;

        public Bone(string name, int id, NodeAnimationChannel channel)
        {
            this.name = name;
            this.id = id;
            this.channel = channel;

            localTransform = Matrix4.Identity;

            Positions = new List<KeyPosition>();
            Rotations = new List<KeyRotation>();
            Scales = new List<KeyScale>();

            numPositions = channel.PositionKeyCount;
            for (int positionIndex = 0; positionIndex < numPositions; ++positionIndex)
            {
                KeyPosition keyPositionData = new KeyPosition() {
                    position = GLConverter.FromVector3(channel.PositionKeys[positionIndex].Value),
                    timeStamp = (float)channel.PositionKeys[positionIndex].Time
                };
                Positions.Add(keyPositionData);
            }

            numRotations = channel.RotationKeyCount;
            for (int rotationIndex = 0; rotationIndex < numRotations; ++rotationIndex)
            {
                KeyRotation keyRotationData = new KeyRotation()
                {
                    orientation = GLConverter.FromQuaternion(channel.RotationKeys[rotationIndex].Value),
                    timeStamp = (float)channel.RotationKeys[rotationIndex].Time
                };
                Rotations.Add(keyRotationData);
            }

            numScalings = channel.ScalingKeyCount;
            for (int scaleIndex = 0; scaleIndex < numScalings; ++scaleIndex)
            {
                KeyScale keyScaleData = new KeyScale()
                {
                    scale = GLConverter.FromVector3(channel.ScalingKeys[scaleIndex].Value),
                    timeStamp = (float)channel.ScalingKeys[scaleIndex].Time
                };
                Scales.Add(keyScaleData);
            }
        }

        public int GetPositionIndex(float animationTime)
        {
            for (int index = 0; index < numPositions - 1; ++index)
            {
                if (animationTime < Positions[index + 1].timeStamp)
                    return index;
            }

            return 0;
        }

        public int GetRotationIndex(float animationTime)
        {
            for (int index = 0; index < numRotations - 1; ++index)
            {
                if (animationTime < Rotations[index + 1].timeStamp)
                    return index;
            }

            return 0;
        }

        public int GetScaleIndex(float animationTime)
        {
            for (int index = 0; index < numScalings - 1; ++index)
            {
                if (animationTime < Scales[index + 1].timeStamp)
                    return index;
            }

            return 0;
        }

        private float GetScaleFactor(float lastTimeStamp, float nextTimeStamp, float animationTime)
        {
            float midWayLength = animationTime - lastTimeStamp;
            float framesDiff = nextTimeStamp - lastTimeStamp;
            float scaleFactor = midWayLength / framesDiff;
            
            return scaleFactor;
        }

        private Matrix4 InterpolatePosition(float animationTime)
        {
            if (1 == numPositions)
            {
                return Matrix4.CreateTranslation(Positions[0].position);
            }

            int p0Index = GetPositionIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor(Positions[p0Index].timeStamp, Positions[p1Index].timeStamp, animationTime);
            Vector3 finalPosition = Vector3.Lerp(Positions[p0Index].position, Positions[p1Index].position, scaleFactor);

            return Matrix4.CreateTranslation(finalPosition);
        }

        private Matrix4 InterpolateRotation(float animationTime)
        {
            if (1 == numRotations)
            {
                return Matrix4.CreateFromQuaternion(Rotations[0].orientation.Normalized());
            }

            int p0Index = GetRotationIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor(Rotations[p0Index].timeStamp, Rotations[p1Index].timeStamp, animationTime);
            OpenTK.Mathematics.Quaternion finalRotation = OpenTK.Mathematics.Quaternion.Slerp(Rotations[p0Index].orientation, Rotations[p1Index].orientation, scaleFactor);

            return Matrix4.CreateFromQuaternion(finalRotation.Normalized());

        }

        private Matrix4 InterpolateScaling(float animationTime)
        {
            if (1 == numScalings)
            {
                return Matrix4.CreateScale(Scales[0].scale);
            }

            int p0Index = GetScaleIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor(Scales[p0Index].timeStamp, Scales[p1Index].timeStamp, animationTime);
            Vector3 finalScale = Vector3.Lerp(Scales[p0Index].scale, Scales[p1Index].scale, scaleFactor);

            return Matrix4.CreateScale(finalScale);
        }

        public void Update(float animationTime)
        {
            Matrix4 translation = InterpolatePosition(animationTime);
            Matrix4 rotation = InterpolateRotation(animationTime);
            Matrix4 scale = InterpolateScaling(animationTime);

            localTransform = translation * rotation * scale;
        }
    }
}
