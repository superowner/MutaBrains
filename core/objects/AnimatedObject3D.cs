using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Assimp;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Objects.Support;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MutaBrains.Core.Objects
{
    class AnimatedObject3D : Object3D
    {
        protected List<AnimatedMeshObject> animatedMeshes;

        float x_angle = 0;
        float y_angle = 0;

        public AnimatedObject3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
        }

        protected override void Initialize(string name, string path, Vector3 position, Vector3 scale)
        {
            this.name = name;
            this.path = path;
            this.position = position;
            this.scale = scale;

            AssimpContext importer = new AssimpContext();
            scene = importer.ImportFile(path,
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.GenerateBoundingBoxes |
                PostProcessSteps.GenerateUVCoords
            );

            vertexLength = 16;

            ProcessMeshes();

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

            int positionLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            int normalLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(normalLocation);

            int texCoordLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            int boneIdsLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aBoneIDs");
            GL.VertexAttribPointer(boneIdsLocation, 4, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(boneIdsLocation);

            int boneWeightsLocation = ShaderManager.simpleAnimationShader.GetAttribLocation("aBoneWeights");
            GL.VertexAttribPointer(boneWeightsLocation, 4, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 12 * sizeof(float));
            GL.EnableVertexAttribArray(boneWeightsLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected override void ProcessMeshes()
        {
            animatedMeshes = new List<AnimatedMeshObject>();

            foreach (Mesh mesh in scene.Meshes)
            {
                int parseOffset = (vertices != null) ? (vertices.Length / vertexLength) : 0;

                Material material = scene.Materials[mesh.MaterialIndex];

                AnimatedMeshObject meshObject = new AnimatedMeshObject(mesh, scene.Animations[0], scene.RootNode);
                meshObject.ParseMesh(material, path, parseOffset);

                List<float> verticesList = new List<float>();
                if (vertices != null)
                {
                    verticesList.AddRange(vertices);
                }
                verticesList.AddRange(meshObject.vertices);

                vertices = verticesList.ToArray();

                List<uint> indicesList = new List<uint>();
                if (indices != null)
                {
                    indicesList.AddRange(indices);
                }
                indicesList.AddRange(meshObject.indices);

                indices = indicesList.ToArray();

                animatedMeshes.Add(meshObject);

                float x_size = mesh.BoundingBox.Max.X - mesh.BoundingBox.Min.X;
                float y_size = mesh.BoundingBox.Max.Y - mesh.BoundingBox.Min.Y;
                float z_size = mesh.BoundingBox.Max.Z - mesh.BoundingBox.Min.Z;

                if (x_size > objectSize.X) objectSize.X = x_size;
                if (y_size > objectSize.Y) objectSize.Y = y_size;
                if (z_size > objectSize.Z) objectSize.Z = z_size;
            }

            objectSize.X *= scale.X;
            objectSize.Y *= scale.Y;
            objectSize.Z *= scale.Z;
        }

        protected override void RefreshMatrices()
        {
            rotationMatrix = Matrix4.CreateRotationX(x_angle) * Matrix4.CreateRotationY(y_angle);
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        public override void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            base.Update(time, mouseState, keyboardState);

            if (keyboardState.IsKeyDown(Keys.Left)) {
                y_angle -= 1.0f * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.Right)) {
                y_angle += 1.0f * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.Up)) {
                x_angle -= 1.0f * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.Down)) {
                x_angle += 1.0f * (float)time;
            }

            foreach (AnimatedMeshObject mesh in animatedMeshes)
            {
                mesh.UpdateAnimation(time);
            }
        }

        public override void Draw(double time)
        {
            if (visible)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Ccw);

                GL.BindVertexArray(vertexArray);

                foreach (AnimatedMeshObject mesh in animatedMeshes)
                {
                    if (mesh.diffuseTexture != null)
                    {
                        mesh.diffuseTexture.Use(TextureUnit.Texture0);
                    }
                    //if (mesh.specularTexture != null)
                    //{
                    //    mesh.specularTexture.Use(TextureUnit.Texture1);
                    //}

                    ShaderManager.simpleAnimationShader.Use();
                    ShaderManager.simpleAnimationShader.SetMatrix4("model", modelMatrix);
                    ShaderManager.simpleAnimationShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                    ShaderManager.simpleAnimationShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());

                    List<Matrix4> transforms = mesh.m_FinalBoneMatrices;
                    for (int i = 0; i < transforms.Count; ++i)
                    {
                        ShaderManager.simpleAnimationShader.SetMatrix4Raw("finalBonesMatrices[" + i + "]", transforms[i]);
                    }

                    GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                }

                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }
    }
}
