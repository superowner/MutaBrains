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
                PostProcessSteps.GenerateBoundingBoxes |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.SortByPrimitiveType);

            vertexLength = 16;

            ProcessMeshes();

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

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
                Material material = scene.Materials[mesh.MaterialIndex];

                AnimatedMeshObject meshObject = new AnimatedMeshObject(mesh, scene.Animations);
                meshObject.ParseMesh(material, path);

                List<float> verticesList = new List<float>();
                if (vertices != null)
                {
                    verticesList.AddRange(vertices);
                }
                verticesList.AddRange(meshObject.vertices);
                vertices = verticesList.ToArray();

                animatedMeshes.Add(meshObject);
            }
        }

        public override void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            base.Update(time, mouseState, keyboardState);

            foreach (AnimatedMeshObject mesh in animatedMeshes)
            {
                mesh.Update(time);
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

                    if (mesh.specularTexture != null)
                    {
                        mesh.specularTexture.Use(TextureUnit.Texture1);
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
                    ShaderManager.simpleAnimationShader.SetVector3("dirLight.diffuse", new Vector3(1.0f));
                    ShaderManager.simpleAnimationShader.SetVector3("dirLight.specular", new Vector3(1.0f));

                    for (int i = 0; i < mesh.FinalBonesTransformations.Count; i++)
                    {
                        ShaderManager.simpleAnimationShader.SetMatrix4Raw("finalBonesTransformations[" + i + "]", mesh.FinalBonesTransformations[i]);
                    }

                    GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, vertices.Length / vertexLength);
                }

                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }
    }
}
