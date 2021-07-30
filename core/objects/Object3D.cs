﻿using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Assimp;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects
{
    class Object3D
    {
        protected float[] vertices;
        protected int vertexBuffer;
        protected int vertexArray;
        protected int vertexLength;

        protected Vector3 position;
        protected Vector3 scale = Vector3.One;

        protected Matrix4 rotationMatrix;
        protected Matrix4 scaleMatrix;
        protected Matrix4 translationMatrix;
        protected Matrix4 modelMatrix;

        protected Texture texture;
        protected Scene scene;

        public string name;
        public bool visible = true;
        public string path;

        public Object3D(string name, string path, Vector3 position, Vector3 scale)
        {
            Initialize(name, path, position, scale);
        }

        protected virtual void Initialize(string name, string path, Vector3 position, Vector3 scale)
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
                PostProcessSteps.SortByPrimitiveType);

            vertexLength = 8;

            InitializeVertices();

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            int vertexLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(vertexLocation);

            int normalLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(normalLocation);

            int texCoordLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected virtual void InitializeVertices()
        {
            List<float> vertList = new List<float>();

            foreach (Mesh mesh in scene.Meshes)
            {
                Material material = scene.Materials[mesh.MaterialIndex];
                int diff_texture_index = material.TextureDiffuse.TextureIndex;
                List<Vector3D> textures = mesh.TextureCoordinateChannels[diff_texture_index];
                texture = Texture.LoadTexture(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), material.TextureDiffuse.FilePath));

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
                }
            }

            vertices = vertList.ToArray();
        }

        protected virtual void RefreshVertexBuffer()
        {
            InitializeVertices();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected virtual void RefreshMatrices()
        {
            rotationMatrix = Matrix4.Identity;
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        public virtual void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            RefreshMatrices();
        }

        public virtual void Draw(double time)
        {
            if (visible)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Ccw);

                GL.BindVertexArray(vertexArray);
                texture.Use();

                ShaderManager.simpleMeshShader.Use();
                ShaderManager.simpleMeshShader.SetMatrix4("model", modelMatrix);
                ShaderManager.simpleMeshShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                ShaderManager.simpleMeshShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());
                ShaderManager.simpleMeshShader.SetVector3("viewPosition", CameraManager.Perspective.Position);
                // Material
                ShaderManager.simpleMeshShader.SetInt("material.diffuse", 0);
                ShaderManager.simpleMeshShader.SetInt("material.specular", 0);
                ShaderManager.simpleMeshShader.SetFloat("material.shininess", 2.0f);
                // Directional light
                ShaderManager.simpleMeshShader.SetVector3("dirLight.direction", new Vector3(-.1f));
                ShaderManager.simpleMeshShader.SetVector3("dirLight.ambient", new Vector3(0.1f));
                ShaderManager.simpleMeshShader.SetVector3("dirLight.diffuse", new Vector3(5.0f));
                ShaderManager.simpleMeshShader.SetVector3("dirLight.specular", new Vector3(1.0f));
                // Point light
                //ShaderManager.simpleMeshShader.SetVector3("pointLight.position", lightPos);
                // ShaderManager.simpleMeshShader.SetVector3("pointLight.position", LightManager.GetLight("point").Position);
                // ShaderManager.simpleMeshShader.SetVector3("pointLight.ambient", LightManager.GetLight("point").Ambient);
                // ShaderManager.simpleMeshShader.SetVector3("pointLight.diffuse", LightManager.GetLight("point").Diffuse);
                // ShaderManager.simpleMeshShader.SetVector3("pointLight.specular", LightManager.GetLight("point").Specular);
                // ShaderManager.simpleMeshShader.SetFloat("pointLight.constant", LightManager.GetLight("point").Constant);
                // ShaderManager.simpleMeshShader.SetFloat("pointLight.linear", LightManager.GetLight("point").Linear);
                // ShaderManager.simpleMeshShader.SetFloat("pointLight.quadratic", LightManager.GetLight("point").Quadratic);
                // Spot light
                //ShaderManager.simpleMeshShader.SetVector3("spotLight.position", CameraManager.Perspective.Position);
                //ShaderManager.simpleMeshShader.SetVector3("spotLight.direction", CameraManager.Perspective.Front);
                // ShaderManager.simpleMeshShader.SetVector3("spotLight.position", LightManager.GetLight("spot").Position);
                // ShaderManager.simpleMeshShader.SetVector3("spotLight.direction", LightManager.GetLight("spot").Direction);
                // ShaderManager.simpleMeshShader.SetVector3("spotLight.ambient", LightManager.GetLight("spot").Ambient);
                // ShaderManager.simpleMeshShader.SetVector3("spotLight.diffuse", LightManager.GetLight("spot").Diffuse);
                // ShaderManager.simpleMeshShader.SetVector3("spotLight.specular", LightManager.GetLight("spot").Specular);
                // ShaderManager.simpleMeshShader.SetFloat("spotLight.constant", LightManager.GetLight("spot").Constant);
                // ShaderManager.simpleMeshShader.SetFloat("spotLight.linear", LightManager.GetLight("spot").Linear);
                // ShaderManager.simpleMeshShader.SetFloat("spotLight.quadratic", LightManager.GetLight("spot").Quadratic);
                // ShaderManager.simpleMeshShader.SetFloat("spotLight.cutOff", LightManager.GetLight("spot").CutOff);
                // ShaderManager.simpleMeshShader.SetFloat("spotLight.outerCutOff", LightManager.GetLight("spot").CutOffOuter);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, vertices.Length / vertexLength);


                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }
    }
}
