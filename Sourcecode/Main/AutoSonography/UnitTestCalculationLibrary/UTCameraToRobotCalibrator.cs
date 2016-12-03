using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CalculationLibrary;
using Microsoft.Kinect.Fusion;
using DataStructures;

namespace UnitTestCalculationLibrary
{
    
    [TestClass]
    public class UTCameraToRobotCalibrator
    {
        CameraToRobotCalibrator uut;
        [TestInitialize]
        public void Setup()
        {

        }

        //http://www.codinglabs.net/article_world_view_projection_matrix.aspx
        [TestMethod]
        public void CalculateTestMatrix_ValuesInserted_ExpectedMatrixReturned()
        {
            float x_scale = 1;
            float y_scale = 1;
            float z_scale = 1;
            float x_offset = 1.5f;
            float y_offset = 1.0f;
            float z_offset = 1.5f;
            float y_rotation = 90f;
            float x_rotation = 180f;
            uut = new CameraToRobotCalibrator(x_offset, y_offset, z_offset, x_scale, y_scale, z_scale, x_rotation, y_rotation, 0);
            Matrix4 expected = new Matrix4();
            expected.M11 = 0; expected.M12 = 0; expected.M13 = 1; expected.M14 = 1.5f;
            expected.M21 = 0; expected.M22 = -1; expected.M23 = 0; expected.M24 = 1f;
            expected.M31 = 1; expected.M32 = 0; expected.M33 = 0; expected.M34 = 1.5f;
            expected.M41 = 0; expected.M42 = 0; expected.M43 = 0; expected.M44 = 1f;
            Matrix4 actual = uut.CalculateTestMatrix();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TransformVertex_VertexInserted_CorrectVertexOutputted()
        {
            float x_scale = 1;
            float y_scale = 1;
            float z_scale = 1;
            float x_offset = 1.5f;
            float y_offset = 1.0f;
            float z_offset = 1.5f;
            float y_rotation = 90f;
            float x_rotation = 180f;
            uut = new CameraToRobotCalibrator(x_offset, y_offset, z_offset, x_scale, y_scale, z_scale, x_rotation, y_rotation, 0);
            Matrix4 expected = new Matrix4();
            expected.M11 = 0; expected.M12 = 0; expected.M13 = 1; expected.M14 = 1.5f;
            expected.M21 = 0; expected.M22 = -1; expected.M23 = 0; expected.M24 = 1f;
            expected.M31 = 1; expected.M32 = 0; expected.M33 = 0; expected.M34 = 1.5f;
            expected.M41 = 0; expected.M42 = 0; expected.M43 = 0; expected.M44 = 1f;
            Matrix4 matrix = uut.CalculateTestMatrix();

            Vector3 input = new Vector3() { X = 0f, Y = 1.0f, Z = 0f };
            Vector3 output = uut.TransformVertex(input, matrix);
            Assert.AreEqual(1.5, output.X);
            Assert.AreEqual(0, output.Y);
            Assert.AreEqual(1.5, output.Z);
        }
        
    }
    
}
