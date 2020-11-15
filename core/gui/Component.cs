using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using MutaBrains.Core.Shaders;
using MutaBrains.Core.Textures;
using MutaBrains.Core.Output;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

    public enum DragType
    {
        None,
        Full,
        Area
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
        protected DragType dragType = DragType.None;
        protected Box2 dragArea = new Box2();

        protected Matrix4 rotationMatrix;
        protected Matrix4 scaleMatrix;
        protected Matrix4 translationMatrix;
        protected Matrix4 modelMatrix;

        protected Component parent = null;
        protected List<Component> childs = new List<Component>();
        protected bool visible = true;
        protected bool collisionCheckEnabled = false;
        protected double collisionUpdateTime = 0.01;
        protected Box2 boundingBox;

        public delegate void MBMouseEvent(object sender, MouseButtonEventArgs args);
        public event MBMouseEvent OnMouseHover;
        public event MBMouseEvent OnMouseLeave;
        public event MBMouseEvent OnMouseDown;
        public event MBMouseEvent OnMouseUp;
        public event MBMouseEvent OnMouseClick;

        private double collisionTime = 0;
        private bool isHovered = false;
        private bool isMouseDown = false;
        private double clickTimer = 0;
        private bool isDragged = false;
        private Vector2 dragOffset = Vector2.Zero;

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
            float l_x = 0.0f;
            float r_x = 0.0f;
            float t_y = 0.0f;
            float b_y = 0.0f;

            switch (origin)
            {
                case ComponentOrigin.TopLeft:
                    l_x = 0.0f;
                    r_x = size.X;
                    t_y = 0.0f;
                    b_y = size.Y;
                    break;
                case ComponentOrigin.TopRight:
                    l_x = -size.X;
                    r_x = 0.0f;
                    t_y = 0.0f;
                    b_y = size.Y;
                    break;
                case ComponentOrigin.Center:
                    l_x = -size.X / 2.0f;
                    r_x = size.X / 2.0f;
                    t_y = -size.Y / 2.0f;
                    b_y = size.Y / 2.0f;
                    break;
                case ComponentOrigin.BottomLeft:
                    l_x = 0.0f;
                    r_x = size.X;
                    t_y = -size.Y;
                    b_y = 0.0f;
                    break;
                case ComponentOrigin.BottomRight:
                    l_x = -size.X;
                    r_x = 0.0f;
                    t_y = -size.Y;
                    b_y = 0.0f;
                    break;
            }

            vertices = new float[] {
                // Position             Normals                 Textures
                l_x,    t_y,    0.0f,   0.0f,   0.0f,   -1.0f,  0.0f,   1.0f,   // top left
                r_x,    t_y,    0.0f,   0.0f,   0.0f,   -1.0f,  1.0f,   1.0f,   // top right
                r_x,    b_y,    0.0f,   0.0f,   0.0f,   -1.0f,  1.0f,   0.0f,   // bottom right
                
                l_x,    t_y,    0.0f,   0.0f,   0.0f,   -1.0f,  0.0f,   1.0f,   // top left
                r_x,    b_y,    0.0f,   0.0f,   0.0f,   -1.0f,  1.0f,   0.0f,   // bottom right
                l_x,    b_y,    0.0f,   0.0f,   0.0f,   -1.0f,  0.0f,   0.0f,   // bottom left
            };

            rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(angle));
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            boundingBox = new Box2(l_x, t_y, r_x, b_y);
            boundingBox.Translate(position.Xy);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        protected virtual void RefreshVertexBuffer(bool vertexInit = true)
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

        public virtual void Update(double time, Vector2 mousePosition, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            if (collisionCheckEnabled)
            {
                collisionTime += time;
                if (clickTimer < 1) { clickTimer += time; }

                if (isDragged)
                {
                    changePosition(new Vector3(mousePosition + dragOffset));
                }

                if (collisionTime >= collisionUpdateTime)
                {
                    MouseButtonEventArgs MBEA = new MouseButtonEventArgs();

                    if (boundingBox.Contains(mousePosition))
                    {
                        if (!isHovered)
                        {
                            OnMouseHover?.Invoke(this, MBEA);
                            isHovered = true;
                        }

                        if (mouseState.IsAnyButtonDown)
                        {
                            if (!isMouseDown)
                            {
                                OnMouseDown?.Invoke(this, MBEA);
                                isMouseDown = true;
                                clickTimer = 0;

                                if ((dragType == DragType.Area && dragArea.Contains(mousePosition)) || dragType == DragType.Full)
                                {
                                    isDragged = true;
                                    dragOffset = position.Xy - mousePosition;
                                }
                            }
                        }
                        else
                        {
                            if (isMouseDown)
                            {
                                OnMouseUp?.Invoke(this, MBEA);
                                isMouseDown = false;
                                if (isDragged)
                                {
                                    isDragged = false;
                                }
                                if (clickTimer <= 0.9)
                                {
                                    OnMouseClick?.Invoke(this, MBEA);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (isHovered)
                        {
                            OnMouseLeave?.Invoke(this, MBEA);
                            isHovered = false;
                        }
                    }

                    collisionTime = 0;
                }
            }

            UpdateChilds(time, mousePosition, mouseState, keyboardState);
        }

        protected virtual void UpdateChilds(double time, Vector2 mousePosition, MouseState mouseState = null, KeyboardState keyboardState = null)
        {
            foreach (Component child in childs)
            {
                child.Update(time, mousePosition, mouseState, keyboardState);
            }
        }

        public virtual void Draw(double time)
        {
            if (visible)
            {
                GL.Disable(EnableCap.DepthTest);

                GL.BindVertexArray(vertexArray);
                texture.Use(TextureUnit.Texture0);
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

        protected virtual void changePosition(Vector3 newPosition)
        {
            Vector3 offset = new Vector3(newPosition - position);
            position = newPosition;
            RefreshVertexBuffer(false);

            foreach (Component child in childs)
            {
                child.position += offset;
                child.RefreshVertexBuffer(false);
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
            Vector3 positionShift = Vector3.Zero;
            switch (origin)
            {
                case ComponentOrigin.TopLeft:
                    positionShift = position;
                    break;
                case ComponentOrigin.TopRight:
                    positionShift.X += position.X - size.X;
                    positionShift.Y += position.Y;
                    break;
                case ComponentOrigin.Center:
                    positionShift.X += position.X - size.X / 2.0f;
                    positionShift.Y += position.Y - size.Y / 2.0f;
                    break;
                case ComponentOrigin.BottomLeft:
                    positionShift.X += position.X;
                    positionShift.Y += position.Y - size.Y;
                    break;
                case ComponentOrigin.BottomRight:
                    positionShift.X += position.X - size.X;
                    positionShift.Y += position.Y - size.Y;
                    break;
            }
            child.position += positionShift;
            child.RefreshVertexBuffer();
            childs.Add(child);
        }

        public void removeChild(Component child)
        {
            child.parent = null;
            childs.Remove(child);
        }

        public Box2 getBoundingBox()
        {
            return boundingBox;
        }
    }
}