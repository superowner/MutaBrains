using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using MutaBrains.Core.Managers;

namespace MutaBrains.Core.Objects
{
    class Terrain3D
    {
        protected float[] vertices;
        protected uint[] indices;
        protected Image<Rgba32> heightMap;

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

        public Terrain3D()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            heightMap = Image.Load<Rgba32>("assets/textures/terrains/Level1HM.png");

            parseHeightMap();

            vertexLength = 5;

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

            int positionLocation = ShaderManager.meshShader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            int texCoordLocation = ShaderManager.meshShader.GetAttribLocation("aTexture");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, vertexLength * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected virtual void parseHeightMap()
        {
            int map_w = heightMap.Width;
            int map_h = heightMap.Height;

            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();

            for (int z = 0; z < map_h; z++)
            {
                for (int x = 0; x < map_w; x++)
                {
                    float y = heightMap[x, z].R / 127.5f - 1.0f;

                    //vert.position = new Vector3(x, y, z);
                    //vert.texture = new Vector2(x, z);

                    vertices.AddRange(new float[] { x, y, z, x, z });

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
    }
}
