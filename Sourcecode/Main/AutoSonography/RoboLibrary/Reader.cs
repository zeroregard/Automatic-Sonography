using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RoboLibrary.Interfaces;

namespace RoboLibrary
{
    [ExcludeFromCodeCoverage]
    public class Reader : IReader
    {
        private static int port = 30002;
        private static int _socket_timeout = 2000;
        public bool IsConnected = false;
        public bool ShouldStop = false;
        private Socket socket;
        private IAnalyzer analyzer;

        public Reader(IData d)
        {
            analyzer = new Analyzer(d);
        }

        public void StartListen(string ip_address, AutoResetEvent are)
        {
            OpenSocket(ip_address, are);
            Listen();

        }

        private void OpenSocket(string ip_address, AutoResetEvent are)
        {
            try
            {
                IPAddress host = IPAddress.Parse(ip_address);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(host, port);
                socket.ReceiveTimeout = _socket_timeout;
                are.Set();
                IsConnected = socket.Connected;
            }
            catch(Exception exc)
            {
                Debug.WriteLine("Could establish socket connectio to UR10: " + exc.Message);
            }
        }

        private void Listen()
        {
            while (!ShouldStop)
            {
                byte[] data = new byte[2048];
                try
                {
                    if (IsConnected)
                        socket.Receive(data);
                    else
                        break; //We're not connected anymore, shut down socket.
                }
                catch (Exception exc)
                {
                    Debug.WriteLine("Socket connection to UR10 lost: " + exc.Message);
                    break;
                }
                analyzer.ReadURData(data);
            }
        }

        private void CloseSocket()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                IsConnected = false;
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Could not close socket connection properly to UR10: " + exc.Message);
            }
        }

        public void RequestSocketShutdown()
        {
            ShouldStop = true;
        }
    }

}
