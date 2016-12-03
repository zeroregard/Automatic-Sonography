using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboLibrary;
using RoboLibrary.Dummies;
using System.Collections.Generic;
using ComputerVisionLibrary;
using System.Threading;
using DataStructures;
namespace UnitTestRobotLibrary
{
    [TestClass]
    public class UTRoboMaster
    {
        private RoboMaster uut;
        private DummyData data;
        private DummyPathFeeder feeder;
        [TestInitialize]
        public void Setup()
        {
            feeder = new DummyPathFeeder();
            data = new DummyData();
            uut = new RoboMaster(feeder, data);
        }
        
        [TestMethod]
        public void ProcessPath_InsertTwoPoses_RunsTwoPoses()
        {
            List<URPose> path = new List<URPose>();
            path.Add(new URPose(0, 0, 0, 0, 0, 0));
            path.Add(new URPose(1, 1, 1, 0, 0, 0));
            uut.ProcessPath(path);
            Thread.Sleep(feeder.SleepTime/2);
            Assert.AreEqual(0, feeder.CurrentPose.Xpose);
            Assert.AreEqual(0, feeder.CurrentPose.Ypose);
            Assert.AreEqual(0, feeder.CurrentPose.Zpose);
            Thread.Sleep(feeder.SleepTime + 1);
            Assert.AreEqual(1, feeder.CurrentPose.Xpose);
            Assert.AreEqual(1, feeder.CurrentPose.Ypose);
            Assert.AreEqual(1, feeder.CurrentPose.Zpose);
        }
        
        [TestMethod]
        public void PauseScanning_PauseMidway_FirstVerticeReturned()
        {
            List<URPose> path = new List<URPose>();
            path.Add(new URPose(0, 0, 0, 0, 0, 0));
            path.Add(new URPose(1, 1, 1, 0, 0, 0));
            uut.ProcessPath(path);
            Thread.Sleep(feeder.SleepTime / 2);
            uut.PauseScanning();
            Assert.AreEqual(0, feeder.CurrentPose.Xpose);
            Assert.AreEqual(0, feeder.CurrentPose.Ypose);
            Assert.AreEqual(0, feeder.CurrentPose.Zpose);
        }

        
        [TestMethod]
        public void PauseScanning_ResumeAfterPause_PathEventuallyFinishes()
        {
            List<URPose> path = new List<URPose>();
            path.Add(new URPose(0, 0, 0, 0, 0, 0));
            path.Add(new URPose(1, 1, 1, 0, 0, 0));
            uut.ProcessPath(path);
            Thread.Sleep(feeder.SleepTime / 2);
            uut.PauseScanning();
            Assert.AreEqual(0, feeder.CurrentPose.Xpose);
            Assert.AreEqual(0, feeder.CurrentPose.Ypose);
            Assert.AreEqual(0, feeder.CurrentPose.Zpose);
            uut.PauseScanning();
            Thread.Sleep(feeder.SleepTime+1);
            Assert.AreEqual(1, feeder.CurrentPose.Xpose);
            Assert.AreEqual(1, feeder.CurrentPose.Ypose);
            Assert.AreEqual(1, feeder.CurrentPose.Zpose);
        }

        [TestMethod]
        public void SetStandardPose_StandardPoseBeingSet_LastKnownPoseIsStandard()
        {
            uut.SetStandardPose();
            var pose = data.GetLastKnownPose();
            Assert.AreEqual(RoboMaster.standard_pose_x, pose.Xpose);
            Assert.AreEqual(RoboMaster.standard_pose_y, pose.Ypose);
            Assert.AreEqual(RoboMaster.standard_pose_z, pose.Zpose);
            Assert.AreEqual(RoboMaster.standard_pose_rx, pose.RXpose);
            Assert.AreEqual(RoboMaster.standard_pose_ry, pose.RYpose);
            Assert.AreEqual(RoboMaster.standard_pose_rz, pose.RZpose);
        }
    }
}
