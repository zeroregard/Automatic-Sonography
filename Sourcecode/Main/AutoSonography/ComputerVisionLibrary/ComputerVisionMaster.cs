using Microsoft.Kinect.Fusion;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DataStructures;
using Microsoft.Kinect;

namespace ComputerVisionLibrary
{
    public class ComputerVisionMaster
    {
        public IKinect fusionizer;
        public Slicer slicer;
        //private const string meshFileLocation = @"C:/Users/Mathias/Documents/Meshes";
        //public string LastMeshLocation;
        public CVMesh LastMesh;
        public event EventHandler MeshFinished;
        public event EventHandler DepthArrived;
        public DepthFrame CurrentFrame;

        public ComputerVisionMaster()
        {
            slicer = new Slicer();
        }

        public void RequestCurrentImageAsMesh()
        {
            LastMesh = null;
            fusionizer.CaptureMeshNow();
        }

        public void RetrieveMesh(CVMesh m)
        {
            LastMesh = m;
            ((IDisposable)fusionizer).Dispose();
            fusionizer = null;
            MeshFinished?.Invoke(this, EventArgs.Empty);
        }

        [ExcludeFromCodeCoverage]
        public void RetrieveFrame(DepthFrame current)
        {
            CurrentFrame = current;
            DepthArrived?.Invoke(this, EventArgs.Empty);
        }

    }
}
