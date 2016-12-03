using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UR10TCPController;
namespace UnitTests
{
    [TestClass]
    public class UnitTest_PathCreator
    {
        [TestMethod]
        public void TestCircle()
        {
            PathCreator pc = new PathCreator();
            pc.generate_circle();
        }

        [TestMethod]
        public void TestGenerateCirclePoses()
        {
            PathCreator pc = new PathCreator();
            pc.ReturnCircle();
        }



    }
}
