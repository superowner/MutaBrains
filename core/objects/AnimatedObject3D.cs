using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Assimp;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects
{
    public class BoneLineInfo
    {
        public float Length;
        public Vector3 Point;
        public Vector3 Direction;
    }

    public class BoneInfo
    {
        public List<BoneLineInfo> Lines = new List<BoneLineInfo>();
        public Vector3 Point;
        public Matrix4 BoneOffset = Matrix4.Identity;
        public Matrix4 FinalTransformation = Matrix4.Identity;
    }

    public class AnimationInfo
    {
        public string Title;

        public string Sound = string.Empty;

        public bool IsPose;
        public AnimationInfo(string title, bool isPose)
        {
            Title = title;
            IsPose = isPose;
        }

        public float TicksPerSecond;
        public float DurationInTicks;
        public Dictionary<string, NodeAnimation> AnimationNodes = new Dictionary<string, NodeAnimation>();
    }

    public class VectorInfo
    {
        public float Time;
        public Vector3 Value;
    }

    public class QuaternionInfo
    {
        public float Time;
        public Assimp.Quaternion Value;
    }

    public class NodeAnimation
    {
        public int PositionKeysCount;
        public int RotationKeysCount;
        public int ScalingKeysCount;

        public VectorInfo[] PositionKeys;
        public QuaternionInfo[] RotationKeys;
        public VectorInfo[] ScalingKeys;
    }

    internal class AnimatedNode
    {
        public Matrix4 Transform = Matrix4.Identity;
        public string Name = "";
        public AnimatedNode Parent;
        public List<AnimatedNode> Childs = new List<AnimatedNode>();
    }

    class AnimatedObject3D : Object3D
    {
        private AnimatedNode rootNode;
        private AnimationInfo currentAnimation;

        public AnimationInfo СurrentAnimation
        {
            get
            {
                return currentAnimation;
            }
            set
            {
                currentAnimation = value;
            }
        }

        public readonly Dictionary<string, AnimationInfo> Animations = new Dictionary<string, AnimationInfo>();
        public readonly Dictionary<string, AnimationInfo> Poses = new Dictionary<string, AnimationInfo>();
        private readonly Dictionary<string, int> bonesMapping = new Dictionary<string, int>();
        private Matrix4 m_globalInverseTransform = Matrix4.Identity;

        public List<BoneInfo> Bones = new List<BoneInfo>();
        public Matrix4[] AnimationTransform = new Matrix4[80];

        public AnimatedObject3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
            LoadBodyMeshes(scene, scene.RootNode, null);
            LoadBonesInfo(rootNode);
            LoadAnimations(scene, "");
        }

        protected override void InitializeVertices()
        {
            Bones.Clear();
            bonesMapping.Clear();
            Animations.Clear();
            Poses.Clear();
            rootNode = null;
            СurrentAnimation = null;

            m_globalInverseTransform = FromMatrix(scene.RootNode.Transform);
            m_globalInverseTransform.Invert();

            List<float> vertList = new List<float>();

            List<float> vertexBoneIndices = new List<float>(100);
            List<float> vertexBoneWeights = new List<float>(100);

            foreach (Mesh mesh in scene.Meshes)
            {
                Material material = scene.Materials[mesh.MaterialIndex];
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

                foreach (Face face in mesh.Faces)
                {
                    int vert_index_1 = face.Indices[0];
                    int vert_index_2 = face.Indices[1];
                    int vert_index_3 = face.Indices[2];

                    Vector3D vertex_1 = mesh.Vertices[vert_index_1];
                    Vector3D vertex_2 = mesh.Vertices[vert_index_2];
                    Vector3D vertex_3 = mesh.Vertices[vert_index_3];

                    Vector3D normal_1 = mesh.Normals[vert_index_1];
                    Vector3D normal_2 = mesh.Normals[vert_index_2];
                    Vector3D normal_3 = mesh.Normals[vert_index_3];

                    Vector3D texture_1 = new Vector3D(0);
                    Vector3D texture_2 = new Vector3D(0);
                    Vector3D texture_3 = new Vector3D(0);

                    if (mesh.HasTextureCoords(diff_texture_index))
                    {
                        texture_1 = textures[vert_index_1];
                        texture_2 = textures[vert_index_2];
                        texture_3 = textures[vert_index_3];
                    }

                    float[] array = new float[] {
                        vertex_1.X, vertex_1.Y, vertex_1.Z, normal_1.X, normal_1.Y, normal_1.Z, texture_1.X, texture_1.Y,
                        vertex_2.X, vertex_2.Y, vertex_2.Z, normal_2.X, normal_2.Y, normal_2.Z, texture_2.X, texture_2.Y,
                        vertex_3.X, vertex_3.Y, vertex_3.Z, normal_3.X, normal_3.Y, normal_3.Z, texture_3.X, texture_3.Y
                    };

                    vertList.AddRange(array);

                    vertexBoneIndices.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
                    vertexBoneWeights.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
                }

                if (mesh.HasBones)
                {
                    foreach (Bone bone in mesh.Bones)
                    {
                        if (!bone.HasVertexWeights) continue;

                        int boneIndex;
                        
                        if (bonesMapping.ContainsKey(bone.Name))
                        {
                            boneIndex = bonesMapping[bone.Name];
                        }
                        else
                        {
                            boneIndex = Bones.Count;
                            BoneInfo boneInfo = new BoneInfo
                            {
                                BoneOffset = FromMatrix(bone.OffsetMatrix),
                            };
                            Bones.Add(boneInfo);
                            bonesMapping.Add(bone.Name, boneIndex);
                        }
                        
                        foreach (VertexWeight weight in bone.VertexWeights)
                        {
                            int vi = weight.VertexID * 4;
                            for (int i = 0; i < 4; i++)
                            {
                                if (vertexBoneWeights[vi + i] == 0.0f)
                                {
                                    vertexBoneIndices[vi + i] = boneIndex;
                                    vertexBoneWeights[vi + i] = weight.Weight;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            vertices = vertList.ToArray();
        }

        private void LoadBodyMeshes(Scene scene, Node node, AnimatedNode animationNode)
        {
            AnimatedNode parentNode = animationNode;
            if (animationNode == null)
            {
                rootNode = new AnimatedNode();
                parentNode = rootNode;
            }

            parentNode.Name = node.Name;
            parentNode.Transform = FromMatrix(node.Transform);

            for (int i = 0; i < node.ChildCount; i++)
            {
                AnimatedNode childNode = new AnimatedNode
                {
                    Parent = parentNode
                };
                LoadBodyMeshes(scene, node.Children[i], childNode);
                parentNode.Childs.Add(childNode);
            }
        }
        private void LoadBonesInfo(AnimatedNode node, BoneInfo parent = null)
        {
            if (bonesMapping.ContainsKey(node.Name))
            {
                Vector3 point = Vector3.Zero;
                int boneIndex = bonesMapping[node.Name];
                BoneInfo bone = Bones[boneIndex];
                Matrix4 transform = m_globalInverseTransform * bone.BoneOffset;
                transform.Transpose();

                //bone.Point = -Vector3.Transform(point, transform);
                bone.Point = -Vector3.TransformVector(point, transform);
#warning DON'T KNOW WHAT THIS TRANFORMATION FOR!

                if (parent != null)
                {
                    BoneLineInfo line = new BoneLineInfo
                    {
                        Point = bone.Point,
                        Direction = bone.Point - parent.Point
                    };
                    line.Length = line.Direction.Length;
                    line.Direction /= line.Length;
                    parent.Lines.Add(line);
                }
                foreach (AnimatedNode child in node.Childs)
                {
                    LoadBonesInfo(child, bone);
                }
            }
            else
            {
                foreach (AnimatedNode child in node.Childs)
                {
                    LoadBonesInfo(child, parent);
                }
            }
        }
        private void FillAnimationNode(Scene scene, Node node, int animationIndex)
        {
            Animation animation = scene.Animations[animationIndex];
            NodeAnimationChannel channel = null;
            for (int i = 0; i < animation.NodeAnimationChannelCount; i++)
            {
                if (animation.NodeAnimationChannels[i].NodeName == node.Name)
                    channel = animation.NodeAnimationChannels[i];
            }
            if (channel != null && !СurrentAnimation.AnimationNodes.ContainsKey(node.Name))
            {
                NodeAnimation nodeAnim = new NodeAnimation
                {
                    PositionKeysCount = channel.PositionKeyCount,
                    ScalingKeysCount = channel.ScalingKeyCount,
                    RotationKeysCount = channel.RotationKeyCount
                };
                nodeAnim.PositionKeys = new VectorInfo[nodeAnim.PositionKeysCount];
                for (int i = 0; i < nodeAnim.PositionKeysCount; i++)
                    nodeAnim.PositionKeys[i] = new VectorInfo
                    {
                        Time = (float)channel.PositionKeys[i].Time,
                        Value = FromVector(channel.PositionKeys[i].Value)
                    };
                nodeAnim.RotationKeys = new QuaternionInfo[nodeAnim.RotationKeysCount];
                for (int i = 0; i < nodeAnim.RotationKeysCount; i++)
                    nodeAnim.RotationKeys[i] = new QuaternionInfo
                    {
                        Time = (float)channel.RotationKeys[i].Time,
                        Value = channel.RotationKeys[i].Value
                    };
                nodeAnim.ScalingKeys = new VectorInfo[nodeAnim.ScalingKeysCount];
                for (int i = 0; i < nodeAnim.ScalingKeysCount; i++)
                    nodeAnim.ScalingKeys[i] = new VectorInfo
                    {
                        Time = (float)channel.ScalingKeys[i].Time,
                        Value = FromVector(channel.ScalingKeys[i].Value)
                    };
                СurrentAnimation.AnimationNodes.Add(node.Name, nodeAnim);
            }
            for (int i = 0; i < node.ChildCount; i++)
                FillAnimationNode(scene, node.Children[i], animationIndex);
        }
        private AnimationInfo LoadAnimations(Scene scene, string title, bool pose = false)
        {
            //AnimationInfo prevAnimation = СurrentAnimation;
            AnimationInfo animation = default(AnimationInfo);
            if (scene.AnimationCount == 0)
                return animation;
            const int firstAnimationIndex = 0;

            //for (int i = 0; i < scene.AnimationCount; i++)            // in one file always one animation
            {
                animation = new AnimationInfo(title, pose);
                СurrentAnimation = animation;
                animation.TicksPerSecond = (float)scene.Animations[firstAnimationIndex].TicksPerSecond;
                animation.DurationInTicks = (float)scene.Animations[firstAnimationIndex].DurationInTicks;
                FillAnimationNode(scene, scene.RootNode, firstAnimationIndex);
                if (pose)
                    Poses.Add(title, animation);
                else
                    Animations.Add(title, animation);
            }
            // СurrentAnimation = prevAnimation;
            return animation;
        }

        private int FindPosition(float animationTime, NodeAnimation channel)
        {
            for (int i = 0; i < channel.PositionKeysCount; i++)
                if (animationTime < channel.PositionKeys[i].Time)
                {
                    return i;
                }
            return 0;
        }
        private int FindRotation(float animationTime, NodeAnimation channel)
        {
            for (int i = 0; i < channel.RotationKeysCount; i++)
                if (animationTime < channel.RotationKeys[i].Time)
                {
                    return i;
                }
            return 0;
        }
        private int FindScaling(float animationTime, NodeAnimation channel)
        {
            for (int i = 0; i < channel.ScalingKeysCount; i++)
                if (animationTime < channel.ScalingKeys[i].Time)
                {
                    return i;
                }
            return 0;
        }

        private Vector3 CalcInterpolatedPosition(float animationTime, NodeAnimation channel)
        {
            if (channel.PositionKeysCount == 1)
            {
                return channel.PositionKeys[0].Value;
            }

            int positionIndex = FindPosition(animationTime, channel);
            int nextPositionIndex = positionIndex + 1;
            if (nextPositionIndex < channel.PositionKeysCount)
            {
                VectorInfo positionKey = channel.PositionKeys[positionIndex];
                VectorInfo nextPositionKey = channel.PositionKeys[nextPositionIndex];
                float deltaTime = nextPositionKey.Time - positionKey.Time;
                float factor = (animationTime - positionKey.Time) / deltaTime;
                return positionKey.Value + (nextPositionKey.Value - positionKey.Value) * factor;
            }
            return channel.PositionKeys[0].Value;
        }
        private Matrix4 CalcInterpolatedRotation(float animationTime, NodeAnimation channel)
        {
            Matrix4x4 matrix;
            if (channel.ScalingKeysCount == 1)
            {
                matrix = channel.RotationKeys[0].Value.GetMatrix();
                return FromMatrix(matrix);
            }

            int rotateIndex = FindRotation(animationTime, channel);
            int nextRotateIndex = rotateIndex + 1;
            
            if (nextRotateIndex < channel.RotationKeysCount)
            {
                QuaternionInfo rotateKey = channel.RotationKeys[rotateIndex];
                QuaternionInfo nextRotateKey = channel.RotationKeys[nextRotateIndex];
                float deltaTime = nextRotateKey.Time - rotateKey.Time;
                float factor = (animationTime - rotateKey.Time) / deltaTime;
                matrix = Assimp.Quaternion.Slerp(rotateKey.Value, nextRotateKey.Value, factor).GetMatrix();
            }
            else
            {
                matrix = channel.RotationKeys[0].Value.GetMatrix();
            }

            return FromMatrix(matrix);
        }
        private Vector3 CalcInterpolatedScaling(float animationTime, NodeAnimation channel)
        {
            if (channel.ScalingKeysCount == 1)
            {
                return channel.ScalingKeys[0].Value;
            }

            int scaleIndex = FindScaling(animationTime, channel);
            int nextScaleIndex = scaleIndex + 1;
            if (nextScaleIndex < channel.ScalingKeysCount)
            {
                VectorInfo scaleKey = channel.ScalingKeys[scaleIndex];
                VectorInfo nextScaleKey = channel.ScalingKeys[nextScaleIndex];
                float deltaTime = nextScaleKey.Time - scaleKey.Time;
                float factor = (animationTime - scaleKey.Time) / deltaTime;
                return scaleKey.Value + (nextScaleKey.Value - scaleKey.Value) * factor;
            }
            return channel.ScalingKeys[0].Value;
        }

        private void ReadNodeHeirarchy(float animationTime, AnimatedNode node, Matrix4 parentTransform)
        {
            NodeAnimation channel = СurrentAnimation.AnimationNodes.ContainsKey(node.Name) ? СurrentAnimation.AnimationNodes[node.Name] : null;
            Matrix4 nodeTransformation = node.Transform;
            if (channel != null)
            {
                Vector3 scale = CalcInterpolatedScaling(animationTime, channel);
                Matrix4 scaleMatrix = Matrix4.CreateScale(scale);

                Matrix4 rotateMatrix = CalcInterpolatedRotation(animationTime, channel);

                Vector3 translate = CalcInterpolatedPosition(animationTime, channel);

                nodeTransformation = rotateMatrix * scaleMatrix;
                nodeTransformation.M14 = translate.X;
                nodeTransformation.M24 = translate.Y;
                nodeTransformation.M34 = translate.Z;
            }
            Matrix4 globalTransformation = parentTransform * nodeTransformation;

            if (bonesMapping.ContainsKey(node.Name))
            {
                int boneIndex = bonesMapping[node.Name];
                Bones[boneIndex].FinalTransformation = m_globalInverseTransform * globalTransformation * Bones[boneIndex].BoneOffset;
                Bones[boneIndex].FinalTransformation.Transpose();
            }

            foreach (AnimatedNode child in node.Childs)
            {
                ReadNodeHeirarchy(animationTime, child, globalTransformation);
            }
        }

        private Vector3 FromVector(Vector3D vector)
        {
            return new Vector3
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
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

        private bool PoseBonesTransform(float animationTime)     // track value / 100f
        {
            if (СurrentAnimation == null)
                return false;
            ReadNodeHeirarchy(animationTime * СurrentAnimation.DurationInTicks, rootNode, Matrix4.Identity);

            for (var i = 0; i < Bones.Count; i++)
            {
                AnimationTransform[i] = Bones[i].FinalTransformation;
            }
            return true;
        }

        private bool BonesTransform(float timeInSeconds)
        {
            if (СurrentAnimation == null)
                return false;
            var ticksPerSecond = СurrentAnimation.TicksPerSecond != 0 ? СurrentAnimation.TicksPerSecond : 25.0f;
            var timeInTicks = timeInSeconds * ticksPerSecond;
            var animationTime = timeInTicks % СurrentAnimation.DurationInTicks;

            ReadNodeHeirarchy(animationTime, rootNode, Matrix4.Identity);

            for (var i = 0; i < Bones.Count; i++)
            {
                AnimationTransform[i] = Bones[i].FinalTransformation;
            }
            return true;
        }

        public override void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            base.Update(time, mouseState, keyboardState);

            BonesTransform((float)time);
        }

        public override void Draw(double time)
        {
            if (visible)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Ccw);

                GL.BindVertexArray(vertexArray);
                if (diffuseTexture != null)
                {
                    diffuseTexture.Use(TextureUnit.Texture0);
                }
                if (specularTexture != null)
                {
                    specularTexture.Use(TextureUnit.Texture1);
                }

                ShaderManager.simpleAnimationShader.Use();
                ShaderManager.simpleAnimationShader.SetMatrix4("model", modelMatrix);
                ShaderManager.simpleAnimationShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                ShaderManager.simpleAnimationShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());
                ShaderManager.simpleAnimationShader.SetVector3("viewPosition", CameraManager.Perspective.Position);
                // Material
                ShaderManager.simpleAnimationShader.SetInt("material.diffuse", 0);
                ShaderManager.simpleAnimationShader.SetInt("material.specular", 1);
                ShaderManager.simpleAnimationShader.SetFloat("material.shininess", 2.0f);
                // Directional light
                ShaderManager.simpleAnimationShader.SetVector3("dirLight.direction", new Vector3(-.1f));
                ShaderManager.simpleAnimationShader.SetVector3("dirLight.ambient", new Vector3(0.1f));
                ShaderManager.simpleAnimationShader.SetVector3("dirLight.diffuse", new Vector3(5.0f));
                ShaderManager.simpleAnimationShader.SetVector3("dirLight.specular", new Vector3(1.0f));
                // Point light
                //ShaderManager.simpleAnimationShader.SetVector3("pointLight.position", lightPos);
                // ShaderManager.simpleAnimationShader.SetVector3("pointLight.position", LightManager.GetLight("point").Position);
                // ShaderManager.simpleAnimationShader.SetVector3("pointLight.ambient", LightManager.GetLight("point").Ambient);
                // ShaderManager.simpleAnimationShader.SetVector3("pointLight.diffuse", LightManager.GetLight("point").Diffuse);
                // ShaderManager.simpleAnimationShader.SetVector3("pointLight.specular", LightManager.GetLight("point").Specular);
                // ShaderManager.simpleAnimationShader.SetFloat("pointLight.constant", LightManager.GetLight("point").Constant);
                // ShaderManager.simpleAnimationShader.SetFloat("pointLight.linear", LightManager.GetLight("point").Linear);
                // ShaderManager.simpleAnimationShader.SetFloat("pointLight.quadratic", LightManager.GetLight("point").Quadratic);
                // Spot light
                //ShaderManager.simpleAnimationShader.SetVector3("spotLight.position", CameraManager.Perspective.Position);
                //ShaderManager.simpleAnimationShader.SetVector3("spotLight.direction", CameraManager.Perspective.Front);
                // ShaderManager.simpleAnimationShader.SetVector3("spotLight.position", LightManager.GetLight("spot").Position);
                // ShaderManager.simpleAnimationShader.SetVector3("spotLight.direction", LightManager.GetLight("spot").Direction);
                // ShaderManager.simpleAnimationShader.SetVector3("spotLight.ambient", LightManager.GetLight("spot").Ambient);
                // ShaderManager.simpleAnimationShader.SetVector3("spotLight.diffuse", LightManager.GetLight("spot").Diffuse);
                // ShaderManager.simpleAnimationShader.SetVector3("spotLight.specular", LightManager.GetLight("spot").Specular);
                // ShaderManager.simpleAnimationShader.SetFloat("spotLight.constant", LightManager.GetLight("spot").Constant);
                // ShaderManager.simpleAnimationShader.SetFloat("spotLight.linear", LightManager.GetLight("spot").Linear);
                // ShaderManager.simpleAnimationShader.SetFloat("spotLight.quadratic", LightManager.GetLight("spot").Quadratic);
                // ShaderManager.simpleAnimationShader.SetFloat("spotLight.cutOff", LightManager.GetLight("spot").CutOff);
                // ShaderManager.simpleAnimationShader.SetFloat("spotLight.outerCutOff", LightManager.GetLight("spot").CutOffOuter);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, vertices.Length / vertexLength);


                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }
    }
}
