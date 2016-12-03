using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using Util;
using DataStructures;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace RoboLibrary.Dummies
{
    [ExcludeFromCodeCoverage]
    public class DummyWriter : IWriter
    {
        private IData dataInstance;
        private double CurrentSpeed;
        private double CurrentAcceleration;
        public DummyWriter(IData data)
        {
            dataInstance = data;
        }
        public void AddURPose(DataStructures.URPose pose)
        {
            DataStructures.URPose currentPose = DummyUR10.Instance.pose;
            currentPose.Xpose += pose.Xpose;
            currentPose.Ypose += pose.Ypose;
            currentPose.Zpose += pose.Zpose;

            currentPose.RXpose += pose.RXpose;
            currentPose.RYpose += pose.RYpose;
            currentPose.RZpose += pose.RZpose;

            DummyUR10.Instance.pose = currentPose;
        }

        public ConfigurationData GetConfigurations()
        {
            ConfigurationData d = new ConfigurationData();
            d.Acceleration = CurrentAcceleration;
            d.Speed = CurrentSpeed;
            dataInstance.ReceiveConfiguration(d);
            return d;
        }

        public void SendConfigurations(ConfigurationData data)
        {
            CurrentAcceleration = data.Acceleration;
            CurrentSpeed = data.Speed;
        }

        public void SendURPose(DataStructures.URPose pose)
        {
            //INSTANT!
            dataInstance.SetLastKnownPose(pose);
        }
    }
}
