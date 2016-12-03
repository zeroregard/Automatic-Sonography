using System;
using System.Threading;
using System.Runtime.InteropServices;
using Util;

namespace DATA
{
    /// <summary>
    /// Class that handles the communication with the GeoMagicTouch (GMT).
    /// This class gets the current device state of the GMT and sets the force.
    /// </summary>
    public class GeoMagicTouch
    {
        // Uses P/Invoke to call unmanaged methods from the DLL, that makes it possible to communicate with the GeoMagicTouch.

        /// <summary>Gets the current device state.</summary>
        [DllImport("GeoMagicTouchDLL.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr getCurrentDeviceState();

        /// <summary>Initiates the connection to the GeoMagicTouch and return 1 if succesfull, otherwise 0.</summary>
        [DllImport("GeoMagicTouchDLL.dll", CharSet = CharSet.Unicode)]
        private static extern int startScheduler();

        /// <summary>Closes the connection to the GeoMagicTouch and return 1 if successful, otherwise 0.</summary>
        [DllImport("GeoMagicTouchDLL.dll", CharSet = CharSet.Unicode)]
        private static extern int stopScheduler();

        /// <summary>Checks the connection to the GeoMagicTouch and return 1 if connection is alive, otherwise 0.</summary>
        [DllImport("GeoMagicTouchDLL.dll", CharSet = CharSet.Unicode)]
        private static extern int isAlive();

        /// <summary>Sets the force in a 3-dimensional space on the GeoMagicTouch.</summary>
        /// <param name="x">Force value in x dircetion in Newtons.</param>
        /// <param name="y">Force value in y dircetion in Newtons.</param>
        /// <param name="z">Force value in z dircetion in Newtons.</param>
        [DllImport("GeoMagicTouchDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setForces(double x, double y, double z);

        /// <summary>GeoMagic pose event. This event is called when a new pose is collected from the GeoMagicTouch.</summary>
        public delegate void GeoMagicPoseEventHandler(GeoMagicPose geoPose);
        /// <summary>GeoMagic pose event. This event is called when a new pose is collected from the GeoMagicTouch.</summary>
        public static event GeoMagicPoseEventHandler OnGeoPose;

        /// <summary>GeoMagic Exception event. This event is called when an exception has occurred.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void GeoMagicExceptionEventHandler(string error, string source);
        /// <summary>GeoMagic Exception event. This event is called when an exception has occurred.</summary>
        public static event GeoMagicExceptionEventHandler OnException;

        /// <summary>Variable to determine when to stop the connection to the GeoMagicTouch.</summary>
        private volatile bool shouldStop = false;
        /// <summary>Variable to show if the connection is active.</summary>
        private static bool isConnected = false;

        /// <summary>Private ForceData that contains the force values to be set on the GeoMagicTouch.</summary>
        ForceData force = new ForceData(0, 0, 0, 0, 0, 0);

        /// <summary>Listener to establish connection to the GeoMagicTouch.
        /// If successful it gets current device state and sets force on the GeoMagicTouch.</summary>
        /// <param name="are">AutoResetEvent that will be set when the connection is successful.</param>
        public void GeoListener(AutoResetEvent are)
        {
            GeoMagicPose gPose;

            if (startScheduler() == 1)                                          // startScheduler() called to connect to GeoMagicTouch.
            {
                isConnected = true;
                are.Set();                                                      // Throws event when connection is successful
                shouldStop = false;

                while (!shouldStop)
                {
                    if (isAlive() == 1)                                         // Every iteration it checks whether the connection is still alive
                    {
                        double[] geoArray = GetCoordinates();
                        gPose = new GeoMagicPose();
                        gPose.Xpose = geoArray[0];
                        gPose.Ypose = geoArray[1];
                        gPose.Zpose = geoArray[2];
                        gPose.Joint1pose = geoArray[3];
                        gPose.Joint2pose = geoArray[4];
                        gPose.Joint3pose = geoArray[5];
                        gPose.RXpose = geoArray[6];
                        gPose.RYpose = geoArray[7];
                        gPose.RZpose = geoArray[8];
                        gPose.BtnState = geoArray[12];

                        if (OnGeoPose != null)
                        {
                            OnGeoPose(gPose);                                   // Event thrown to signal the current position of the GeoMagicTouch
                        }

                        setForces(force.Xforce, force.Zforce, force.Yforce);    // Force set on the GeoMagicTouch based on the values of the local variable

                        Thread.Sleep(10);                                       // Sleep thread to optain a frequency of 100Hz
                    }
                    else
                    {
                        if (OnException != null)
                            OnException("Der er mistet forbindelse til Geomagic Touch.", "GMT");
                        isConnected = false;
                        break;
                    }
                }
            }
            else
            {
                if (OnException != null)
                    OnException("Der kunne ikke oprettes forbindelse til GeoMagic Touch", "GMT");
                isConnected = false;
            }
            if (shouldStop)
            {
                if (stopScheduler() == 1) { }                                   // stopScheduler called to close connection to the GeoMagicTouch
                else
                {
                    if (OnException != null)
                        OnException("GeoMagic Touch kunne ikke stoppes.", "GMT");
                    isConnected = false;
                }
            }
        }

        /// <summary>Gets the double array of the current device state from the unmanaged memory pointer.</summary>
        private static double[] GetCoordinates()
        {
            double[] arr = new double[13];
            Marshal.Copy(getCurrentDeviceState(), arr, 0, 13);
            return arr;
        }

        /// <summary>Sets the force on the GeoMagicTouch.</summary>
        /// <param name="fd">ForceData with the desired force values to be set on the device.</param>
        public void SetForce(ForceData fd)
        {
            force = fd;
        }

        /// <summary>Requests the listener to terminate.</summary>
        public void RequestStop()
        {
            shouldStop = true;
        }

        /// <summary>Shows the state of the connection.</summary>
        public bool IsConnected()
        {
            return isConnected;
        }
    }
}
