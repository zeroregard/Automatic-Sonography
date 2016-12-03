using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using URPose = DataStructures.URPose;

namespace RoboLibrary.Interfaces
{
    public interface IData
    {
        URPose GetLastKnownPose();
        void SetLastKnownPose(URPose pose);
        void SendNewPose(URPose pose);
        void ReceiveConfiguration(ConfigurationData conf);
        void Init(IReader reader, IWriter writer);
        void SpeedUp();
        void SlowDown();
    }
}
