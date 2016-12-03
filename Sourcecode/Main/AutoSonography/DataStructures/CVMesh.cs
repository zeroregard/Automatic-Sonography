using Microsoft.Kinect.Fusion;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{

    public class Face
    {
        public int index1;
        public int index2;
        public int index3;
        public Vector3 normal;
        public bool Visited; //Used for flood filling algorithm
        public List<Face> Neighbors;

        public Face(int v1, int v2, int v3)
        {
            index1 = v1;
            index2 = v2;
            index3 = v3;
        }
    }
    public class CVMesh
    {
        public List<int> Colors = new List<int>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<int> TriangleIndeces = new List<int>();
        public List<Vector3> Vertices = new List<Vector3>();
        public List<Face> Faces = new List<Face>(); //TriangleIndices as faces instead
        [ExcludeFromCodeCoverage] //Cannot test this as it requires a Kinect ColorMesh which is dependent on the Kinect API: 
        //you can't construct this yourself
        public static CVMesh ConvertToMesh(ColorMesh mesh)
        {
            CVMesh m = new CVMesh();
            m.Colors.AddRange(mesh.GetColors());
            m.Normals.AddRange(mesh.GetNormals());
            m.TriangleIndeces.AddRange(mesh.GetTriangleIndexes());
            m.Vertices.AddRange(mesh.GetVertices());
            return m;
        }

        public static CVMesh Clone(CVMesh original)
        {
            CVMesh cloned = new CVMesh();
            cloned.Vertices = CloneVectors(original.Vertices);
            cloned.Normals = CloneVectors(original.Normals);
            cloned.Colors.AddRange(original.Colors); //TODO: Clone? Or does the list get closed over automatically in AddRange
            cloned.TriangleIndeces.AddRange(original.TriangleIndeces);
            return cloned;
        }

        public static List<Vector3> CloneVectors(List<Vector3> original)
        {
            List<Vector3> cloned = new List<Vector3>();
            foreach (var v in original)
            {
                Vector3 clonedVector = new Vector3();
                clonedVector.X = v.X;
                clonedVector.Y = v.Y;
                clonedVector.Z = v.Z;
                cloned.Add(clonedVector);
            }
            return cloned;
        }
    }
}
