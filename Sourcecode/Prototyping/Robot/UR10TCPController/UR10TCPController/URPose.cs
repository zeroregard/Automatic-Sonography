using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UR10TCPController
{
    public class URPose
    {
        public URPose()
        {
            Xpose = 0;
            Ypose = 0;
            Zpose = 0;
            RXpose = 0;
            RYpose = 0;
            RZpose = 0;
        }

        public URPose(double x, double y, double z, double Rx, double Ry, double Rz)
        {
            Xpose = x;
            Ypose = y;
            Zpose = z;
            RXpose = Rx;
            RYpose = Ry;
            RZpose = Rz;
        }

        public static URPose Copy(URPose from)
        {

            URPose to = new URPose();
            to.Xpose = from.Xpose;
            to.Ypose = from.Ypose;
            to.Zpose = from.Zpose;

            to.RXpose = from.RXpose;
            to.RYpose = from.RYpose;
            to.RZpose = from.RZpose;
            return to;
        }

        public double Xpose;
        public double Ypose;
        public double Zpose;
        public double RXpose;
        public double RYpose;
        public double RZpose;
    }
}
