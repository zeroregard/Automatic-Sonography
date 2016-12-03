using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UR10TCPController.Interfaces
{
    public interface IReader
    {
        void StartListen(string IPAdress, AutoResetEvent are);
    }
}
