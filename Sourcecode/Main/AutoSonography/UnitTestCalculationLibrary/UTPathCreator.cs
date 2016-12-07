using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using CalculationLibrary;
using Microsoft.Kinect.Fusion;
using DataStructures;

namespace UnitTestCalculationLibrary
{
    [TestClass]
    public class UTPathCreator
    {
        PathCreator uut;
        private string testModelLocation;
        [TestInitialize]
        public void Setup()
        {
            uut = new PathCreator();
            testModelLocation = Directory.GetParent(
                Directory.GetParent(
                Directory.GetParent(
                Environment.CurrentDirectory).
                ToString()).
                ToString()) +
                "\\TestModels";
        }

        [TestMethod]
        public void DistanceToLine_LineAndPointInserted_CorrectDistanceReturned()
        {
            Vector3 lineStart = new Vector3 { X = 0, Y = 0, Z = 0 };
            Vector3 lineEnd = new Vector3 { X = 0, Y = -1, Z = 0 };
            Vector3 point = new Vector3 { X = 0.5f, Y = -0.5f, Z = 0 };

            float result = uut.DistanceToLine(lineStart, lineEnd, point);
            Assert.AreEqual(0.5f, result);
        }

        [TestMethod]
        public void IdentifyPath_InsertPlane_CorrectPathOutPut()
        {
            string location = testModelLocation + "\\plane.ply";
            CVMesh mesh = PLYHandler.ReadMesh(location);
            BoundingBox b = Extensions.FindBoundingBox(mesh.Vertices);
            uut.distance_length = -(b.x_max - b.x_min) / 9f;
            uut.distance_width = -(b.y_max - b.y_min) / 9f;
            List<VertexIndex> path = uut.CreatePath(mesh.Vertices);
            path = Extensions.PruneVertices(path);
            Assert.AreEqual(path.Count, mesh.Vertices.Count);
        }
    }
}
