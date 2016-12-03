using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.Kinect.Fusion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestDataStructures
{
    [TestClass]
    public class UTExtensions
    {
        [TestInitialize]
        public void Setup()
        {

        }

        [TestMethod]
        public void FindBoundingBox_InsertTwoDefiningPoints_BoundingBoxFound()
        {
            Vector3 v1 = new Vector3 { X = 1, Y = 1, Z = 2 };
            Vector3 v2 = new Vector3 { X = -1, Y = -1, Z = 0 };
            List<Vector3> vertices = new List<Vector3> { v1, v2 };
            BoundingBox b = Extensions.FindBoundingBox(vertices);
            Assert.AreEqual(-1, b.x_min);
            Assert.AreEqual(1, b.x_max);
            Assert.AreEqual(-1, b.y_min);
            Assert.AreEqual(1, b.y_max);
            Assert.AreEqual(0, b.z_min);
            Assert.AreEqual(2, b.z_max);
        }

        [TestMethod]
        public void MidPoint_InsertedOrigoBox_OrigoReturned()
        {
            BoundingBox b = new BoundingBox();
            b.x_min = -1;
            b.x_max = 1;
            b.y_min = -1;
            b.y_max = 1;
            b.z_min = -1;
            b.z_max = 1;
            Vector3 mid = Extensions.MidPoint(b);
            Assert.AreEqual(0, mid.X);
            Assert.AreEqual(0, mid.Y);
            Assert.AreEqual(0, mid.Z);
        }

        [TestMethod]
        public void MidPoint_InsertedOffsetBox_BoxOffsetReturned()
        {
            BoundingBox b = new BoundingBox();
            b.x_min = 0;
            b.x_max = 2;
            b.y_min = 0;
            b.y_max = 2;
            b.z_min = 0;
            b.z_max = 2;
            Vector3 mid = Extensions.MidPoint(b);
            Assert.AreEqual(1, mid.X);
            Assert.AreEqual(1, mid.Y);
            Assert.AreEqual(1, mid.Z);
        }

        [TestMethod]
        public void PruneVertices_InsertedPathWithDupes_NoDupesInPath()
        {
            List<VertexIndex> vertexIndices = new List<VertexIndex>();
            for (int i = 0; i < 10; i++)
            {
                vertexIndices.Add(new VertexIndex(new Vector3 { X = i, Y = i, Z = i }, i));
                vertexIndices.Add(new VertexIndex(new Vector3 { X = i, Y = i, Z = i }, i));
            }
            Assert.AreEqual(20, vertexIndices.Count);
            vertexIndices = Extensions.PruneVertices(vertexIndices);
            Assert.AreEqual(10, vertexIndices.Count);
        }

        [TestMethod]
        public void PrunePoints_InsertedPointsWithDupes_NoDupesReturned()
        {
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < 10; i++)
            {
                vertices.Add(new Vector3 { X = i, Y = i, Z = i });
                vertices.Add(new Vector3 { X = i, Y = i, Z = i });
            }
            Assert.AreEqual(20, vertices.Count);
            vertices = Extensions.PrunePoints(vertices);
            Assert.AreEqual(10, vertices.Count);
        }
    }
}
