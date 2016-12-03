using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UR10TCPController.Interfaces
{
    public interface IAnalyzer
    {
        void ReadURData(byte[] data);
    }
}
