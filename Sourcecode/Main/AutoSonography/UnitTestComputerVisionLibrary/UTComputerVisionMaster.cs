using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComputerVisionLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataStructures;
using Microsoft.Kinect.Fusion;

namespace UnitTestComputerVisionLibrary
{
    [TestClass]
    public class UTComputerVisionMaster
    {
        [TestMethod]
        public void RequestCurrentImageAsMesh_CallMethod_NewMeshArrived()
        {
            ComputerVisionMaster uut = new ComputerVisionMaster();
            uut.fusionizer = new DummyKinect(uut);
            Assert.AreEqual(null, uut.LastMesh);
            uut.RequestCurrentImageAsMesh();
            Assert.AreNotEqual(null, uut.LastMesh);
        }
    }
}
