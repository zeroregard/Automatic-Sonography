using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboLibrary;
using RoboLibrary.Dummies;

namespace UnitTestRobotLibrary
{
    [TestClass]
    public class UTData
    {
        private Data uut;

        [TestInitialize]
        public void SetUp()
        {
            uut = new Data();
        }

        [TestMethod]
        public void FetchVelocity_VelocitySetUp_SpeedAndAccelerationWasSetUp()
        {
            uut.Init(new DummyReader(uut), new DummyWriter(uut));
            uut.FetchCurrentConfiguration();
            Assert.AreEqual(uut.Speed, uut.CurrentConfiguration.Speed);
            Assert.AreEqual(uut.Acceleration, uut.CurrentConfiguration.Acceleration);
        }

        [TestMethod]
        public void ChangeVelocity_NewValuesInserted_NewValuesCommitted()
        {
            uut.Init(new DummyReader(uut), new DummyWriter(uut));
            uut.FetchCurrentConfiguration();
            Assert.AreEqual(uut.Speed, uut.CurrentConfiguration.Speed);
            Assert.AreEqual(uut.Acceleration, uut.CurrentConfiguration.Acceleration);
            float speed = 1;
            float acceleration = 1;
            uut.ChangeVelocity(speed, acceleration);
            uut.FetchCurrentConfiguration();
            Assert.AreEqual(speed, uut.CurrentConfiguration.Speed);
            Assert.AreEqual(speed, uut.CurrentConfiguration.Acceleration);
        }

        [TestMethod]
        public void SendNewPose_PoseSent_LastKnownPoseIsPoseSent()
        {
            uut.Init(new DummyReader(uut), new DummyWriter(uut));
            URPose p = new URPose(0.5, 0.5, 0.5, 1,1,1);
            uut.SendNewPose(p);
            URPose r = uut.GetLastKnownPose();
            Assert.AreEqual(p.Xpose, r.Xpose);
            Assert.AreEqual(p.Ypose, r.Ypose);
            Assert.AreEqual(p.Zpose, r.Zpose);
            Assert.AreEqual(p.RXpose, r.RXpose);
            Assert.AreEqual(p.RYpose, r.RYpose);
            Assert.AreEqual(p.RZpose, r.RZpose);
        }
    }
}
