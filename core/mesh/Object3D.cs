using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Import.ObjLoader;
using MutaBrains.Core.Managers;
using System.Collections.Generic;

namespace MutaBrains.Core.Mesh
{
    public class Object3D
    {
        protected float[] vertices;
        protected int vertexBuffer;
        protected int vertexArray;
        protected int vertexLength;

        protected Vector3 position;
        protected float angle = 0.0f;
        protected Vector3 scale = Vector3.One;

        protected Matrix4 rotationMatrix;
        protected Matrix4 scaleMatrix;
        protected Matrix4 translationMatrix;
        protected Matrix4 modelMatrix;

        protected bool visible = true;

        protected SimpleMesh mesh;

        public Object3D(SimpleMesh mesh, Vector3 position)
        {
            Initialize(mesh, position);
        }

        public virtual void Initialize(SimpleMesh mesh, Vector3 position)
        {
            this.position = position;
            this.mesh = mesh;
            vertexLength = 8;

            // scale = new Vector3(0.2f);

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
            List<float> vertsList = new List<float>();
            int faceIndexCount = mesh.facesNormsIndxs.Count;

            for (int i = 0; i < faceIndexCount; i++)
            {
                var faceVerts = mesh.facesVertsIndxs[i];
                var faceNormals = mesh.facesVertsIndxs[i];
                var faceTextures = mesh.facesVertsIndxs[i];

                for (int faceIndex = 0; faceIndex < 3; faceIndex++)
                {
                    int vertex = faceVerts[faceIndex];
                    int normal = faceNormals[faceIndex];
                    int texture = faceTextures[faceIndex];

                    vertsList.Add(mesh.vertices[vertex - 1].X);
                    vertsList.Add(mesh.vertices[vertex - 1].Y);
                    vertsList.Add(mesh.vertices[vertex - 1].Z);

                    if (mesh.normals.Count > 0) {
                        vertsList.Add(mesh.normals[normal - 1].X);
                        vertsList.Add(mesh.normals[normal - 1].Y);
                        vertsList.Add(mesh.normals[normal - 1].Z);
                    } else {
                        vertsList.Add(0);
                        vertsList.Add(1);
                        vertsList.Add(0);
                    }

                    if (mesh.uvw.Count > 0)
                    {
                        vertsList.Add(mesh.uvw[texture - 1].X);
                        vertsList.Add(mesh.uvw[texture - 1].Y);
                    } else {
                        vertsList.Add(0);
                        vertsList.Add(0);
                    }
                }
            }

            vertices = vertsList.ToArray();

            RefreshMatrices();
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
            rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angle));
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        public virtual void Update(double time, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            float step = 10.0f * (float)time;
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                position.X += step;
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                position.X -= step;
            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                position.Y += step;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {
                position.Y -= step;
            }

            angle += 45.0f * (float)time;
            RefreshMatrices();
        }

        public virtual void Draw(double time)
        {
            if (visible)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Ccw);

                GL.BindVertexArray(vertexArray);

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
                ShaderManager.simpleMeshShader.SetVector3("dirLight.ambient",   new Vector3(0.1f));
                ShaderManager.simpleMeshShader.SetVector3("dirLight.diffuse",   new Vector3(5.0f));
                ShaderManager.simpleMeshShader.SetVector3("dirLight.specular",  new Vector3(1.0f));
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

                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / vertexLength);
                GL.FrontFace(FrontFaceDirection.Cw);
            }
        }
    }
}
