using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
namespace RoboLibrary.Interfaces
{
    public interface ILogic
    {
        void ProcessPath(List<URPose> path);
        void SetStandardPose();
        void PauseScanning();
        void StopScanning();
        void PoseReached(int pose, int amount);
    }
}
