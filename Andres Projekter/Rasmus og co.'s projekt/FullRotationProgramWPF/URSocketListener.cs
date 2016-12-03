using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Util;

namespace FullRotationProgramWPF
{
    public class URSocketListener
    {
        private bool shouldStop = false;
        //private bool isConnected = false;
        Socket socket;

        public delegate void URExceptionEventHandler(string error, string source);
        public static event URExceptionEventHandler OnException;

        public void RunSocketListener(string ipAddress, AutoResetEvent are)
        {
            IPAddress HOST = IPAddress.Parse(ipAddress);
            int PORT = 30002;
            
            AnalyseURData analyse = new AnalyseURData();

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(HOST, PORT);
                socket.ReceiveTimeout = 2000;
                are.Set();
                //isConnected = socket.Connected;

                while (!shouldStop)
                {
                    byte[] data = new byte[2048];
                    try {
                        if (socket.Connected)
                        {
                            socket.Receive(data);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        if (OnException != null)
                            OnException("Der er mistet forbindelse til UR10.", "UR");
                        break;
                    }
                    analyse.ReadURData(data);
                }
                try { socket.Shutdown(SocketShutdown.Both); }
                catch (Exception e)
                {
                    if (OnException != null)
                        OnException("Forbindelse til UR10 kan ikke lukkes.", "UR");
                }

                socket.Close();
                //isConnected = false;
            }
            catch (Exception e)
            {
                if (OnException != null)
                    OnException("Der kan ikke oprettes forbindelse til UR10.", "UR");
            }
        }

        public void RequestSocketStop()
        {
            shouldStop = true;
        }

        public bool IsConnected()
        {
            //return isConnected;
            return socket.Connected;
        }
    }
}
