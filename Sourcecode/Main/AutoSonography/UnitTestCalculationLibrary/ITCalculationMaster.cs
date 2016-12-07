using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationLibrary;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTestCalculationLibrary
{
    [TestClass]
    public class ITCalculationMaster
    {
        private CalculationMaster uut;
        private string testModelLocation;
        [TestInitialize]
        public void Setup()
        {
            uut = new CalculationMaster();
            testModelLocation = Directory.GetParent(
                Directory.GetParent(
                Directory.GetParent(
                Environment.CurrentDirectory).
                ToString()).
                ToString()) + 
                "\\TestModels";
        }

        [TestMethod]
        public void FindPath_InsertFlatMeshFromCamera_OutputURPath()
        {
            string location = testModelLocation + "\\cameraBoxScan.ply";
            CVMesh mesh = PLYHandler.ReadMesh(location);
            List<URPose> poses = uut.FindPath(mesh, 2);
        }
    }
}
