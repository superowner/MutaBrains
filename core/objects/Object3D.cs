using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Assimp;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Objects.Support;

namespace MutaBrains.Core.Objects
{
    class Object3D
    {
        protected float[] vertices;
        protected uint[] indices;

        protected int indexBuffer;
        protected int vertexBuffer;
        protected int vertexArray;
        protected int vertexLength;

        protected Vector3 position;
        protected Vector3 scale = Vector3.One;
        protected Matrix4 rotationMatrix;
        protected Matrix4 scaleMatrix;
        protected Matrix4 translationMatrix;
        protected Matrix4 modelMatrix;

        protected List<MeshObject> meshes;

        protected Vector3 objectSize;

        public Scene scene;
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

            int positionLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            int normalLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(normalLocation);

            int texCoordLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected virtual void ProcessMeshes()
        {
            meshes = new List<MeshObject>();

            foreach (Mesh mesh in scene.Meshes)
            {
                Material material = scene.Materials[mesh.MaterialIndex];

                MeshObject meshObject = new MeshObject(mesh);
                meshObject.ParseMesh(material, path);

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

                meshes.Add(meshObject);

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

        protected virtual void RefreshVertexBuffer()
        {
            ProcessMeshes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected virtual void RefreshMatrices()
        {
            rotationMatrix = Matrix4.CreateRotationX(0);
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

                foreach (MeshObject mesh in meshes)
                {
                    if (mesh.diffuseTexture != null)
                    {
                        mesh.diffuseTexture.Use(TextureUnit.Texture0);
                    }
                    if (mesh.specularTexture != null)
                    {
                        mesh.specularTexture.Use(TextureUnit.Texture1);
                    }

                    ShaderManager.simpleMeshShader.Use();
                    ShaderManager.simpleMeshShader.SetMatrix4("model", modelMatrix);
                    ShaderManager.simpleMeshShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                    ShaderManager.simpleMeshShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());
                    ShaderManager.simpleMeshShader.SetVector3("viewPosition", CameraManager.Perspective.Position);
                    // Material
                    ShaderManager.simpleMeshShader.SetInt("material.diffuse", 0);
                    ShaderManager.simpleMeshShader.SetInt("material.specular", 1);
                    ShaderManager.simpleMeshShader.SetFloat("material.shininess", 2.0f);
                    // Directional light
                    ShaderManager.simpleMeshShader.SetVector3("dirLight.direction", new Vector3(-.1f));
                    ShaderManager.simpleMeshShader.SetVector3("dirLight.ambient", new Vector3(0.1f));
                    ShaderManager.simpleMeshShader.SetVector3("dirLight.diffuse", new Vector3(1.0f));
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

                    //GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, vertices.Length / vertexLength);
                    GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                }

                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }
    }
}
