using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using RoboLibrary.Interfaces;

namespace RoboLibrary.Dummies
{
    public class DummyLogic : ILogic
    {
        private IData data;
        public DummyLogic(IData data)
        {
            this.data = data;
        }
        public void ProcessPath(List<URPose> path)
        {

        }

        public void SetStandardPose()
        {
            data.SendNewPose(new URPose(0,0,0,0,0,0));
        }

        public void PauseScanning()
        {

        }

        public void StopScanning()
        {

        }

        public void PoseReached(int pose, int amount)
        {
            
        }
    }
}
