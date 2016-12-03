using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Kinect.Fusion;
using System.Globalization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DataStructures;
namespace DataStructures
{
    [ExcludeFromCodeCoverage]
    public class PLYHandler
    {
        public static List<Vector3> ReadMeshVertices(string location)
        {
            List<Vector3> vertices = new List<Vector3>();
            int vertexStartIndex = 10; //the lines before these is the header
            using (StreamReader sr = File.OpenText(location))
            {
                string s = string.Empty;
                int index = 0;
                int vertexIndex = 0;
                int vertexCount = 0;
                while ((s = sr.ReadLine()) != null)
                {
                    if (index == 3)
                    {
                        vertexCount = FetchIndexCount(s);
                    }
                    if (index >= vertexStartIndex)
                    {
                        if (vertexIndex == vertexCount)
                        {
                            break;
                        }
                        string[] xyz = s.Split(' ');
                        decimal dx, dy, dz;
                        decimal.TryParse(xyz[0], NumberStyles.Float, CultureInfo.InvariantCulture, out dx);
                        decimal.TryParse(xyz[1], NumberStyles.Float, CultureInfo.InvariantCulture, out dy);
                        decimal.TryParse(xyz[2], NumberStyles.Float, CultureInfo.InvariantCulture, out dz);
                        float x = (float)dx;
                        float y = (float)dy;
                        float z = (float)dz;
                        Vector3 vector = new Vector3() { X = x, Y = y, Z = z };
                        vertices.Add(vector);
                        vertexIndex++;
                    }
                    index++;
                }
            }
            return vertices;
        }

        public static CVMesh ReadMesh(string location)
        {
            CVMesh mesh = new CVMesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            int vertexStartIndex = 10; //the lines before these is the header
            using (StreamReader sr = File.OpenText(location))
            {
                string s = string.Empty;
                int index = 0;
                int vertexIndex = 0;
                int vertexCount = 0;
                bool headerDone = false;
                while ((s = sr.ReadLine()) != null)
                {
                    if (index == 3)
                        vertexCount = FetchIndexCount(s);
                    if (headerDone)
                    {
                        if (vertexIndex < vertexCount)
                        {
                            string[] xyz = s.Split(' ');
                            decimal dx, dy, dz;
                            decimal.TryParse(xyz[0], NumberStyles.Float, CultureInfo.InvariantCulture, out dx);
                            decimal.TryParse(xyz[1], NumberStyles.Float, CultureInfo.InvariantCulture, out dy);
                            decimal.TryParse(xyz[2], NumberStyles.Float, CultureInfo.InvariantCulture, out dz);
                            float x = (float)dx;
                            float y = (float)dy;
                            float z = (float)dz;
                            Vector3 vector = new Vector3() { X = x, Y = y, Z = z };
                            vertices.Add(vector);
                            vertexIndex++;
                        }
                        else
                        {
                            string[] threeABC = s.Split(' ');
                            int a = int.Parse(threeABC[1]);
                            int b = int.Parse(threeABC[2]);
                            int c = int.Parse(threeABC[3]);
                            triangles.Add(a);
                            triangles.Add(b);
                            triangles.Add(c);
                        }
                    }
                    if (s.Equals("end_header"))
                        headerDone = true;
                    index++;
                }
            }
            mesh.Vertices = vertices;
            mesh.TriangleIndeces = triangles;
            return mesh;
        }

        public static int FetchIndexCount(string vertex)
        {
            string[] s = vertex.Split(' ');
            int amount = int.Parse(s[2]);
            return amount;
        }

        public static void Output(List<Vector3> vertices, string location, string name = "points")
        {
            string fileName = name;
            fileName += " " + string.Format("pointcloud_mesh-{0:yyyy-MM-dd_hh-mm-ss-tt}.ply", DateTime.Now);
            location = location + "/" + fileName;
            var file = File.Create(location);
            file.Close();
            try
            {
                WritePointsToFile(location, vertices);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in PLYExporter: " + ex.Message);
            }
        }

        public static void Output(List<URPose> poses, string location, string name = "poses")
        {
            string fileName = name;
            fileName += " " + string.Format("pointcloud_mesh-{0:yyyy-MM-dd_hh-mm-ss-tt}.ply", DateTime.Now);
            location = location + "/" + fileName;
            var file = File.Create(location);
            file.Close();
            try
            {
                WritePointsToFile(location, poses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in PLYExporter: " + ex.Message);
            }
        }

        public static bool Output(CVMesh mesh, ref string location, bool exportColors = true)
        {
            string fileName = string.Format("pointcloud_mesh-{0:yyyy-MM-dd_hh-mm-ss-tt}.ply", DateTime.Now);
            location = location + "/" + fileName;
            var file = File.Create(location);
            file.Close();
            try
            {
                WritePointsToFile(location, mesh, exportColors);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in PLYExporter: " + ex.Message);
                return false;
            }
        }

        private static void WritePointsToFile(string filePath, CVMesh mesh, bool exportColors)
        {
            var indeces = mesh.TriangleIndeces;
            var vertices = mesh.Vertices;
            var colors = mesh.Colors;
            string header = GetHeader(vertices.Count, indeces.Count/3, exportColors);
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine(header); //header 
                for (int i = 0; i < vertices.Count; i++) //vertices + colors
                {
                    string line = GetPointString(vertices[i]);
                    if (exportColors)
                        line += " " + GetColorString(colors[i]);
                    file.WriteLine(line);
                }
                for (int i = 0; i < indeces.Count; i=i+3)
                {
                    string baseIndex0 = indeces[i].ToString(CultureInfo.InvariantCulture);
                    string baseIndex1 = indeces[i + 1].ToString(CultureInfo.InvariantCulture);
                    string baseIndex2 = indeces[i + 2].ToString(CultureInfo.InvariantCulture);
                    string faceString = "3 " + baseIndex0 + " " + baseIndex1 + " " + baseIndex2;
                    file.WriteLine(faceString);
                }
            }
        }

        private static void WritePointsToFile(string filePath, List<Vector3> vertices)
        {
            string header = GetHeader(vertices.Count);
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine(header); //header 
                for (int i = 0; i < vertices.Count; i++) //vertices + colors
                {
                    file.WriteLine(GetPointString(vertices[i]));
                }
            }
        }

        private static void WritePointsToFile(string filePath, List<URPose> poses)
        {
            string header = GetPosesHeader(poses.Count);
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine(header);
                for (int i = 0; i < poses.Count; i++)
                {
                    file.WriteLine(GetPoseString(poses[i]));
                }
            }
        }


        private static string GetColorString(int color)
        {
            int red = (color >> 16) & 255;
            int green = (color >> 8) & 255;
            int blue = color & 255;
            string redString = red.ToString(CultureInfo.InvariantCulture);
            string greenString = green.ToString(CultureInfo.InvariantCulture);
            string blueString = blue.ToString(CultureInfo.InvariantCulture);
            return redString + " " + greenString + " " + blueString;
        }

        private static string GetPointString(Vector3 point)
        {
            decimal x = (decimal)(point.X);
            decimal y = (decimal)(point.Y);
            decimal z = (decimal)(point.Z);
            string xString = x.ToString(new CultureInfo("en-US"));
            string yString = y.ToString(new CultureInfo("en-US"));
            string zString = z.ToString(new CultureInfo("en-US"));
            return xString + " " + yString + " " + zString;
        }

        private static string GetPoseString(URPose pose)
        {
            decimal x = (decimal)(pose.Xpose);
            decimal y = (decimal)(pose.Ypose);
            decimal z = (decimal)(pose.Zpose);
            decimal rx = (decimal)(pose.RXpose);
            decimal ry = (decimal)(pose.RYpose);
            decimal rz = (decimal)(pose.RZpose);
            string xString = x.ToString(new CultureInfo("en-US"));
            string yString = y.ToString(new CultureInfo("en-US"));
            string zString = z.ToString(new CultureInfo("en-US"));
            string rxString = rx.ToString(new CultureInfo("en-US"));
            string ryString = ry.ToString(new CultureInfo("en-US"));
            string rzString = rz.ToString(new CultureInfo("en-US"));
            return xString + " " + yString + " " + zString + rxString + " " + ryString + " " + rzString;
        }

        private static string GetPosesHeader(int points)
        {
            string nl = Environment.NewLine;
            return
            "ply" + nl +
            "format ascii 1.0" + nl +
            "element vertex " + points + nl +
            "property float x" + nl +
            "property float y" + nl +
            "property float z" + nl +
            "property float nx" + nl +
            "property float ny" + nl +
            "property float nz" + nl +
            "end_header";
        }

        private static string GetHeader(int points, int faces, bool exportColors)
        {
            string nl = Environment.NewLine;
            string colors = "";
            if (exportColors)
            {
                colors +=
            "property uchar red" + nl +
            "property uchar green" + nl +
            "property uchar blue" + nl;
            }
            return
            "ply" + nl +
            "format ascii 1.0" + nl +
            "element vertex " + points + nl +
            "property float x" + nl +
            "property float y" + nl +
            "property float z" + nl +
            colors +
            "element face " + faces.ToString(CultureInfo.InvariantCulture) + nl +
            "property list uchar int vertex_indices" + nl +
            "end_header";
        }

        private static string GetHeader(int points)
        {
            string nl = Environment.NewLine;
            return
            "ply" + nl +
            "format ascii 1.0" + nl +
            "element vertex " + points + nl +
            "property float x" + nl +
            "property float y" + nl +
            "property float z" + nl +
            "end_header";
        }
    }
}
