using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationLibrary;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestCalculationLibrary
{
    [TestClass]
    public class ITCalculationMaster
    {
        private CalculationMaster uut;
        [TestInitialize]
        public void Setup()
        {
            uut = new CalculationMaster();
        }

        [TestMethod]
        public void FindPath_InsertFlatMeshFromCamera_OutputURPath()
        {
            string location = Environment.CurrentDirectory + "\\cameraBoxScan.ply";
            CVMesh mesh = PLYHandler.ReadMesh(location);
            List<URPose> poses = uut.FindPath(mesh, 2);
        }
    }
}
