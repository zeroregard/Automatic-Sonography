using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using System.Threading;

namespace FullRotationProgramWPF
{
    public class Logic
    {
        Thread socketThread, geoThread;

        ModBus m;
        GeoMagicTouch gmt;

        GeoMagicPose actualGeoPose, newCenterGeoPose;
        URPose actualURPose, newCenterURPose;

        private static bool isMoving = false;

        public Logic()
        {
            m = new ModBus("10.0.0.1");
            Misc.OnActualPose += new Misc.ActualURPoseEventHandler(setURPose);
            //Misc.OnActualJoint += new Misc.ActualURJointEventHandler(setURPose);
            GeoMagicTouch.OnGeoPose += new GeoMagicTouch.GeoMagicPoseEventHandler(setGeoPose);

            socketThread = new Thread(new ThreadStart(DATA.SocketListener.RunSocketListener));
            socketThread.IsBackground = true;
            socketThread.SetApartmentState(ApartmentState.STA);
            socketThread.Start();

            gmt = new GeoMagicTouch();
            geoThread = new Thread(() => gmt.GeoListener());
            geoThread.IsBackground = true;
            geoThread.SetApartmentState(ApartmentState.STA);
            geoThread.Start();
        }

        private void setGeoPose(GeoMagicPose geoPose)
        {
            actualGeoPose = geoPose;
            //updateGeoPoseLabelsEvent(geoPose);

            if (geoPose.BtnState == 3) btnPressed();
            else btnUnPressed();

            if (isMoving)
            {
                URPose pose = calculatePose();

                m.SendPose(pose);
            }
        }

        private URPose calculatePose()
        {
            double x = newCenterURPose.Xpose + ((actualGeoPose.Xpose + newCenterGeoPose.Xpose * -1) * -1 / 1000);           // Calculates each direction of the position based on the registered center position of the robot and GeoMagicTouch when movement is initiated.
            double y = newCenterURPose.Ypose + ((actualGeoPose.Zpose + newCenterGeoPose.Zpose * -1) / 1000);                // Changes in direction of the GeoMagicTouch is calculated by taking the actual position and adding the invers of the center position.
            double z = newCenterURPose.Zpose + ((actualGeoPose.Ypose + newCenterGeoPose.Ypose * -1) / 1000);                // The x-direction is inversed to inverse the movement from the GeoMagicTouch to the robot based on observation angle of the robot.

            //double Rx = actualGeoPose.RXpose;                                                                               // Direct transfor of the rotation angles from the GeoMagicTouch
            //double Ry = actualGeoPose.RYpose;                                                                               // These values are not used in this version
            //double Rz = actualGeoPose.RZpose;

            double a11 = Math.Cos(actualGeoPose.RXpose * -1) * Math.Cos(actualGeoPose.RYpose);
            double a22 = Math.Cos(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RYpose) - Math.Sin(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Sin(actualGeoPose.RYpose);
            double a33 = Math.Cos(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RXpose * -1);

            double angle = Math.Acos(0.5 * (a11 + a22 + a33 - 1));

            double a32 = Math.Sin(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RYpose) + Math.Cos(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Sin(actualGeoPose.RYpose);
            double a23 = -Math.Sin(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RXpose * -1);
            double e1 = ((a32 - a23) / (2 * Math.Sin(angle))) * angle;

            double a13 = Math.Sin(actualGeoPose.RXpose * -1);
            double a31 = Math.Sin(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RYpose) - Math.Cos(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Cos(actualGeoPose.RYpose);
            double e2 = ((a13 - a31) / (2 * Math.Sin(angle))) * angle;

            double a21 = Math.Cos(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RYpose) + Math.Sin(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Cos(actualGeoPose.RYpose);
            double a12 = -(Math.Cos(actualGeoPose.RXpose * -1) * Math.Sin(actualGeoPose.RYpose));
            double e3 = ((a21 - a12) / (2 * Math.Sin(angle))) * angle;

            URPose p = new URPose(x, y, z, e1, e2 + 3.14, e3);                                                                     // Creates a position object based on the different calculated directions. 
            return p;
        }

        private void setURPose(URPose a)
        {
            actualURPose = a;
        }

        public void btnPressed()
        {
            if (!isMoving)
            {
                newCenterURPose = actualURPose;
                newCenterGeoPose = actualGeoPose;
                isMoving = true;
            }
        }

        public void btnUnPressed()
        {
            isMoving = false;
        }

    }
}
