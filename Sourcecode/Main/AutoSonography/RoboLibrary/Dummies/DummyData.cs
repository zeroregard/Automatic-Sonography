using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Util;
using URPose = DataStructures.URPose;

namespace RoboLibrary.Dummies
{
    [ExcludeFromCodeCoverage]
    public class DummyData : IData
    {
        private URPose lastKnownPose;
        public URPose GetLastKnownPose()
        {
            return lastKnownPose;
        }

        public void SetLastKnownPose(URPose pose)
        {
            lastKnownPose = pose;
        }

        public void SendNewPose(URPose pose)
        {
            SetLastKnownPose(pose);
        }

        public void ReceiveConfiguration(ConfigurationData conf)
        {
           
        }

        public void Init(IReader reader, IWriter writer)
        {
            
        }

        public void SpeedUp()
        {
            
        }

        public void SlowDown()
        {
            
        }
    }
}
