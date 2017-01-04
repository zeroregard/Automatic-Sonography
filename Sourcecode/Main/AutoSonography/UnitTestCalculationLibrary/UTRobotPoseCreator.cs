using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationLibrary;
using DataStructures;
using Microsoft.Kinect.Fusion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestCalculationLibrary
{
    [TestClass]
    public class UTRobotPoseCreator
    {
        public RobotPoseCreator uut;
        [TestInitialize]
        public void Setup()
        {
            uut = new RobotPoseCreator();
        }

        /*
        [TestMethod]
        public void FindURPosition_VertexAndNormalInserted_OffsetPositionReturned()
        {
            Vector3 origo = new Vector3 { X = 0, Y = 0, Z = 0 };
            Vector3 up = new Vector3 { X = 0, Y = 0, Z = 1 };
            RobotPoseCreator.UR_PROBE_OFFSET = 0.12f;
            Vector3 pos = uut.FindURPosition(origo, up);
            Assert.AreEqual(0, pos.X);
            Assert.AreEqual(0, pos.Y);
            Assert.AreEqual(0.12f, pos.Z);
        }


        [TestMethod]
        public void ToURPath_FourTrianglesInserted_AllURPosesPointDown()
        {
            string folder = Environment.CurrentDirectory + "\\fourTriangles.ply";
            CVMesh mesh = PLYHandler.ReadMesh(folder);
            List<VertexIndex> path = Extensions.ToVertexIndices(mesh.Vertices);
            List<URPose> poses = uut.ToURPath(path, mesh);
            foreach (var p in poses)
            {
                Assert.AreEqual(Math.PI*3, p.RXpose, 0.0001);
                Assert.AreEqual(0, p.RYpose);
                Assert.AreEqual(0, p.RZpose);
            }
        }

        [TestMethod]
        public void ToURPath_OnlyVerticesInserted_AllURPosesPointDown()
        {
            CVMesh mesh = new CVMesh();
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < 10; i++)
            {
                vertices.Add(new Vector3 {X = i, Y = i, Z = i});
            }
            mesh.Vertices = vertices;
            List<URPose> poses = uut.ToURPath(Extensions.ToVertexIndices(vertices), mesh);
            foreach (var urPose in poses)
            {
                Assert.AreEqual(Math.PI * 3, urPose.RXpose, 0.0001);
            }
        }
        */

        [TestMethod]
        public void ToRollPitchYaw_InsertUp_ExpectedValuesReturned()
        {
            Vector3 up = new Vector3 {X=0, Y=0, Z=1};
            Vector3 rpy = uut.ToRollPitchYaw(up);
            Assert.AreEqual(0, rpy.X, 0.0001);
            Assert.AreEqual(0, rpy.Y, 0.0001);
            Assert.AreEqual(0, rpy.Z, 0.0001);
        }

        [TestMethod]
        public void ToRollPitchYaw_InsertDown_ExpectedValuesReturned()
        {
            Vector3 down = new Vector3 { X = 0, Y = 0, Z = -1 };
            Vector3 rpy = uut.ToRollPitchYaw(down);
            Assert.AreEqual(Math.PI, rpy.X, 0.0001);
            Assert.AreEqual(0, rpy.Y, 0.0001);
            Assert.AreEqual(0, rpy.Z, 0.0001);
        }

        [TestMethod]
        public void ToRollPitchYaw_InsertLeft_ExpectedValuesReturned()
        {
            Vector3 left = new Vector3 { X = 0, Y = -1, Z = 0 };
            Vector3 rpy = uut.ToRollPitchYaw(left);
            Assert.AreEqual(Math.PI/2, rpy.X, 0.0001);
            Assert.AreEqual(0, rpy.Y, 0.0001);
            Assert.AreEqual(0, rpy.Z, 0.0001);
        }

        [TestMethod]
        public void ToRollPitchYaw_InsertRight_ExpectedValuesReturned()
        {
            Vector3 right = new Vector3 { X = 0, Y = 1, Z = 0 };
            Vector3 rpy = uut.ToRollPitchYaw(right);
            Assert.AreEqual(Math.PI / 2, rpy.X, 0.0001);
            Assert.AreEqual(0, rpy.Y, 0.0001);
            Assert.AreEqual(Math.PI, rpy.Z, 0.0001);
        }

        [TestMethod]
        public void ToRollPitchYaw_InsertForward_ExpectedValuesReturned()
        {
            Vector3 forward = new Vector3 { X = -1, Y = 0, Z = 0 };
            Vector3 rpy = uut.ToRollPitchYaw(forward);
            Assert.AreEqual(Math.PI / 2, rpy.X, 0.0001);
            Assert.AreEqual(0, rpy.Y, 0.0001);
            Assert.AreEqual(Math.PI / 2, rpy.Z, 0.0001);
        }

        [TestMethod]
        public void ToRollPitchYaw_InsertPointAtCorner_ExpectedValuesReturned()
        {
            Vector3 vector = new Vector3 {X = 0.500f, Y = 0.500f, Z = 0.707f};
            vector = Extensions.Normalize(vector);
            Vector3 rpy = uut.ToRollPitchYaw(vector);
            double xDeg = Extensions.ToDegrees(rpy.X);
            double zDeg = Extensions.ToDegrees(rpy.Z);
            Assert.AreEqual(45, xDeg, 0.1);
            Assert.AreEqual(0, rpy.Y, 0.1);
            Assert.AreEqual(135, zDeg, 0.1);
        }

        /*
        [TestMethod]
        public void ToRollPitchYaw_TEST_ExpectedValuesReturned()
        {
            Vector3 vector = new Vector3 { X = -0.500f, Y = -0.500f, Z = 0.707f };
            vector = Extensions.Normalize(vector);
            Vector3 rpy = uut.ToRollPitchYaw(vector);
            double xDeg = RadianToDegree(rpy.X);
            double zDeg = RadianToDegree(rpy.Z);
        }*/

        [TestMethod]
        public void ToRotVector_Up_ExpectedValuesReturned()
        {
            Vector3 up = new Vector3 {X = 0, Y = 0, Z = 0};
            Vector3 rotVec = uut.ToRotVector(up);
            Assert.AreEqual(0, rotVec.X);
            Assert.AreEqual(0, rotVec.Y);
            Assert.AreEqual(0, rotVec.Z);
        }

        [TestMethod]
        public void ToRotVector_Down_ExpectedValuesReturned()
        {
            Vector3 down = new Vector3 { X = (float)Math.PI, Y = 0, Z = 0 };
            Vector3 rotVec = uut.ToRotVector(down);
            Assert.AreEqual(Math.PI, Math.Abs(rotVec.X), 0.1);
            Assert.AreEqual(0, rotVec.Y);
            Assert.AreEqual(0, rotVec.Z);
        }

        [TestMethod]
        public void ToRotVector_Left_ExpectedValuesReturned()
        {
            Vector3 left = new Vector3 { X = (float)Math.PI / 2, Y = 0, Z = 0 };
            Vector3 rotVec = uut.ToRotVector(left);
            Assert.AreEqual(Math.PI/2, rotVec.X, 0.1);
            Assert.AreEqual(0, rotVec.Y);
            Assert.AreEqual(0, rotVec.Z);
        }

        [TestMethod]
        public void ToRotVector_Right_ExpectedValuesReturned()
        {
            Vector3 right = new Vector3 { X = -(float)Math.PI / 2, Y = 0, Z = 0 };
            Vector3 rotVec = uut.ToRotVector(right);
            Assert.AreEqual(-Math.PI/2, rotVec.X, 0.1);
            Assert.AreEqual(0, rotVec.Y, 0.1);
            Assert.AreEqual(0, rotVec.Z, 0.1);
        }

        [TestMethod]
        public void ToRotVector_Foward_ExpectedValuesReturned()
        {
            Vector3 forward = new Vector3 { X = (float)Math.PI / 2, Y = 0, Z = -(float)Math.PI / 2 };
            Vector3 rotVec = uut.ToRotVector(forward);
            Assert.AreEqual(1.2091, rotVec.X, 0.1);
            Assert.AreEqual(-1.2091, rotVec.Y, 0.1);
            Assert.AreEqual(-1.2091, rotVec.Z, 0.1);
        }

        [TestMethod]
        public void ToRotVector_Backward_ExpectedValuesReturned()
        {
            Vector3 forward = new Vector3 { X = (float)Math.PI / 2, Y = 0, Z = (float)Math.PI / 2 };
            Vector3 rotVec = uut.ToRotVector(forward);
            Assert.AreEqual(1.2091, rotVec.X, 0.1);
            Assert.AreEqual(1.2091, rotVec.Y, 0.1);
            Assert.AreEqual(1.2091, rotVec.Z, 0.1);
        }

    }
}
