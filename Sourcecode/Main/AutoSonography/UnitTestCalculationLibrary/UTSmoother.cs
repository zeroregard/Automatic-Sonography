using System;
using System.Collections.Generic;
using System.IO;
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
    public class UTSmoother
    {
        private string testModelLocation;
        public Smoother uut;

        [TestInitialize]
        public void Setup()
        {
            uut = new Smoother();
            testModelLocation = Directory.GetParent(
                Directory.GetParent(
                Directory.GetParent(
                Environment.CurrentDirectory).
                ToString()).
                ToString()) +
                "\\TestModels";
        }

        
        [TestMethod]
        public void LaplacianFilter_BoxInserted_BoxSmoothed()
        {
            string location = testModelLocation + "\\box.ply";
            CVMesh mesh = PLYHandler.ReadMesh(location);
            List<Vector3> vertices = mesh.Vertices;
            List<Vector3> newVertices = uut.LaplacianFilter(vertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            mesh.Vertices = newVertices;
        }

        [TestMethod]
        public void LaplacianFilter_BoxInsertedTwice_BoxSmoothedMore()
        {
            string location = testModelLocation + "\\box.ply";
            CVMesh mesh = PLYHandler.ReadMesh(location);
            List<Vector3> vertices = mesh.Vertices;
            List<Vector3> oneVertices = uut.LaplacianFilter(vertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            List<Vector3> twoVertices = uut.LaplacianFilter(oneVertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            mesh.Vertices = twoVertices;
        }
    }
}
