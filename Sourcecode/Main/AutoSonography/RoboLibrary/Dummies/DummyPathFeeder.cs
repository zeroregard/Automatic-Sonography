using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using System.Threading;
using DataStructures;
using System.Diagnostics.CodeAnalysis;
namespace RoboLibrary.Dummies
{
    [ExcludeFromCodeCoverage]
    public class DummyPathFeeder : IPathFeeder
    {
        public int SleepTime = 1000;
        public List<URPose> CurrentPath;
        public URPose CurrentPose;
        public bool CompletedPath;
        private ManualResetEvent Event = new ManualResetEvent(true);
        private Thread pathRunningProcess;
        private bool paused;
        public void PauseRunning()
        {
            if (!paused)
            {
                paused = true;
                Event.Reset();
            }
            else
            {
                paused = false;
                Event.Set();
            }
        }

        private void Run()
        {
            foreach (URPose urPose in CurrentPath)
            {
                CurrentPose = urPose;
                Thread.Sleep(SleepTime);
            }
            CompletedPath = true;
        }



        public void RunPath(List<URPose> poses)
        {
            pathRunningProcess?.Join();
            CompletedPath = false;
            CurrentPath = poses;
            pathRunningProcess = new Thread(Run);
            pathRunningProcess.Start();
        }

        public void StopRunning()
        {
            throw new NotImplementedException();
        }
    }
}
