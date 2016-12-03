using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ComputerVisionLibrary;
using System.Threading;
using DataStructures;
using Microsoft.Kinect.Fusion;

namespace UnitTestRobotLibrary
{
    [TestClass]
    public class UTSlicer
    {
        private Slicer uut;
        [TestInitialize]
        public void Setup()
        {
            uut = new Slicer();
        }

        [TestMethod]
        public void Slice_InsertFourFaces_2Returned()
        {
            //Mesh has 10 vertices, of which 8 are unique. It has 4 faces.
            //We're slicing halfway through the third face, so we should end up with 2 faces and 5 vertices.
            string location = Environment.CurrentDirectory + @"\fourTriangles.ply";
            CVMesh mesh = PLYHandler.ReadMesh(location);
            float lowerLimit = 58.5f;
            float upperLimit = 1000f;
            Slicer.xMin = float.MinValue;
            Slicer.xMax = float.MaxValue;
            CVMesh sliced = uut.Slice(mesh, lowerLimit, upperLimit, false);
            Assert.AreEqual(5, sliced.Vertices.Count);
            Assert.AreEqual(0, sliced.Faces[0].index1);
            Assert.AreEqual(1, sliced.Faces[0].index2);
            Assert.AreEqual(2, sliced.Faces[0].index3);
            Assert.AreEqual(3, sliced.Faces[1].index1);
            Assert.AreEqual(2, sliced.Faces[1].index2);
            Assert.AreEqual(4, sliced.Faces[1].index3);
        }

        [TestMethod]
        public void RemoveNonSurfaceFaces_InsertBox_BoxTopRemoved()
        {
            CVMesh mesh = PLYHandler.ReadMesh(Environment.CurrentDirectory + @"\box.ply");
            Slicer.xMin = double.MinValue;
            Slicer.xMax = double.MaxValue;
            CVMesh meshOut = uut.Slice(mesh, double.MinValue, double.MaxValue, false);

        }

        [TestMethod]
        public void Slice_Inserted45VerticesOutOfBound_0Returned()
        {
            CVMesh mesh = new CVMesh();
            List<Vector3> vertices = new List<Vector3>();
            int lowerLimit = 10;
            int upperLimit = 20;
            Slicer.xMin = lowerLimit;
            Slicer.xMax = upperLimit;
            for (int i = 0; i < 45; i++)
            {
                int x = RandomOutSideOfRange(lowerLimit, upperLimit);
                int y = RandomOutSideOfRange(lowerLimit, upperLimit);
                vertices.Add(new Vector3 {X = x, Y = y, Z = 0});
            }
            mesh.Vertices = vertices;
            CVMesh sliced = uut.Slice(mesh, lowerLimit, upperLimit, false);
            Assert.AreEqual(0,sliced.Vertices.Count);
        }
        
        public int RandomOutSideOfRange(int minExclusive, int maxExclusive)
        {
            Random r = new Random();
            bool lowerThan = r.NextDouble() >= 0.5;
            if (lowerThan)
                return r.Next(int.MinValue, minExclusive);
            return r.Next(maxExclusive + 1, int.MaxValue);
        }

    }
}
