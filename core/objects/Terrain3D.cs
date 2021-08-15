using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using MutaBrains.Core.Managers;
using MutaBrains.Core.Textures;

namespace MutaBrains.Core.Objects
{
    class Terrain3D : Object3D
    {
        protected Texture heightMap, ground, grass;

        public Terrain3D(string name, string path, Vector3 position, Vector3 scale) : base(name, path, position, scale)
        {
        }

        protected override void Initialize(string name, string path, Vector3 position, Vector3 scale)
        {
            heightMap = Texture.LoadTexture("assets/textures/terrains/height16bit3.png");
            ground = Texture.LoadTexture("assets/textures/terrains/Level1T1.jpg");
            grass = Texture.LoadTexture("assets/textures/terrains/Level1T2.jpg");

            this.name = name;
            this.path = path;
            this.position = position;
            this.scale = scale;

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

        protected override void ProcessMeshes()
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
                    y *= 50;

                    float u = (float)x / 1.0f;
                    float v = (float)z / 1.0f;

                    vertices.AddRange(new float[] { x, y, z, 0, 1, 0, u, v });

                    if (z < map_h - 2 && x < map_w - 2)
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

            for (int z = 0; z < map_h; z++)
            {
                for (int x = 0; x < map_w; x++)
                {
                    int p, pL, pR, pT, pB;
                    float nx, ny, nz;

                    p = z * map_w + x;

                    pL = (x == 0) ? p : p - 1;
                    pR = (p + 1 >= map_w * map_h) ? p : p + 1;
                    pB = (p + map_w >= map_w * map_h) ? p : p + map_w;
                    pT = (z == 0) ? p : p - map_w;

                    int pL_vertex_height_index = pL * vertexLength + 1;
                    int pR_vertex_height_index = pR * vertexLength + 1;
                    int pB_vertex_height_index = pB * vertexLength + 1;
                    int pT_vertex_height_index = pT * vertexLength + 1;

                    nx = vertices[pL_vertex_height_index] - vertices[pR_vertex_height_index];
                    ny = vertices[pB_vertex_height_index] - vertices[pT_vertex_height_index];
                    nz = 2;

                    Vector3 normal = new Vector3(nx, ny, nz).Normalized();
                    int vertex_normal_x_index = p * vertexLength + 3;
                    int vertex_normal_y_index = p * vertexLength + 4;
                    int vertex_normal_z_index = p * vertexLength + 5;
                    vertices[vertex_normal_x_index] = normal.X;
                    vertices[vertex_normal_y_index] = normal.Y;
                    vertices[vertex_normal_z_index] = normal.Z;
                }
            }

            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();
        }

        public override void Draw(double time)
        {
            if (visible)
            {
                //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

                GL.Enable(EnableCap.DepthTest);
                GL.FrontFace(FrontFaceDirection.Cw);

                GL.BindVertexArray(vertexArray);

                ground.Use(TextureUnit.Texture0);
                grass.Use(TextureUnit.Texture1);
                ground.Use(TextureUnit.Texture3);

                ShaderManager.heightMapShader.Use();
                ShaderManager.heightMapShader.SetMatrix4("model", modelMatrix);
                ShaderManager.heightMapShader.SetMatrix4("view", CameraManager.Perspective.GetViewMatrix());
                ShaderManager.heightMapShader.SetMatrix4("projection", CameraManager.Perspective.GetProjectionMatrix());
                ShaderManager.heightMapShader.SetVector3("viewPosition", CameraManager.Perspective.Position);
                // Material
                ShaderManager.heightMapShader.SetInt("material.diffuse", 0);
                ShaderManager.heightMapShader.SetInt("material.diffuse2", 1);
                ShaderManager.heightMapShader.SetInt("material.specular", 3);
                ShaderManager.heightMapShader.SetFloat("material.shininess", 1.0f);
                // Directional light
                ShaderManager.heightMapShader.SetVector3("dirLight.direction", new Vector3(-.1f));
                ShaderManager.heightMapShader.SetVector3("dirLight.ambient", new Vector3(0.1f));
                ShaderManager.heightMapShader.SetVector3("dirLight.diffuse", new Vector3(1.0f));
                ShaderManager.heightMapShader.SetVector3("dirLight.specular", new Vector3(1.0f));

                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                GL.FrontFace(FrontFaceDirection.Cw);

                //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }
    }
}
