using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
namespace RoboLibrary.Interfaces
{
    public interface IPathFeeder
    {
        void RunPath(List<URPose> poses);
        void PauseRunning();
        void StopRunning();
    }
}
