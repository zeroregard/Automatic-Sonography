using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
namespace UR10TCPController.Interfaces
{
    public interface IWriter
    {
        void AddURPose(URPose pose);
        void SendURPose(URPose pose);
        ConfigurationData GetConfigurations();
        void SendConfigurations(ConfigurationData data);
    }
}
