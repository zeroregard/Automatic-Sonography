using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;

namespace ComputerVisionLibrary
{
    public class DummyKinect : IKinect, IDisposable
    {
        private ComputerVisionMaster master;
        public DummyKinect(ComputerVisionMaster master)
        {
            this.master = master;
        }

        public void CaptureMeshNow()
        {
            CVMesh m = new CVMesh();
            master.RetrieveMesh(m);
        }

        public void Dispose()
        {
            
        }
    }
}
