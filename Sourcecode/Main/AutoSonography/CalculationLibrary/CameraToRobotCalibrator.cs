using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DataStructures;
using Microsoft.Kinect.Fusion;
namespace CalculationLibrary
{
    public class CameraToRobotCalibrator
    {
        //offset in meters compared to robot origo
        public float x_offset = 0.175f;
        public float y_offset = -0.880f;
        public float z_offset = 1.800f;

        //scale
        public float x_scale = -1f;
        public float y_scale = 1f;
        public float z_scale = -1f;

        //camera rotated around its own origo
        public float x_rotation = 0f;
        public float y_rotation = 0f;
        public float z_rotation = -90f;

        public CameraToRobotCalibrator()
        {
            SetUpMatrices();
            TransformMatrix = CalculateTransformMatrix();
        }

        public CameraToRobotCalibrator(float xO, float yO, float zO, float xS, float yS, float zS, float xR, float yR, float zR)
        {

            x_offset = xO;
            y_offset = yO;
            z_offset = zO;
            x_scale = xS;
            y_scale = yS;
            z_scale = zS;
            x_rotation = xR;
            y_rotation = yR;
            z_rotation = zR;
            SetUpMatrices();
            TransformMatrix = CalculateTransformMatrix();
        }

        public Matrix4 TransformMatrix;
        /// <summary>
        /// Translates, rotates and scales a mesh from the 3D camera's view to the Robot Arm's view
        /// </summary>
        /// <param name="mesh">The mesh we wish to transform</param>
        /// <returns>A mesh in another space</returns>
        public CVMesh ConvertToRobospace(CVMesh mesh)
        {
            CVMesh converted = CVMesh.Clone(mesh);
            List<Vector3> vertices = converted.Vertices;
            List<Vector3> transformedVertices = new List<Vector3>();
            foreach (var v in vertices)
            {
                Vector3 transformed = TransformVertex(v, TransformMatrix);
                transformedVertices.Add(transformed);
            }
            converted.Vertices = transformedVertices;

            return converted;
        }

        Matrix4 t; //translation
        Matrix4 x; //rotation x
        Matrix4 y; //rotation y
        Matrix4 z; //rotation z
        Matrix4 s; //scale

        /// <summary>
        /// Sets up the individual matrices used for moving, rotating and rescaling a mesh
        /// </summary>
        public void SetUpMatrices()
        {
            x_rotation = Extensions.ToRadians(x_rotation);
            y_rotation = Extensions.ToRadians(y_rotation);
            z_rotation = Extensions.ToRadians(z_rotation);
            t = new Matrix4();
            t.M11 = 1; t.M12 = 0; t.M13 = 0; t.M14 = x_offset;
            t.M21 = 0; t.M22 = 1; t.M23 = 0; t.M24 = y_offset;
            t.M31 = 0; t.M32 = 0; t.M33 = 1; t.M34 = z_offset;
            t.M41 = 0; t.M42 = 0; t.M43 = 0; t.M44 = 1;

            x = new Matrix4();
            x.M11 = 1; x.M12 = 0; x.M13 = 0; x.M14 = 0;
            x.M21 = 0; x.M22 = (float)Math.Cos(x_rotation); x.M23 = -(float)Math.Sin(x_rotation); x.M24 = 0;
            x.M31 = 0; x.M32 = (float)Math.Sin(x_rotation); x.M33 = (float)Math.Cos(x_rotation); x.M34 = 0;
            x.M41 = 0; x.M42 = 0; x.M43 = 0; x.M44 = 1;
            x = Prune(x);
            y = new Matrix4();
            y.M11 = (float)Math.Cos(y_rotation); y.M12 = 0; y.M13 = (float)Math.Sin(y_rotation); y.M14 = 0;
            y.M21 = 0; y.M22 = 1; y.M23 = 0; y.M24 = 0;
            y.M31 = -(float)Math.Sin(y_rotation); y.M32 = 0; y.M33 = (float)Math.Cos(y_rotation); y.M34 = 0;
            y.M41 = 0; y.M42 = 0; y.M43 = 0; y.M44 = 1;
            y = Prune(y);
            z = new Matrix4();
            z.M11 = (float)Math.Cos(z_rotation); z.M12 = -(float)Math.Sin(z_rotation); z.M13 = 0; z.M14 = 0;
            z.M21 = (float)Math.Sin(z_rotation); z.M22 = (float)Math.Cos(z_rotation); z.M23 = 0; z.M24 = 0;
            z.M31 = 0; z.M32 = 0; z.M33 = 1; z.M34 = 0;
            z.M41 = 0; z.M42 = 0; z.M43 = 0; z.M44 = 1;
            z = Prune(z);
            s = new Matrix4();
            s.M11 = x_scale; s.M12 = 0; s.M13 = 0; s.M14 = 0;
            s.M21 = 0; s.M22 = y_scale; s.M23 = 0; s.M24 = 0;
            s.M31 = 0; s.M32 = 0; s.M33 = z_scale; s.M34 = 0;
            s.M41 = 0; s.M42 = 0; s.M43 = 0; s.M44 = 1;

        }

        /// <summary>
        /// Sets very small values to zero in a Matrix4
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private Matrix4 Prune(Matrix4 m)
        {
            //Apparently Matrix4s are copied over entirely when put as a parameter
            float min = 0.001f;
            m.M11 = Math.Abs(m.M11) < min ? 0 : m.M11;
            m.M12 = Math.Abs(m.M12) < min ? 0 : m.M12;
            m.M13 = Math.Abs(m.M13) < min ? 0 : m.M13;
            m.M14 = Math.Abs(m.M14) < min ? 0 : m.M14;

            m.M21 = Math.Abs(m.M21) < min ? 0 : m.M21;
            m.M22 = Math.Abs(m.M22) < min ? 0 : m.M22;
            m.M23 = Math.Abs(m.M23) < min ? 0 : m.M23;
            m.M24 = Math.Abs(m.M24) < min ? 0 : m.M24;

            m.M31 = Math.Abs(m.M31) < min ? 0 : m.M31;
            m.M32 = Math.Abs(m.M32) < min ? 0 : m.M32;
            m.M33 = Math.Abs(m.M33) < min ? 0 : m.M33;
            m.M34 = Math.Abs(m.M34) < min ? 0 : m.M34;

            m.M41 = Math.Abs(m.M41) < min ? 0 : m.M41;
            m.M42 = Math.Abs(m.M42) < min ? 0 : m.M42;
            m.M43 = Math.Abs(m.M43) < min ? 0 : m.M43;
            m.M44 = Math.Abs(m.M44) < min ? 0 : m.M44;
            return m;
        }

        /// <summary>
        /// Sets up the entire transform matrix, multiplying matrices in the correct order
        /// </summary>
        /// <returns></returns>
        public Matrix4 CalculateTransformMatrix()
        {
            //rotate the Kinect 90 degrees around the z-axis
            //mirror x
            //mirror z
            Matrix4 rotatedError = Extensions.Dot(x, z);
            Matrix4 mirrored = Extensions.Dot(s, rotatedError);
            Matrix4 translated = Extensions.Dot(t, mirrored);
            return translated;
        }

        [ExcludeFromCodeCoverage]
        public Matrix4 CalculateTestMatrix()
        {
            Matrix4 baseMatrix = y;
            Matrix4 rotated = Extensions.Dot(x, y);
            Matrix4 translated = Extensions.Dot(t, rotated);
            return translated;
        }

        /// <summary>
        /// Moves a vertex according to rotation, scale and translation to a new coordinate
        /// </summary>
        /// <param name="vertex">X, Y, and Z position</param>
        /// <param name="transformM">Transform matrix to convert vertex with</param>
        /// <returns>The new X, Y and Z position of a Vertex</returns>
        public Vector3 TransformVertex(Vector3 vertex, Matrix4 transformM)
        {
            float x, y, z;
            x = transformM.M11 * vertex.X + transformM.M12 * vertex.Y + transformM.M13 * vertex.Z + transformM.M14 * 1;
            y = transformM.M21 * vertex.X + transformM.M22 * vertex.Y + transformM.M23 * vertex.Z + transformM.M24 * 1;
            z = transformM.M31 * vertex.X + transformM.M32 * vertex.Y + transformM.M33 * vertex.Z + transformM.M34 * 1;
            return new Vector3 { X = x, Y = y, Z = z };
        }

    }
}
