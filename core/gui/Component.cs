using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using MutaBrains.Core.Shaders;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Output;

namespace MutaBrains.Core.GUI
{
    public enum ComponentOrigin
    {
        TopLeft,
        TopRight,
        Center,
        BottomLeft,
        BottomRight
    }

    public class Component
    {
        protected float[] vertices;
        protected int vertexBuffer;
        protected int vertexArray;
        protected int vertexLength;
        protected Texture texture;
        protected Vector2 size;
        protected Vector3 position;
        protected float angle = 0.0f;
        protected Vector3 scale = Vector3.One;
        protected ComponentOrigin origin = ComponentOrigin.Center;

        protected Matrix4 rotationMatrix;
        protected Matrix4 scaleMatrix;
        protected Matrix4 translationMatrix;
        protected Matrix4 modelMatrix;

        protected Component parent = null;
        protected List<Component> childs = new List<Component>();
        protected bool visible = true;

        public virtual void Initialize(Vector2 size, Vector3 startPosition)
        {
            this.size = size;

            position = startPosition;

            vertexLength = 8;

            InitializeVertices();

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            int vertexLocation = ShaderManager.guiShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(vertexLocation);

            int normalLocation = ShaderManager.guiShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(normalLocation);

            int texCoordLocation = ShaderManager.guiShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected virtual void InitializeVertices()
        {
            switch (origin)
            {
                case ComponentOrigin.TopLeft:
                    vertices = new float[] {
                        // Position             Normals             Texture coordinates
                        0.0f,   0.0f,   0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        size.X, 0.0f,   0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 1.0f, // top right
                        size.X, size.Y, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        
                        0.0f,   0.0f,   0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        size.X, size.Y, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        0.0f,   size.Y, 0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 0.0f, // bottom left
                    };
                    break;
                case ComponentOrigin.TopRight:
                    vertices = new float[] {
                        // Position                 Normals             Texture coordinates
                        -size.X,    0.0f,   0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        0.0f,       0.0f,   0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 1.0f, // top right
                        0.0f,       size.Y, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        
                        -size.X,    0.0f,   0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        0.0f,       size.Y, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        -size.X,    size.Y, 0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 0.0f, // bottom left
                    };
                    break;
                case ComponentOrigin.Center:
                    vertices = new float[] {
                        // Position                             Normals             Texture coordinates
                        -size.X / 2.0f, -size.Y / 2.0f, 0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        +size.X / 2.0f, -size.Y / 2.0f, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 1.0f, // top right
                        +size.X / 2.0f, +size.Y / 2.0f, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        
                        -size.X / 2.0f, -size.Y / 2.0f, 0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        +size.X / 2.0f, +size.Y / 2.0f, 0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        -size.X / 2.0f, +size.Y / 2.0f, 0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 0.0f, // bottom left
                    };
                    break;
                case ComponentOrigin.BottomLeft:
                    vertices = new float[] {
                        // Position                 Normals             Texture coordinates
                        0.0f,   -size.Y,    0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        size.X, -size.Y,    0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 1.0f, // top right
                        size.X, 0.0f,       0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        
                        0.0f,   -size.Y,    0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        size.X, 0.0f,       0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        0.0f,   0.0f,       0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 0.0f, // bottom left
                    };
                    break;
                case ComponentOrigin.BottomRight:
                    vertices = new float[] {
                        // Position             Normals             Texture coordinates
                        -size.X,    -size.Y,    0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        0.0f,       -size.Y,    0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 1.0f, // top right
                        0.0f,       0.0f,       0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        
                        -size.X,    -size.Y,    0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 1.0f, // top left
                        0.0f,       0.0f,       0.0f,   0.0f, 0.0f, -1.0f,  1.0f, 0.0f, // bottom right
                        -size.X,    0.0f,       0.0f,   0.0f, 0.0f, -1.0f,  0.0f, 0.0f, // bottom left
                    };
                    break;
            }

            rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(angle));
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        protected virtual void RefreshVertexBuffer()
        {
            InitializeVertices();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public virtual void WindowResize(Vector2 newSize)
        {
            RefreshVertexBuffer();
        }

        public virtual void Update(double time, Vector2 newPosition, bool updateInput = true)
        {

        }

        public virtual void Draw(double time)
        {
            if (visible)
            {
                GL.Disable(EnableCap.DepthTest);

                GL.BindVertexArray(vertexArray);
                texture.Use();
                ShaderManager.guiShader.Use();

                ShaderManager.guiShader.SetMatrix4("model", modelMatrix);
                ShaderManager.guiShader.SetMatrix4("view", CameraManager.Orthographic.GetViewMatrix());
                ShaderManager.guiShader.SetMatrix4("projection", CameraManager.Orthographic.GetProjectionMatrix());

                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / vertexLength);

                DrawChilds(time);
            }
        }

        protected virtual void DrawChilds(double time)
        {
            foreach (Component child in childs)
            {
                child.Draw(time);
            }
        }

        public virtual void Dispose()
        {
            GL.DeleteBuffer(vertexBuffer);
            GL.DeleteVertexArray(vertexArray);

            foreach (Component child in childs)
            {
                child.Dispose();
            }
        }

        public Vector2 getSize()
        {
            return size;
        }

        public void Show()
        {
            visible = true;
        }

        public void Hide()
        {
            visible = false;
        }

        public Component getParent()
        {
            return parent;
        }

        public List<Component> getChilds()
        {
            return childs;
        }

        public void addChild(Component child)
        {
            child.parent = this;
            switch (origin)
            {
                // учитывать ориджин другого при выщитывании смещения
                // создать класс для подсчета
                case ComponentOrigin.TopLeft:
                    child.position.X += position.X;
                    child.position.Y += position.Y;
                    break;
                case ComponentOrigin.TopRight:
                    child.position.X += position.X - size.X;
                    child.position.Y += position.Y;
                    break;
                case ComponentOrigin.Center:
                    child.position = new Vector3(child.position.X + position.X, child.position.Y + position.Y, 0);
                    break;
                case ComponentOrigin.BottomLeft:
                    child.position.X += position.X;
                    child.position.Y = position.Y - 10;
                    break;
                case ComponentOrigin.BottomRight:
                    child.position = new Vector3(child.position.X + position.X, child.position.Y + position.Y, 0);
                    break;
            }
            child.RefreshVertexBuffer();
            childs.Add(child);
        }

        public void removeChild(Component child)
        {
            child.parent = null;
            childs.Remove(child);
        }
    }
}