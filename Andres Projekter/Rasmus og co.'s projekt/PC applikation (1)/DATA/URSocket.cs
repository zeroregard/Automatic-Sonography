using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DATA
{
    /// <summary>
    /// <para>Socket that connects to an Universal Robot on port 30002.</para>
    /// The socket receives the data send on port 30002 and sends it to be analysed.
    /// Result from this analysation kan be received from the class AnalyseURData.
    /// </summary>
    public class URSocket
    {
        /// <summary>Variable to determine when to stop the connection to the URSocket.</summary>
        private volatile bool shouldStop = false;
        Socket socket;

        /// <summary>URsocket Exception event. This event is called when an exception has occurred.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void URExceptionEventHandler(string error, string source);
        /// <summary>URsocket Exception event. This event is called when an exception has occurred.</summary>
        public static event URExceptionEventHandler OnException;

        /// <summary><para>Socket listener that connects to the Universal Robots server, and receives the data stream send from the robot.</para>
        /// The data received on the stream is analysed by the class AnalyseURData.</summary>
        /// <param name="ipAddress">Ip address to the Universal Robot server on port 30002.</param>
        /// <param name="are">AutoResetEvent that will be set when the connection is successful.</param>
        public void RunSocketListener(string ipAddress, AutoResetEvent are)
        {
            IPAddress HOST = IPAddress.Parse(ipAddress);
            int PORT = 30002;
            
            AnalyseURData analyse = new AnalyseURData();

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(HOST, PORT);
                socket.ReceiveTimeout = 2000;                                           // ReceiveTimeout set to throw an exception when the connection to the robot is lost.
                are.Set();

                while (!shouldStop)
                {
                    byte[] data = new byte[2048];
                    try {
                        if (socket.Connected)
                        {
                            socket.Receive(data);                                       // continuously receives the data stream from the robot
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

                    analyse.ReadURData(data);                                           // Data is sent to be analysed
                }
                try { socket.Shutdown(SocketShutdown.Both); }
                catch (Exception e)
                {
                    if (OnException != null)
                        OnException("Forbindelse til UR10 kan ikke lukkes.", "UR");
                }

                socket.Close();
            }
            catch (Exception e)
            {
                if (OnException != null)
                    OnException("Der kan ikke oprettes forbindelse til UR10.", "UR");
            }
        }

        /// <summary>Requests the listener to terminate.</summary>
        public void RequestSocketStop()
        {
            shouldStop = true;
        }

        /// <summary>Shows the state of the connection.</summary>
        public bool IsConnected()
        {
            return socket.Connected;
        }
    }
}
