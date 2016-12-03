using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UR10TCPController
{
    public class PathCreator
    {
        public List<URPose> ReturnTwoTestPoints(URPose currentPose)
        {
            URPose position1 = URPose.Copy(currentPose);
            URPose position2 = URPose.Copy(currentPose);
            position1.Xpose = 0.3d;
            position2.Xpose = 0.1d;
            List<URPose> poses = new List<URPose>();
            poses.Add(position1);
            poses.Add(position2);
            return poses;
        }

        public List<URPose> ReturnCircle()
        {
            List<URPose> poses = new List<URPose>();
            var coords = generate_circle();
            foreach (var coord in coords)
            {
                double x = Math.Round(coord.Xpose, 3);
                double z = Math.Round(coord.Zpose, 3);
                double ry = Math.Round(Math.PI*1.5d - coord.RYpose, 3);
                poses.Add(new URPose(x, -0.8d, z, 0, ry, 0));
            }
            return poses;
        }

        public List<URPose> generate_circle()
        {
            double x, z;
            double xOffset, zOffset;
            zOffset = 0.45;
            double length = 0.35d;
            double angle = 0.0;
            double angle_stepsize = 0.01d;
            List<URPose> coordinates = new List<URPose>();

            // go through all angles from 0 to 2 * PI radians
            while (angle < 2 * Math.PI)
            {
                x = length * Math.Cos(angle);
                z = length * Math.Sin(angle) + zOffset;

                angle += angle_stepsize;
                coordinates.Add(new URPose(x, 0, z, 0, angle, 0));
                //Debug.WriteLine("X: " + x + " Z: " + z);
            }
            return coordinates;
        }
    }
}
