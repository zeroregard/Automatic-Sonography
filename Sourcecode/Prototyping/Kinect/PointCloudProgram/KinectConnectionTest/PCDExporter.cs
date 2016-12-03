using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Kinect.Fusion;
using System.Collections.ObjectModel;
using System.Globalization;

namespace KinectConnectionTest
{
    public class PCDExporter
    {

        public static void Output(ColorMesh mesh)
        {
            //string outputLocation = Directory.GetCurrentDirectory();
            string fileName = string.Format("pointcloud-{0:yyyy-MM-dd_hh-mm-ss-tt}.ply", DateTime.Now);
            string filePath = /*outputLocation + */fileName;
            var file = File.Create(filePath);
            file.Close();
            WritePointsToFile(filePath, mesh);
        }

        private static void WritePointsToFile(string filePath, ColorMesh mesh)
        {
            var indeces = mesh.GetTriangleIndexes();
            var vertices = mesh.GetVertices();
            var colors = mesh.GetColors();
            int faces = indeces.Count / 3;
            string header = GetHeader(vertices.Count, faces);
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine(header); //header 
                for (int i = 0; i < vertices.Count; i++) //vertices + colors
                {
                    file.WriteLine(GetPointString(vertices[i], colors[i]));
                }
                for (int i = 0; i < faces; i++)
                {
                    string baseIndex0 = (i * 3).ToString(CultureInfo.InvariantCulture);
                    string baseIndex1 = ((i * 3) + 1).ToString(CultureInfo.InvariantCulture);
                    string baseIndex2 = ((i * 3) + 2).ToString(CultureInfo.InvariantCulture);
                    string faceString = "3 " + baseIndex0 + " " + baseIndex1 + " " + baseIndex2;
                    file.WriteLine(faceString);
                }
            }
        }

        private static string GetPointString(Vector3 point, int color)
        {
            decimal x = (decimal)(point.X);
            decimal y = (decimal)(point.Y);
            decimal z = (decimal)(point.Z);
            string xString = x.ToString(new System.Globalization.CultureInfo("en-US"));
            string yString = y.ToString(new System.Globalization.CultureInfo("en-US"));
            string zString = z.ToString(new System.Globalization.CultureInfo("en-US"));
            int red = (color >> 16) & 255;
            int green = (color >> 8) & 255;
            int blue = color & 255;
            string redString = red.ToString(CultureInfo.InvariantCulture);
            string greenString = green.ToString(CultureInfo.InvariantCulture);
            string blueString = blue.ToString(CultureInfo.InvariantCulture);
            return xString + " " + yString + " " + zString + " " + redString + " " + greenString + " " + blueString;
        }

        private static string GetHeader(int points, int faces)
        {
            string nl = Environment.NewLine;
            return
            "ply" + nl +
            "format ascii 1.0" + nl +
            "element vertex " + points + nl +
            "property float x" + nl +
            "property float y" + nl +
            "property float z" + nl +
            "property uchar red" + nl +
            "property uchar green" + nl +
            "property uchar blue" + nl +
            "element face " + faces.ToString(CultureInfo.InvariantCulture) + nl +
            "property list uchar int vertex_index" + nl + 
            "end_header";
        }
        
    }
}
