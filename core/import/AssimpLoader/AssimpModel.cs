using System.Collections.Generic;
using Assimp;

namespace MutaBrains.Core.Import.AssimpLoader
{
    class AssimpModel
    {
        public static float[] vertices;

        public static void Test()
        {
            var importer = new AssimpContext();
            Scene scene = importer.ImportFile(Config.Navigator.MeshPath("brain-simple-mesh"),
                PostProcessSteps.CalculateTangentSpace |
                PostProcessSteps.GenerateUVCoords |
                PostProcessSteps.Triangulate |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.SortByPrimitiveType);

            List<float> vertList = new List<float>();

            foreach (Assimp.Mesh mesh in scene.Meshes)
            {
                // var vertices = mesh.Vertices;
                // var normals = mesh.Normals;

                foreach (Face face in mesh.Faces)
                {
                    int vert_index_1 = face.Indices[0];
                    int vert_index_2 = face.Indices[1];
                    int vert_index_3 = face.Indices[2];

                    float first_1 = mesh.Vertices[vert_index_1].X;
                    float second_1 = mesh.Vertices[vert_index_1].Y;
                    float third_1 = mesh.Vertices[vert_index_1].Z;

                    float norm_first_1 = mesh.Normals[vert_index_1].X;
                    float norm_second_1 = mesh.Normals[vert_index_1].Y;
                    float norm_third_1 = mesh.Normals[vert_index_1].Z;

                    float first_2 = mesh.Vertices[vert_index_2].X;
                    float second_2 = mesh.Vertices[vert_index_2].Y;
                    float third_2 = mesh.Vertices[vert_index_2].Z;

                    float norm_first_2 = mesh.Vertices[vert_index_2].X;
                    float norm_second_2 = mesh.Vertices[vert_index_2].Y;
                    float norm_third_2 = mesh.Vertices[vert_index_2].Z;

                    float first_3 = mesh.Vertices[vert_index_3].X;
                    float second_3 = mesh.Vertices[vert_index_3].Y;
                    float third_3 = mesh.Vertices[vert_index_3].Z;

                    float norm_first_3 = mesh.Vertices[vert_index_3].X;
                    float norm_second_3 = mesh.Vertices[vert_index_3].Y;
                    float norm_third_3 = mesh.Vertices[vert_index_3].Z;

                    float[] array = new float[] {
                        first_1, second_1, third_1, norm_first_1, norm_second_1, norm_third_1, 0, 0,
                        first_2, second_2, third_2, norm_first_2, norm_second_2, norm_third_2, 0, 0,
                        first_3, second_3, third_3, norm_first_3, norm_second_3, norm_third_3, 0, 0
                    };

                    vertList.AddRange(array);
                }
            }

            vertices = vertList.ToArray();
        }
    }
}