using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Fusion;
namespace DataStructures
{
    public class VertexIndex //Used in path creator for referencing back to the original vector in a mesh
    {
        public Vector3 Vector;
        public int Index;

        public VertexIndex(Vector3 v, int i)
        {
            Vector = v;
            Index = i;
        }
    }
    //a box around a mesh
    public class BoundingBox
    {
        public float x_min;
        public float x_max;
        public float y_min;
        public float y_max;
        public float z_min;
        public float z_max;
    }

    public class Matrix3
    {
        //First coordinate is row, second column
        //Most math examples have row first and column second, Microsoft's Matrix4 reverses this though
        public double M00; public double M01; public double M02;
        public double M10; public double M11; public double M12;
        public double M20; public double M21; public double M22;

        public static Matrix3 Dot(Matrix3 a, Matrix3 b)
        {
            Matrix3 ab = new Matrix3();
            ab.M00 = a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20;
            ab.M01 = a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21;
            ab.M02 = a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22;
            ab.M10 = a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20;
            ab.M11 = a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21;
            ab.M12 = a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22;
            ab.M20 = a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20;
            ab.M21 = a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21;
            ab.M22 = a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22;
            return ab;
        }
    }
}
