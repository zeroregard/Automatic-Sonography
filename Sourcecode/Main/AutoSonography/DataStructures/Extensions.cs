using Microsoft.Kinect.Fusion;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public static class Extensions
    {
        public static BoundingBox FindBoundingBox(List<Vector3> vertices, float y_minClamp = float.MaxValue, float y_maxClamp = float.MinValue)
        {
            float x_min = float.MaxValue;
            float x_max = float.MinValue;
            float y_min = float.MaxValue;
            float y_max = float.MinValue;
            float z_min = float.MaxValue;
            float z_max = float.MinValue;
            foreach (var v in vertices)
            {
                if (v.X < x_min)
                    x_min = v.X;
                if (v.X > x_max)
                    x_max = v.X;
                if (v.Y < y_min && v.Y > y_maxClamp)
                    y_min = v.Y;
                if (v.Y > y_max && v.Y < y_minClamp)
                    y_max = v.Y;
                if (v.Z < z_min)
                    z_min = v.Z;
                if (v.Z > z_max)
                    z_max = v.Z;
            }
            BoundingBox b = new BoundingBox() { x_min = x_min, x_max = x_max, y_min = y_min, y_max = y_max, z_min = z_min, z_max = z_max };
            return b;
        }

        public static Vector3 MidPoint(BoundingBox b)
        {
            float x = (b.x_max + b.x_min)/2;
            float y = (b.y_max + b.y_min)/2;
            float z = (b.z_max + b.z_min)/2;
            Vector3 v = new Vector3 {X = x, Y = y, Z = z};
            return v;
        }

        public static Matrix4 Dot(Matrix4 a, Matrix4 b)
        {
            Matrix4 c = new Matrix4();
            c.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            c.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            c.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            c.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M43 + a.M14 * b.M44;

            c.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            c.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            c.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            c.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M43 + a.M24 * b.M44;

            c.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            c.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            c.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            c.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M43 + a.M34 * b.M44;

            c.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            c.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            c.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            c.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M43 + a.M44 * b.M44;

            return c;
        }

        public static float ToRadians(float degrees)
        {
            return (float)(degrees * (Math.PI / 180));
        }

        public static float ToDegrees(float radians)
        {
            
        }

        public static List<Vector3> PrunePoints(List<Vector3> vertices)
        {
            HashSet<Vector3> prunedList = new HashSet<Vector3>();
            foreach (var v in vertices)
            {
                prunedList.Add(v);
            }
            return prunedList.ToList();
        }

        public static List<VertexIndex> PruneVertices(List<VertexIndex> vertices)
        {
            Dictionary<int, VertexIndex> uniques = new Dictionary<int, VertexIndex>();
            foreach (var vertexIndex in vertices)
            {
                if (!uniques.ContainsKey(vertexIndex.Index))
                {
                    uniques.Add(vertexIndex.Index, vertexIndex);
                }
            }
            return uniques.Values.ToList();
        }

        public static List<int> ToTriangleIndices(List<Face> faces)
        {
            List<int> indices = new List<int>();
            foreach (var face in faces)
            {
                indices.Add(face.index1);
                indices.Add(face.index2);
                indices.Add(face.index3);
            }
            return indices;
        }

        public static List<Face> ToFaces(List<int> triangles)
        {
            List<Face> faces = new List<Face>();
            for (int i = 0; i < triangles.Count; i=i+3)
            {
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];
                Face f = new Face(i1,i2,i3);
                faces.Add(f);
            }
            return faces;
        }

        public static Vector3 FaceNormal(Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 bSubA = Subtract(B, A);
            Vector3 cSubA = Subtract(C, A);
            Vector3 dir = CrossProduct(bSubA, cSubA);
            return Normalize(dir);
        }

        [ExcludeFromCodeCoverage]
        public static List<Vector3> ToVector3s(List<VertexIndex> vertices)
        {
            List<Vector3> vectors = new List<Vector3>();
            foreach (var vertexIndex in vertices)
            {
                vectors.Add(vertexIndex.Vector);
            }
            return vectors;
        }

        [ExcludeFromCodeCoverage]
        public static List<VertexIndex> ToVertexIndices(List<Vector3> vectors)
        {
            List<VertexIndex> vertices = new List<VertexIndex>();
            for (int index = 0; index < vectors.Count; index++)
            {
                var v = vectors[index];
                vertices.Add(new VertexIndex(v, index));
            }
            return vertices;
        }

        #region Vector

        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            float x = a.Y * b.Z - b.Y * a.Z;
            float y = a.Z * b.X - b.Z * a.X;
            float z = a.X * b.Y - b.X * a.Y;
            return new Vector3() { X = x, Y = y, Z = z };
        }

        public static Vector3 Subtract(Vector3 a, Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return new Vector3() { X = x, Y = y, Z = z };
        }

        public static Vector3 Add(Vector3 a, Vector3 b)
        {
            float x = a.X + b.X;
            float y = a.Y + b.Y;
            float z = a.Z + b.Z;
            return new Vector3() { X = x, Y = y, Z = z };
        }

        public static float Magnitude(Vector3 a)
        {
            return (float)Math.Sqrt(Math.Pow(a.X, 2) + Math.Pow(a.Y, 2) + Math.Pow(a.Z, 2));
        }

        public static Vector3 Divide(Vector3 a, float division)
        {
            Vector3 b = new Vector3();
            b.X = a.X / division;
            b.Y = a.Y / division;
            b.Z = a.Z / division;
            return b;
        }

        public static Vector3 Multiply(Vector3 a, float multiplication)
        {
            Vector3 b = new Vector3();
            b.X = a.X * multiplication;
            b.Y = a.Y * multiplication;
            b.Z = a.Z * multiplication;
            return b;
        }

        public static Vector3 Normalize(Vector3 a)
        {
            float mag = Magnitude(a);
            return Divide(a, mag);
        }

        public static Vector3 AvgVector(List<Vector3> vectors)
        {
            float x = 0;
            float y = 0;
            float z = 0;

            foreach (var v in vectors)
            {
                x += v.X;
                y += v.Y;
                z += v.Z;
            }
            x /= vectors.Count;
            y /= vectors.Count;
            z /= vectors.Count;
            return new Vector3 { X = x, Y = y, Z = z };
        }

        #endregion
    }
}
