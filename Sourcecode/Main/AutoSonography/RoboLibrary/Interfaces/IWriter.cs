using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using DataStructures;
namespace RoboLibrary.Interfaces
{
    public interface IWriter
    {
        void AddURPose(DataStructures.URPose pose);
        void SendURPose(DataStructures.URPose pose);
        ConfigurationData GetConfigurations();
        void SendConfigurations(ConfigurationData data);
    }
}
