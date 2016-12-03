using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UR10TCPController.Interfaces;

namespace UR10TCPController
{
    public class Reader : IReader
    {
        private static int _port = 30002;
        private static int _socket_timeout = 2000;
        public bool IsConnected = false;
        public bool ShouldStop = false;
        private Socket _socket;
        private IAnalyzer _analyzer = new Analyzer();

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
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(host, _port);
                _socket.ReceiveTimeout = _socket_timeout;
                are.Set();
                IsConnected = _socket.Connected;
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
                        _socket.Receive(data);
                    else
                        break; //We're not connected anymore, shut down socket.
                }
                catch (Exception exc)
                {
                    Debug.WriteLine("Socket connection to UR10 lost: " + exc.Message);
                    break;
                }
                _analyzer.ReadURData(data);
            }
        }

        private void CloseSocket()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
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
