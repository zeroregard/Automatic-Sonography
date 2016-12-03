using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UR10TCPController.Interfaces;
using Util;

namespace UR10TCPController.Dummies
{
    public class DummyWriter : IWriter
    {
        public void AddURPose(URPose pose)
        {
            URPose currentPose = DummyUR10.Instance.pose;
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
            throw new NotImplementedException();
        }

        public void SendConfigurations(ConfigurationData data)
        {
            throw new NotImplementedException();
        }

        public void SendURPose(URPose pose)
        {
            throw new NotImplementedException();
        }
    }
}
