using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UR10TCPController.Interfaces;

namespace UR10TCPController.Dummies
{
    public class DummyReader : IReader
    {
        public void StartListen(string IPAdress, AutoResetEvent are)
        {
            while (true)
            {
                Thread.Sleep(100); //simulate 10Hz 30002 signal
                Data.Instance.ReceivePose(DummyUR10.Instance.pose);
            }
        }
    }
}
