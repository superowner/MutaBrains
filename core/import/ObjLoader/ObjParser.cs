using System.Collections.Generic;
using System.Globalization;
using OpenTK.Mathematics;

namespace MutaBrains.Core.Import.ObjLoader
{
    class OBJParser
    {
        public static bool TryParseVertice(string str, out Vector3 vertice)
        {
            vertice = Vector3.Zero;
            if (str[0] == 'v' && str[1] == ' ')
            {
                var tokens = str.Split(' ');
                vertice.X = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                vertice.Y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                vertice.Z = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                return true;
            }
            return false;
        }
        public static bool TryParseFace(string str,
            out List<int> vertsIndxs, out List<int> uvsIndxs, out List<int> normsIndxs)
        {
            vertsIndxs = null;
            uvsIndxs = null;
            normsIndxs = null;
            if (str[0] == 'f')
            {
                vertsIndxs = new List<int>();
                uvsIndxs = new List<int>();
                normsIndxs = new List<int>();
                string[] facesInfo = str.Substring(2).Split(' ');
                foreach (string info in facesInfo)
                {
                    string[] tokens = info.Split('/');
                    // int tokensCount = 0;
                    // foreach (string token in tokens)
                    // {
                    //     if (token != string.Empty) {
                    //         tokensCount++;
                    //     }
                    // }
                    if (tokens.Length == 0)
                    {
                        vertsIndxs.Add(int.Parse(info));
                    }
                    else if (tokens.Length == 2)
                    {
                        vertsIndxs.Add(int.Parse(tokens[0]));
                        uvsIndxs.Add(int.Parse(tokens[1]));
                    }
                    else if (tokens.Length == 3)
                    {
                        int uv;
                        if (int.TryParse(tokens[1], out uv))
                        {
                            uvsIndxs.Add(uv);
                        }
                        vertsIndxs.Add(int.Parse(tokens[0]));
                        normsIndxs.Add(int.Parse(tokens[2]));
                    }
                }
                return true;
            }
            return false;
        }
        public static bool TryParseNormal(string str, out Vector3 normal)
        {
            normal = Vector3.Zero;
            if (str[0] == 'v' && str[1] == 'n')
            {
                var tokens = str.Split(' ');
                normal.X = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                normal.Y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                normal.Z = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                return true;
            }
            return false;
        }
        public static bool TryParseUVw(string str, out Vector3 uvw)
        {
            uvw = Vector3.Zero;
            if (str[0] == 'v' && str[1] == 't')
            {
                var tokens = str.Split(' ');
                uvw.X = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                uvw.Y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                if (tokens.Length == 4)
                {
                    uvw.Z = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                }
                return true;
            }
            return false;
        }
    }
}
