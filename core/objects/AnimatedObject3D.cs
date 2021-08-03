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

    class AnimatedObject3D : Object3D
    {
        public const int MAX_BONES = 100;
        public const int MAX_BONE_INFLUENCE = 4;

        Animation animation;
        Animator animator;
        float lastFrame = 0;

        public Dictionary<string, BoneInfo> BoneInfoMap = new Dictionary<string, BoneInfo>();
        public int BoneCounter = 0;

        List<BonedNormalTextureVertex> all_vertices;
        List<uint> all_indices;

        private void SetVertexBoneDataToDefault(ref BonedNormalTextureVertex vertex)
        {
            vertex.BoneIDs = new int[MAX_BONE_INFLUENCE];
            vertex.BoneWeights = new float[MAX_BONE_INFLUENCE];

            for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
            {
                vertex.BoneIDs[i] = -1;
                vertex.BoneWeights[i] = 0.0f;
            }
        }

        public AnimatedObject3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
            animation = new Animation(this);
            animator = new Animator(animation);
        }

        protected override void Initialize(string name, string path, Vector3 position, Vector3 scale)
        {
            this.name = name;
            this.path = path;
            this.position = position;
            this.scale = scale;

            AssimpContext importer = new AssimpContext();
            scene = importer.ImportFile(path,
                PostProcessSteps.GenerateBoundingBoxes |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.SortByPrimitiveType
            );
            //aiProcess_Triangulate | aiProcess_GenSmoothNormals | aiProcess_FlipUVs | aiProcess_CalcTangentSpace

            ProcessMeshes();

            int sizeOfvertex = Marshal.SizeOf(typeof(BonedNormalTextureVertex));

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, all_vertices.Count * sizeOfvertex, all_vertices.ToArray(), BufferUsageHint.DynamicDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            //int indexBuffer = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, all_indices.Count * sizeof(uint), all_indices.ToArray(), BufferUsageHint.DynamicDraw);

            int positionLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, sizeOfvertex, Marshal.OffsetOf(typeof(BonedNormalTextureVertex), "Position"));
            GL.EnableVertexAttribArray(positionLocation);

            int normalLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, sizeOfvertex, Marshal.OffsetOf(typeof(BonedNormalTextureVertex), "Normal"));
            GL.EnableVertexAttribArray(normalLocation);

            int texCoordLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, sizeOfvertex, Marshal.OffsetOf(typeof(BonedNormalTextureVertex), "Texture"));
            GL.EnableVertexAttribArray(texCoordLocation);

            int boneIdsLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aBoneIDs");
            GL.VertexAttribIPointer(boneIdsLocation, 4, VertexAttribIntegerType.Int, sizeOfvertex, Marshal.OffsetOf(typeof(BonedNormalTextureVertex), "BoneIDs"));
            GL.EnableVertexAttribArray(boneIdsLocation);

            int boneWeightsLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aBoneWeights");
            GL.VertexAttribPointer(boneWeightsLocation, 4, VertexAttribPointerType.Float, false, sizeOfvertex, Marshal.OffsetOf(typeof(BonedNormalTextureVertex), "BoneWeights"));
            GL.EnableVertexAttribArray(boneWeightsLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public override void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            RefreshMatrices();

            animator.UpdateAnimation((float)time);
        }

        protected override void RefreshVertexBuffer()
        {
            ProcessMeshes();

            int sizeOfvertex = Marshal.SizeOf(typeof(BonedNormalTextureVertex));
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, all_vertices.Count * sizeOfvertex, all_vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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

                for (int i = 0; i < animator.Transforms.Count; i++)
                {
                    ShaderManager.simpleAnimationShader.SetMatrix4v2("finalBonesTransformations[" + i + "]", animator.Transforms[i]);
                }

                //GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, all_indices.Count, DrawElementsType.UnsignedInt, 0);
                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, all_vertices.Count / 3);


                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }

        protected override void ProcessMeshes()
        {
            all_vertices = new List<BonedNormalTextureVertex>();
            all_indices = new List<uint>();

            //foreach (Mesh mesh in scene.Meshes)
            //{
            //    List<BonedNormalTextureVertex> vertices = new List<BonedNormalTextureVertex>();
            //    List<uint> indicesList = new List<uint>();

            //    for (int i = 0; i < mesh.VertexCount; i++)
            //    {
            //        BonedNormalTextureVertex vertex = new BonedNormalTextureVertex();
            //        SetVertexBoneDataToDefault(ref vertex);

            //        vertex.Position = FromVector(mesh.Vertices[i]);
            //        vertex.Normal = FromVector(mesh.Normals[i]);

            //        Material material = scene.Materials[mesh.MaterialIndex];
            //        int diff_texture_index = material.TextureDiffuse.TextureIndex;

            //        if (mesh.HasTextureCoords(diff_texture_index))
            //        {
            //            float tex_x = mesh.TextureCoordinateChannels[diff_texture_index][0].X;
            //            float tex_y = mesh.TextureCoordinateChannels[diff_texture_index][0].Y;
                        
            //            vertex.Texture = new Vector2(tex_x, tex_y);
            //        }
            //        else
            //        {
            //            vertex.Texture = Vector2.Zero;
            //        }

            //        vertices.Add(vertex);
            //    }

            //    int[] indices = mesh.GetIndices();
            //    for (int i = 0; i < indices.Length; i++)
            //    {
            //        indicesList.Add(Convert.ToUInt32(indices[i]));
            //    }


            //    //for (int i = 0; i < mesh.FaceCount; i++)
            //    //{
            //    //    Face face = mesh.Faces[i];
            //    //    for (int j = 0; j < face.IndexCount; j++)
            //    //    {
            //    //        indices.Add((uint)face.Indices[j]);
            //    //    }
            //    //}

            //    ExtractBoneWeightForVertices(vertices, mesh);

            //    all_vertices.AddRange(vertices);
            //    all_indices.AddRange(indicesList);
            //}

            // 1233333333333333333333



            List<float> vertList = new List<float>();

            foreach (Mesh mesh in scene.Meshes)
            {
                List<BonedNormalTextureVertex> vertices = new List<BonedNormalTextureVertex>();

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

                    BonedNormalTextureVertex vertex1 = new BonedNormalTextureVertex();
                    SetVertexBoneDataToDefault(ref vertex1);
                    vertex1.Position = FromVector(vertex_1);
                    vertex1.Normal = FromVector(normal_1);
                    vertex1.Texture = new Vector2(texture_1.X, texture_1.Y);
                    vertices.Add(vertex1);

                    BonedNormalTextureVertex vertex2 = new BonedNormalTextureVertex();
                    SetVertexBoneDataToDefault(ref vertex2);
                    vertex2.Position = FromVector(vertex_2);
                    vertex2.Normal = FromVector(normal_2);
                    vertex2.Texture = new Vector2(texture_2.X, texture_2.Y);
                    vertices.Add(vertex2);

                    BonedNormalTextureVertex vertex3 = new BonedNormalTextureVertex();
                    SetVertexBoneDataToDefault(ref vertex3);
                    vertex3.Position = FromVector(vertex_3);
                    vertex3.Normal = FromVector(normal_3);
                    vertex3.Texture = new Vector2(texture_3.X, texture_3.Y);
                    vertices.Add(vertex3);

                    float[] array = new float[] {
                        vertex_1.X, vertex_1.Y, vertex_1.Z, normal_1.X, normal_1.Y, normal_1.Z, texture_1.X, texture_1.Y,
                        vertex_2.X, vertex_2.Y, vertex_2.Z, normal_2.X, normal_2.Y, normal_2.Z, texture_2.X, texture_2.Y,
                        vertex_3.X, vertex_3.Y, vertex_3.Z, normal_3.X, normal_3.Y, normal_3.Z, texture_3.X, texture_3.Y
                    };

                    vertList.AddRange(array);
                }

                ExtractBoneWeightForVertices(vertices, mesh);

                all_vertices.AddRange(vertices);
            }

            vertices = vertList.ToArray();
        }

        private void SetVertexBoneData(BonedNormalTextureVertex vertex, int boneID, float weight)
        {
            for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
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
            for (int boneIndex = 0; boneIndex < mesh.BoneCount; ++boneIndex)
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
                        offset = FromMatrix(mesh.Bones[boneIndex].OffsetMatrix)
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

        private Vector3 FromVector(Vector3D vector)
        {
            return new Vector3
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
        }
    }
}
