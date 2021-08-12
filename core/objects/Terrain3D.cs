using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MutaBrains.Core.Objects
{
    class Terrain3D
    {
        protected Image<Rgba32> heightMap;

        public Terrain3D()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            heightMap = Image.Load<Rgba32>("assets/textures/terrains/Level1HM.png");

            parseHeightMap();
        }

        protected virtual void parseHeightMap()
        {

        }
    }
}

// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.DirectX;
// using Microsoft.DirectX.Direct3D;
// using System.Drawing;
// namespace Benihana
// {
//     class Ground
//     {
//         public Matrix RotationMatrix = Matrix.Identity;
//         /// <summary>
//         /// Вектор смещения
//         /// </summary>
//         public Vector3 OffsetVector;
//         private Device device;
//         private List<CustomVertex.PositionNormalTextured> VertsList;
//         private CustomVertex.PositionNormalTextured[] Verts;
//         /// <summary>
//         /// Количество треугольников
//         /// </summary>
//         private int PrimitivesCount;
//         public Bitmap HeightMap, TextureMap;
//         private List<Vector3> PointsList;
//         private Texture MainTexture;
//         private Camera camera;
//         public Ground(Device device)
//         {
//             this.device = device;
//             HeightMap = new Bitmap(@"Landscapes\Level1\Level1HM.png");
//             TextureMap = new Bitmap(@"Landscapes\Level1\Level1T1.jpg");
//             MainTexture = new Texture(device, TextureMap, Usage.Dynamic | Usage.AutoGenerateMipMap, Pool.Default);
//             PointsList = new List<Vector3>();
//             Load();
//         }
//         public void ResetDevice(Device device)
//         {
//             this.device = device;
//             MainTexture = new Texture(device, TextureMap, Usage.Dynamic | Usage.AutoGenerateMipMap, Pool.Default);
//         }
//         public void Load()
//         {
//             VertsList = new List<CustomVertex.PositionNormalTextured>();
//             for (int z = 0; z < HeightMap.Height; z++)
//             {
//                 for (int x = 0; x < HeightMap.Width; x++)
//                 {
//                     float y = HeightMap.GetPixel(x, z).R / 255.0f * 10;
//                     PointsList.Add(new Vector3(x, y, z));
//                 }
//             }

//             Vector3 norm;
//             int pL, pR, pB, pT;
//             for (int z = 0; z < HeightMap.Height - 1; z++)
//             {
//                 for (int x = 0; x < HeightMap.Width - 1; x++)
//                 {
//                     int p1 = x + HeightMap.Width * z;
//                     float y1 = PointsList[p1].Y;
//                     if (x == 0) { pL = p1; } else { pL = p1 - 1; } pR = p1 + 1; pB = p1 + HeightMap.Width; if (z == 0) { pT = p1; } else { pT = p1 - HeightMap.Width; }
//                     float n1x = (PointsList[pL].Y - PointsList[pR].Y), n1y = (PointsList[pB].Y - PointsList[pT].Y), n1z = 2;
//                     norm = new Vector3(n1x, n1y, n1z); norm.Normalize(); n1x = norm.X; n1y = norm.Y; n1z = norm.Z;
//                     float t1u = 0, t1v = 1;
//                     VertsList.Add(new CustomVertex.PositionNormalTextured(x, y1, z, n1x, n1y, n1z, t1u, t1v));

//                     int p2 = (x + HeightMap.Width * z) + HeightMap.Width;
//                     float y2 = PointsList[p2].Y;
//                     if (x == 0) { pL = p2; } else { pL = p2 - 1; } pR = p2 + 1; pB = p2 + HeightMap.Width; pT = p2 - HeightMap.Width;
//                     float n2x, n2y, n2z;
//                     n2x = (PointsList[pL].Y - PointsList[pR].Y); if (z < HeightMap.Height - 2) { n2y = (PointsList[pB].Y - PointsList[pT].Y); } else { n2y = 1; } n2z = 2;
//                     norm = new Vector3(n2x, n2y, n2z); norm.Normalize(); n2x = norm.X; n2y = norm.Y; n2z = norm.Z;
//                     float t2u = 0, t2v = 0;
//                     VertsList.Add(new CustomVertex.PositionNormalTextured(x, y2, z + 1, n2x, n2y, n2z, t2u, t2v));

//                     int p3 = (x + HeightMap.Width * z) + 1;
//                     float y3 = PointsList[p3].Y;
//                     pL = p3 - 1; pR = p3 + 1; pB = p3 + HeightMap.Width; if (z == 0) { pT = p3; } else { pT = p3 - HeightMap.Width; }
//                     float n3x = (PointsList[pL].Y - PointsList[pR].Y), n3y = (PointsList[pB].Y - PointsList[pT].Y), n3z = 2;
//                     norm = new Vector3(n3x, n3y, n3z); norm.Normalize(); n3x = norm.X; n3y = norm.Y; n3z = norm.Z;
//                     float t3u = 1, t3v = 1;
//                     VertsList.Add(new CustomVertex.PositionNormalTextured(x + 1, y3, z, n3x, n3y, n3z, t3u, t3v));
                    
//                     VertsList.Add(new CustomVertex.PositionNormalTextured(x, y2, z + 1, n2x, n2y, n2z, t2u, t2v));

//                     int p5 = ((x + HeightMap.Width * z) + HeightMap.Width) + 1;
//                     float y5 = PointsList[p5].Y;
//                     pL = p5 - 1; pR = p5 + 1; pB = p5 + HeightMap.Width; pT = p5 - HeightMap.Width;
//                     float n5x, n5y, n5z;
//                     if (x < HeightMap.Width - 2) { n5x = (PointsList[pL].Y - PointsList[pR].Y); } else { n5x = 1; } if (z < HeightMap.Height - 2) { n5y = (PointsList[pB].Y - PointsList[pT].Y); } else { n5y = 1; }; n5z = 2;
//                     norm = new Vector3(n5x, n5y, n5z); norm.Normalize(); n5x = norm.X; n5y = norm.Y; n5z = norm.Z;
//                     float t5u = 1, t5v = 0;
//                     VertsList.Add(new CustomVertex.PositionNormalTextured(x + 1, y5, z + 1, n5x, n5y, n5z, t5u, t5v));
                    
//                     VertsList.Add(new CustomVertex.PositionNormalTextured(x + 1, y3, z, n3x, n3y, n3z, t3u, t3v));
//                 }
//             }
//             PrimitivesCount = VertsList.Count() / 3;
//             Verts = VertsList.ToArray();
//         }
//         public void Update(Camera camera)
//         {
//             this.camera = camera;
//         }
//         public void Draw()
//         {
//             Material material = new Material();
//             material.Diffuse = Color.White;
//             material.Ambient = Color.Gray;
//             material.Specular = Color.White;
//             device.Material = material;

//             device.Transform.World = Matrix.Identity;
//             device.Transform.World = Matrix.Translation(OffsetVector) * RotationMatrix;
//             device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
//             device.SetTexture(0, MainTexture);
//             device.DrawUserPrimitives(PrimitiveType.TriangleList, PrimitivesCount, Verts);
//             device.SetTexture(0, null);
//             device.Transform.World = Matrix.Identity;
//         }
//     }
// }