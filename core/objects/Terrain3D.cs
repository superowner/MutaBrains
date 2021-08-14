using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects
{
    class Terrain3D
    {
        protected float[] vertices;
        protected uint[] indices;

        protected int indexBuffer;
        protected int vertexBuffer;
        protected int vertexArray;
        protected int vertexLength;

        protected Vector3 position = new Vector3(-50, 0, -50);
        protected Vector3 scale = Vector3.One;
        protected Matrix4 rotationMatrix;
        protected Matrix4 scaleMatrix;
        protected Matrix4 translationMatrix;
        protected Matrix4 modelMatrix;

        protected Texture heightMap;

        public Terrain3D()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            heightMap = Texture.LoadTexture("assets/textures/terrains/Level1HM.png");

            parseHeightMap();

            vertexLength = 8;

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

            int positionLocation = ShaderManager.heightMapShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            int normalLocation = ShaderManager.heightMapShader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(normalLocation);

            int texCoordLocation = ShaderManager.heightMapShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected virtual void parseHeightMap()
        {
            int map_w = (int)heightMap.Size.X;
            int map_h = (int)heightMap.Size.Y;

            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();

            for (int z = 0; z < map_h; z++)
            {
                for (int x = 0; x < map_w; x++)
                {
                    float y = heightMap.Pixels[x, z].R / 127.5f - 1.0f;
                    y *= 4;

                    //vert.position = new Vector3(x, y, z);
                    //vert.normal = new Vector3(0, 1, 0);
                    //vert.texture = new Vector2(x, z);

                    vertices.AddRange(new float[] { x, y, z, 0, 1, 0, x, z });

                    if (z < map_h -2 && x < map_w - 2)
                    {
                        uint a, b, c, d;

                        a = (uint)(z * map_w + x);
                        b = a + 1;
                        c = a + (uint)map_w;
                        d = a + (uint)map_w + 1;

                        indices.AddRange(new uint[] { a, b, c, c, b, d });
                    }
                }
            }

            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();
        }

        protected virtual void RefreshMatrices()
        {
            rotationMatrix = Matrix4.CreateRotationX(0);
            scaleMatrix = Matrix4.CreateScale(scale);
            translationMatrix = Matrix4.CreateTranslation(position);

            modelMatrix = rotationMatrix * scaleMatrix * translationMatrix;
        }

        public virtual void Update()
        {
            RefreshMatrices();
        }

        public virtual void Draw(double time)
        {
            if (true)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Cw);

                GL.BindVertexArray(vertexArray);

                heightMap.Use(TextureUnit.Texture0);
                heightMap.Use(TextureUnit.Texture1);

                ShaderManager.heightMapShader.Use();
                ShaderManager.heightMapShader.SetMatrix4("model", modelMatrix);
                ShaderManager.heightMapShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                ShaderManager.heightMapShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());
                ShaderManager.heightMapShader.SetVector3("viewPosition", CameraManager.Perspective.Position);
                // Material
                ShaderManager.heightMapShader.SetInt("material.diffuse", 0);
                ShaderManager.heightMapShader.SetInt("material.specular", 1);
                ShaderManager.heightMapShader.SetFloat("material.shininess", 1.0f);
                // Directional light
                ShaderManager.heightMapShader.SetVector3("dirLight.direction", new Vector3(-.1f));
                ShaderManager.heightMapShader.SetVector3("dirLight.ambient", new Vector3(0.1f));
                ShaderManager.heightMapShader.SetVector3("dirLight.diffuse", new Vector3(1.0f));
                ShaderManager.heightMapShader.SetVector3("dirLight.specular", new Vector3(1.0f));

                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                GL.FrontFace(FrontFaceDirection.Cw);

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }
    }
}
