using System;
using System.Net;
using System.Net.Sockets;
using Util;

namespace URPortListener
{
    class Program
    {
        static void Main(string[] args)
        {
            String ipAddress = "10.0.0.1";
            IPAddress HOST = IPAddress.Parse(ipAddress);
            int PORT = 30002;
            int msgNum = 0;

            AnalyseURDataWithConsole a = new AnalyseURDataWithConsole();

            //AnalyseURData a = new AnalyseURData();

            //AnalyseURData.OnRobotMode += AnalyseURData_OnRobotMode;
            //AnalyseURData.OnToolMode += AnalyseURData_OnToolMode;
            //AnalyseURData.OnSafetyMode += AnalyseURData_OnSafetyMode;

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(HOST, PORT);
            }
            catch (Exception e)
            {

            }

            int count = 0;
            while (count < 4)
            {
                byte[] data = new byte[2048];

                s.Receive(data);
                msgNum++;
                Console.WriteLine("Messagenumber: {0} ---------------------------------------------------------", msgNum);
                a.ReadURData(data);
                count++;
            }
        }

        static void AnalyseURData_OnSafetyMode(string safetyMode)
        {
            Console.WriteLine("SafetyMode: " + safetyMode);
        }

        static void AnalyseURData_OnToolMode(string toolMode)
        {
            Console.WriteLine("ToolMode: " + toolMode);
        }

        static void AnalyseURData_OnRobotMode(string robotMode, string controlMode)
        {
            Console.WriteLine("RobotMode: " + robotMode);
            Console.WriteLine("ControlMode: " + controlMode);
        }
    }
}
