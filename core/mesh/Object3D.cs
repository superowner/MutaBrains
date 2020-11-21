using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MutaBrains.Core.Import.ObjLoader;
using MutaBrains.Core.Managers;

namespace MutaBrains.Core.Mesh
{
    public class Object3D
    {
        protected float[] vertices;
        protected int vertexBuffer;
        protected int vertexArray;
        protected int vertexLength;

        protected uint[] vertexIndices;
        protected int vertexIndicesBuffer;

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
            vertexLength = 3;

            // scale = new Vector3(0.2f);

            InitializeVertices();

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            vertexIndicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexIndicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vertexIndices.Length * sizeof(uint), vertexIndices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            int vertexLocation = ShaderManager.simpleMeshShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(vertexLocation);

            // int normalLocation = ShaderManager.guiShader.GetAttribLocation("aNormal");
            // GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            // GL.EnableVertexAttribArray(normalLocation);

            // int texCoordLocation = ShaderManager.guiShader.GetAttribLocation("aTexture");
            // GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 6 * sizeof(float));
            // GL.EnableVertexAttribArray(texCoordLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected virtual void InitializeVertices()
        {
            vertices = new float[mesh.vertices.Count * 3];
            int vertex_counter = 0;
            foreach (Vector3 vertex in mesh.vertices)
            {
                vertices[vertex_counter] = vertex.X;
                vertex_counter++;
                vertices[vertex_counter] = vertex.Y;
                vertex_counter++;
                vertices[vertex_counter] = vertex.Z;
                vertex_counter++;
            }

            vertexIndices = new uint[mesh.facesVertsIndxs.Count * 3];
            vertex_counter = 0;
            foreach (var faceVert in mesh.facesVertsIndxs)
            {
                vertexIndices[vertex_counter] = (uint)faceVert[0];
                vertex_counter++;
                vertexIndices[vertex_counter] = (uint)faceVert[1];
                vertex_counter++;
                vertexIndices[vertex_counter] = (uint)faceVert[2];
                vertex_counter++;
            }

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
            if (keyboardState.IsKeyDown(Keys.Right)) {
                position.X += step;
            }

            if (keyboardState.IsKeyDown(Keys.Left)) {
                position.X -= step;
            }

            if (keyboardState.IsKeyDown(Keys.Up)) {
                position.Y += step;
            }

            if (keyboardState.IsKeyDown(Keys.Down)) {
                position.Y -= step;
            }

            angle += 90.0f * (float)time; // 180 deg per second
            RefreshMatrices();
        }

        public virtual void Draw(double time)
        {
            if (visible) {
                GL.Enable(EnableCap.DepthTest);

                GL.BindVertexArray(vertexArray);

                ShaderManager.simpleMeshShader.Use();
                ShaderManager.simpleMeshShader.SetMatrix4("model", modelMatrix);
                ShaderManager.simpleMeshShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                ShaderManager.simpleMeshShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());

                // GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / vertexLength);
                GL.DrawElements(PrimitiveType.Triangles, vertexIndices.Length, DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
