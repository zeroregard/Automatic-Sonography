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
    public class UTSmoother
    {

        public Smoother uut;

        [TestInitialize]
        public void Setup()
        {
            uut = new Smoother();
        }

        
        [TestMethod]
        public void LaplacianFilter_BoxInserted_BoxSmoothed()
        {
            CVMesh mesh = PLYHandler.ReadMesh("box.ply");
            List<Vector3> vertices = mesh.Vertices;
            List<Vector3> newVertices = uut.LaplacianFilter(vertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            mesh.Vertices = newVertices;
            string location = @"D:\Projects\Bachelor\Sourcecode\Main\AutoSonography\UnitTestCalculationLibrary\bin\Debug";
            //PLYHandler.Output(mesh, ref location, false);
        }

        [TestMethod]
        public void LaplacianFilter_BoxInsertedTwice_BoxSmoothedMore()
        {
            CVMesh mesh = PLYHandler.ReadMesh("box.ply");
            List<Vector3> vertices = mesh.Vertices;
            List<Vector3> oneVertices = uut.LaplacianFilter(vertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            List<Vector3> twoVertices = uut.LaplacianFilter(oneVertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            mesh.Vertices = twoVertices;
            string location = @"D:\Projects\Bachelor\Sourcecode\Main\AutoSonography\UnitTestCalculationLibrary\bin\Debug";
            //PLYHandler.Output(mesh, ref location, false);
        }
        /*
        [TestMethod]
        public void LaplacianFilter_ChestInserted_ChestSmoothed()
        {
            CVMesh mesh = PLYHandler.ReadMesh("chest.ply");
            List<Vector3> vertices = Extensions.ToVectors(mesh.Vertices);
            List<Vector3> oneVertices = uut.LaplacianFilter(vertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            List<Vector3> twoVertices = uut.LaplacianFilter(oneVertices.ToArray(), mesh.TriangleIndeces.ToArray()).ToList();
            mesh.Vertices = Extensions.ToIndex(twoVertices);
            string location = @"D:\Projects\Bachelor\Sourcecode\Main\AutoSonography\UnitTestCalculationLibrary\bin\Debug";
            PLYHandler.Output(mesh, ref location, false);
        }*/
    }
}
