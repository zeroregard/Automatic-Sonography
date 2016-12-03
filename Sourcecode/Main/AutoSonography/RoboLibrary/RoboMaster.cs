using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using RoboLibrary.Dummies;
using DataStructures;
using System.Diagnostics.CodeAnalysis;
namespace RoboLibrary
{
    public class RoboMaster : ILogic
    {
        private IData data;
        public IPathFeeder pathFeeder;


        public const double standard_pose_x = -0.160;
        public const double standard_pose_y = -0.268;
        public const double standard_pose_z = 0.500;
        public const double standard_pose_rx = 1.09;
        public const double standard_pose_ry = 1.7;
        public const double standard_pose_rz = -1.9;
        public static EventHandler PathCompletionEvent;
        public float CompletionPercentage;


        [ExcludeFromCodeCoverage] //excluded, this would be an integration NoUR10ConnectionEvent
        public RoboMaster()
        {
            data = new Data();
            data.Init(new Reader(data), new Writer(data, Data.IP_ADDRESS));
            pathFeeder = new PathFeeder(data, this);
        }

        public RoboMaster(IPathFeeder feeder, IData data)
        {
            pathFeeder = feeder;
            this.data = data;
        }

        public void ProcessPath(List<URPose> path)
        {
            SendEntirePose(path[0]);
            pathFeeder.RunPath(path);
        }

        public void PoseReached(int pose, int amountOfPoses)
        {
            CompletionPercentage = ((float)pose/amountOfPoses)*100;
            PathCompletionEvent?.Invoke(this, EventArgs.Empty);
        }

        public void SetStandardPose()
        {
            URPose pose = new URPose(standard_pose_x, standard_pose_y, standard_pose_z, standard_pose_rx, standard_pose_ry, standard_pose_rz);
            SendEntirePose(pose);
        }

        /// <summary>
        /// Pauses the current scanning, freezing UR10 in its current position - or resumes it from where it was paused
        /// </summary>
        public void PauseScanning()
        {
            pathFeeder.PauseRunning();
        }


        /// <summary>
        /// Cancels the current scanning and returns UR10 to its standard position
        /// </summary>
        public void StopScanning()
        {
            pathFeeder.StopRunning();
        }

        private void SendEntirePose(URPose pose)
        {
            data.SendNewPose(pose);
        }
    }
}
