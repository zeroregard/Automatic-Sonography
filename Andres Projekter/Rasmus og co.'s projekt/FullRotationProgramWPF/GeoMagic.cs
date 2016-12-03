using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Util;

namespace FullRotationProgramWPF
{
    public class GeoMagicTouch
    {
        [DllImport("structDll.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr getCurrentDeviceState();

        [DllImport("structDll.dll", CharSet = CharSet.Unicode)]
        private static extern int startScheduler();

        [DllImport("structDll.dll", CharSet = CharSet.Unicode)]
        private static extern int stopScheduler();

        [DllImport("structDll.dll", CharSet = CharSet.Unicode)]
        private static extern int isAlive();

        [DllImport("structDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setForces(double x, double y, double z);

        public delegate void GeoMagicPoseEventHandler(GeoMagicPose geoPose);
        public static event GeoMagicPoseEventHandler OnGeoPose;

        public delegate void GeoMagicExceptionEventHandler(string error, string source);
        public static event GeoMagicExceptionEventHandler OnException;

        private static bool shouldStop = false;
        private static bool isConnected = false;

        ForceData force = new ForceData(0, 0, 0, 0, 0, 0);
        //private int isSucces;
        public void GeoListener()
        {
            GeoMagicPose gPose;
            //isSucces = startScheduler();

            if (startScheduler() == 1)
            {
                isConnected = true;
                shouldStop = false;

                while (!shouldStop)
                {
                    if (isAlive() == 1)
                    {
                        double[] geoArray = GetCoordinates();
                        gPose = new GeoMagicPose();
                        gPose.Xpose = geoArray[3];
                        gPose.Ypose = geoArray[4];
                        gPose.Zpose = geoArray[5];
                        gPose.RXpose = geoArray[6];
                        gPose.RYpose = geoArray[7];
                        gPose.RZpose = geoArray[8];
                        gPose.BtnState = geoArray[12];

                        if (OnGeoPose != null)
                        {
                            OnGeoPose(gPose);
                        }

                        //setForces(force.Xforce, force.Zforce, force.Yforce);
                        //setForces(0, 0, 1);

                        Thread.Sleep(10);
                    }
                    else
                    {
                        if (OnException != null)
                            OnException("Der er mistet forbindelse til Geomagic Touch.", "GMT");
                        isConnected = false;
                        break;
                    }
                }
                //if (stopScheduler() == 1)
                //{
                //    isConnected = false;
                //}
            }
            else
            {
                if (OnException != null)
                    OnException("Der kunne ikke oprettes forbindelse til GeoMagic Touch", "GMT");
                isConnected = false;
            }
        }

        private static double[] GetCoordinates()
        {
            double[] arr = new double[13];
            Marshal.Copy(getCurrentDeviceState(), arr, 0, 13);
            return arr;
        }

        public void SetForce(ForceData fd)
        {
            force = fd;
        }

        public void RequestStop()
        {
            shouldStop = true;
        }

        public bool IsConnected()
        {
            return isConnected;
        }
    }
}
