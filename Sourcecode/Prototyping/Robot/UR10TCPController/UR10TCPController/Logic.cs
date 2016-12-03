using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UR10TCPController
{
    public class Logic
    {
        private static Logic _instance;
        private Pathfeeder _pathfeeder;
        private PathCreator _pathcreator;
        private Thread pathThread;

        private Logic()
        {
            _pathfeeder = new Pathfeeder();
            _pathcreator = new PathCreator();
        }

        

        public void SendPath()
        {
            List<URPose> poses = _pathcreator.ReturnCircle();
            if (pathThread == null)
            {
                pathThread = new Thread(() => _pathfeeder.RunPath(poses));
                pathThread.IsBackground = true;
                pathThread.Start();
            }
            else
            {
                _pathfeeder.ShouldRun = false;
                pathThread = new Thread(() => _pathfeeder.RunPath(poses));
                _pathfeeder.ShouldRun = true;
                pathThread.IsBackground = true;
                pathThread.Start();
            }
        }

        public static Logic Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logic();
                }
                return _instance;
            }
        }

        private void Change_TCP_Position(double x, double y, double z)
        {
            URPose p = new URPose(x, y, z, 0, 0, 0);
            Data.Instance.SendAddedPose(p);
        }

        private void Change_TCP_Rotation(double rx, double ry, double rz)
        {
            URPose p = new URPose(0, 0, 0, rx, ry, rz);
            Data.Instance.SendAddedPose(p);
        }

        public void ChangePositionRotation(double x, double y, double z, double rx, double ry, double rz)
        {
            if (x != 0 || y != 0 || z != 0)
                Change_TCP_Position(x, y, z);
            if (rx != 0 || ry != 0 || rz != 0)
                Change_TCP_Rotation(rx, ry, rz);
        }

        public void SendEntirePose(URPose pose)
        {
            Data.Instance.SendNewPose(pose);
        }
    }
}
