using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using RoboLibrary.Interfaces;
using DataStructures;
using System.Diagnostics.CodeAnalysis;
namespace RoboLibrary
{
    public class PathFeeder : IPathFeeder
    {
        private static int _max_wait_iterations = 1000; //Iterations to try between checking if target pose is close to actual pose and sending pose again
        private ManualResetEvent Event = new ManualResetEvent(true);
        private RoboMaster master;
        private Thread pathThread;
        private bool isPaused = false;
        private bool stopRun = false;
        private IData data;
        private ILogic logic;

        public PathFeeder(IData data, ILogic logic)
        {
            this.data = data;
            this.logic = logic;
        }

        public void RunPath(List<URPose> path)
        {
            pathThread = new Thread(x => DoRunPath(path));
            pathThread.Start();
        }

        [ExcludeFromCodeCoverage] //Put this in class diagram still!
        private void DoRunPath(List<URPose> path)
        {
            data.SlowDown();
            stopRun = false;
            URPose actualPose = data.GetLastKnownPose();
            for (int index = 0; index < path.Count; index++)
            {
                URPose targetPose = path[index];
                if (IsValid(targetPose))
                {
                    logic.PoseReached(index, path.Count);
                    data.SendNewPose(targetPose);
                    while (!IsCloseEnough(targetPose, actualPose) && !stopRun)
                    //if the target position and actual position aren't close enough, check again soon
                    {
                        Event.WaitOne();
                        actualPose = data.GetLastKnownPose();
                    }
                    Thread.Sleep(50); //To not skip any steps instantly
                }
                if (stopRun)
                    Escape();
            }
            Escape();
        }

        private bool IsValid(URPose p)
        {
            return (!double.IsNaN(p.Xpose) && !double.IsNaN(p.Ypose) && !double.IsNaN(p.Zpose) && !double.IsNaN(p.RXpose) &&
                    !double.IsNaN(p.RYpose) && !double.IsNaN(p.RZpose));
        }

        public void Escape()
        {
            URPose pose = data.GetLastKnownPose();
            if (pose == null)
                return;
            pose.Zpose += 0.15f;
            data.SendNewPose(pose);
            Thread.Sleep(25);
            logic.SetStandardPose();
            data.SpeedUp();
        }

        [ExcludeFromCodeCoverage] //Tested in dummy class, exact same code
        public void PauseRunning()
        {
            if (!isPaused)
            {
                isPaused = true;
                Event.Reset();
            }
            else
            {
                isPaused = false;
                Event.Set();
            }
        }

        public static double MinimumProximityPosition = 0.06;
        public static double MinimumProximityRotation = 0.2;
        public bool IsCloseEnough(URPose target, URPose actual)
        {
            if (actual == null) //You're not connected to UR10
                return false;
            decimal xDistance = Math.Abs((decimal)target.Xpose - (decimal)actual.Xpose);
            decimal yDistance = Math.Abs((decimal)target.Ypose - (decimal)actual.Ypose);
            decimal zDistance = Math.Abs((decimal)target.Zpose - (decimal)actual.Zpose);
            bool PositionClose =
                (xDistance <= (decimal)MinimumProximityPosition) &&
                (yDistance <= (decimal)MinimumProximityPosition) &&
                (zDistance <= (decimal)MinimumProximityPosition);
            decimal xRotDis = Math.Abs((decimal)target.RXpose - (decimal)actual.RXpose);
            decimal yRotDis = Math.Abs((decimal)target.RYpose - (decimal)actual.RYpose);
            decimal zRotDis = Math.Abs((decimal)target.RZpose - (decimal)actual.RZpose);
            bool RotationClose =
                (xRotDis <= (decimal)MinimumProximityPosition) &&
                (yRotDis <= (decimal)MinimumProximityPosition) &&
                (zRotDis <= (decimal)MinimumProximityPosition);

            return PositionClose || RotationClose;
        }

        public void StopRunning()
        {
            if (pathThread != null)
            {
                Event.Set(); //Resume the thread so it can terminate
                stopRun = true;
            }
            logic.SetStandardPose();
        }
    }
}
