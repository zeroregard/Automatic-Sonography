using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboLibrary;
using RoboLibrary.Dummies;
using System.Collections.Generic;
using System.Threading;
using DataStructures;
namespace UnitTestRobotLibrary
{
    [TestClass]
    public class UTPathFeeder
    {
        PathFeeder uut;
        private DummyData data;
        private DummyLogic logic;
        double posBounds;
        double rotBounds;
        URPose p1;
        URPose p2;
        [TestInitialize]
        public void Setup()
        {
            data = new DummyData();
            logic = new DummyLogic(data);
            uut = new PathFeeder(data, logic);
            posBounds = PathFeeder.MinimumProximityPosition;
            rotBounds = PathFeeder.MinimumProximityRotation;
            p1 = new URPose(1, 1, 1, 1, 1, 1);
            p2 = URPose.Copy(p1);
        }

        [TestMethod]
        public void IsCloseEnough_WithTwoEqualPoses_ReturnsTrue()
        {
            bool result = uut.IsCloseEnough(p1, p2);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void IsCloseEnough_WithTwoPosesWithinPositionBoundsWithinRotationBounds_ReturnsTrue()
        {
            p2.Xpose += posBounds;
            p2.Ypose += posBounds;
            p2.Zpose += posBounds;
            p2.RXpose += rotBounds;
            p2.RYpose += rotBounds;
            p2.RZpose += rotBounds;

            bool result = uut.IsCloseEnough(p1, p2);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void IsCloseEnough_WithTwoPosesWithinPositionBoundsNotWithinRotationBounds_ReturnsFalse()
        {
            p2.Xpose += 4*posBounds;
            p2.Ypose += posBounds;
            p2.Zpose += posBounds;
            p2.RXpose += 2*rotBounds;
            p2.RYpose += rotBounds;
            p2.RZpose += rotBounds;

            bool result = uut.IsCloseEnough(p1, p2);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void IsCloseEnough_WithTwoPosesNotWithinPositionBoundsWithinRotationBounds_ReturnsFalse()
        {
            p2.Xpose += 2*posBounds;
            p2.Ypose += posBounds;
            p2.Zpose += posBounds;
            p2.RXpose += rotBounds;
            p2.RYpose += rotBounds;
            p2.RZpose += rotBounds;

            bool result = uut.IsCloseEnough(p1, p2);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void IsCloseEnough_WithTwoPosesNotWithinBounds_ReturnsFalse()
        {
            p2.Xpose += 2*posBounds;
            p2.Ypose += posBounds;
            p2.Zpose += posBounds;
            p2.RXpose += 2*rotBounds;
            p2.RYpose += rotBounds;
            p2.RZpose += rotBounds;

            bool result = uut.IsCloseEnough(p1, p2);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void IsCloseEnough_WithOnePoseOtherNull_ReturnsFalse()
        {
            bool result = uut.IsCloseEnough(p1, null);
            Assert.AreEqual(false, result);
        }
        [TestMethod]
        public void RunPath_100PosesInserted_StandardPoseIsCurrentPose()
        {
            List<URPose> poses = new List<URPose>();
            for (int i = 0; i < 100; i++)
            {
                poses.Add(new URPose(1,1,1,1,1,1));
            }
            uut.RunPath(poses);
            var current = data.GetLastKnownPose();
            Assert.AreEqual(0, current.Xpose);
            Assert.AreEqual(0, current.Ypose);
            Assert.AreEqual(0, current.Zpose);
            Assert.AreEqual(0, current.RXpose);
            Assert.AreEqual(0, current.RYpose);
            Assert.AreEqual(0, current.RZpose);
        }

        public void StopRunning_10000PosesInserted_StandardPoseIsCurrentPose()
        {
            List<URPose> poses = new List<URPose>();
            for (int i = 0; i < 10000; i++)
            {
                poses.Add(new URPose(1, 1, 1, 1, 1, 1));
            }
            uut.RunPath(poses);
            uut.StopRunning();
            var current = data.GetLastKnownPose();
            Assert.AreEqual(0, current.Xpose);
            Assert.AreEqual(0, current.Ypose);
            Assert.AreEqual(0, current.Zpose);
            Assert.AreEqual(0, current.RXpose);
            Assert.AreEqual(0, current.RYpose);
            Assert.AreEqual(0, current.RZpose);
        }


    }
}
