using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using DataStructures;
using System.Diagnostics.CodeAnalysis;
namespace RoboLibrary.Dummies
{
    [ExcludeFromCodeCoverage]
    public class DummyReader : IReader
    {
        private IData dataInstance;
        public DummyReader(IData data)
        {
            dataInstance = data;
        }
        public void StartListen(string IPAdress, AutoResetEvent are)
        {
            while (true)
            {
                Thread.Sleep(100); //simulate 10Hz 30002 signal
                dataInstance.SetLastKnownPose(DummyUR10.Instance.pose);
            }
        }
    }
}
