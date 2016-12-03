using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using System.Threading;

namespace UR10TCPController
{
    public class Pathfeeder
    {
        private static int _max_wait_iterations = 1000; //Iterations to try between checking if target pose is close to actual pose and sending pose again
        public bool RunningPath;
        public bool ShouldRun = true;
        public void RunPath(List<URPose> path)
        {
            RunningPath = true;
            
            URPose actualPose = Data.Instance.LastKnownPose;
            int iterations_done;
            foreach (URPose targetPose in path)
            {
                Data.Instance.SendNewPose(targetPose);
                iterations_done = 0;
                while (!IsCloseEnough(targetPose, actualPose) && ShouldRun) //if the target position and actual position aren't close enough, check again soon
                {
                    //Thread.Sleep(_sleep_time);
                    actualPose = Data.Instance.LastKnownPose;
                    iterations_done++;
                    if(iterations_done >= _max_wait_iterations)
                    {
                        iterations_done = 0;
                        Data.Instance.SendNewPose(targetPose);
                    }
                }
            }
            RunningPath = false;
        }

        private static double _minimum_proximity_position = 0.01d;
        private static double _minimum_proximity_rotation = double.MaxValue;
        public bool IsCloseEnough(URPose target, URPose actual)
        {
            bool x_close = Math.Abs(target.Xpose - actual.Xpose) <= _minimum_proximity_position;
            bool y_close = Math.Abs(target.Ypose - actual.Ypose) <= _minimum_proximity_position;
            bool z_close = Math.Abs(target.Zpose - actual.Zpose) <= _minimum_proximity_position;

            bool rx_close = Math.Abs(target.RXpose - actual.RXpose) <= _minimum_proximity_rotation;
            bool ry_close = Math.Abs(target.RYpose - actual.RYpose) <= _minimum_proximity_rotation;
            bool rz_close = Math.Abs(target.RZpose - actual.RZpose) <= _minimum_proximity_rotation;

            if (x_close && y_close && z_close && rx_close && ry_close && rz_close)
                return true;

            return false;
        }
    }
}
