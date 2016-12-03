using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.Kinect.Fusion;

namespace CalculationLibrary
{
    public class CalculationMaster
    {
        public CameraToRobotCalibrator calibrator;
        private Smoother smoother;
        private PathCreator creator;
        private RobotPoseCreator poser;

        public CalculationMaster()
        {
            smoother = new Smoother();
            creator = new PathCreator();
            calibrator = new CameraToRobotCalibrator();
            creator.calibrator = calibrator;
            poser = new RobotPoseCreator();
        }

        /// <summary>
        /// Figures the list of poses the Robot arm should run through to cover a surface-type mesh.
        /// The smoothing is applied to correct extreme normals on the mesh.
        /// </summary>
        /// <param name="cameraMesh">A mesh output from a 3D camera, not transformed into robot space yet</param>
        /// <param name="smoothing">The amount of laplacian smoothing that should be applied to the mesh.</param>
        /// <returns>A list of poses necesarry to perform an automatic ultrasound scan</returns>
        public List<URPose> FindPath(CVMesh cameraMesh, int smoothing = 4)
        {
            CVMesh roboMesh = calibrator.ConvertToRobospace(cameraMesh);
            Vector3[] vertices = roboMesh.Vertices.ToArray();
            for (int i = 0; i < smoothing; i++)
            {
                vertices = smoother.LaplacianFilter(vertices, roboMesh.TriangleIndeces.ToArray());
            }
            roboMesh.Vertices = vertices.ToList();
            float min = calibrator.TransformVertex(new Vector3 { X = -0.25f, Y = 0, Z = 0 }, calibrator.TransformMatrix).Y;
            float max = calibrator.TransformVertex(new Vector3 { X = 0.25f, Y = 0, Z = 0 }, calibrator.TransformMatrix).Y;
            List<VertexIndex> path = creator.CreatePath(roboMesh.Vertices, min, max);
            List<URPose> poses = new List<URPose>();
            path = Extensions.PruneVertices(path);
            List<VertexIndex> prunedAgain = new List<VertexIndex>();
            foreach (var v in path)
            {
                if (v.Vector.X != 0 && v.Vector.Y != 0 && v.Vector.Z != 0)
                    prunedAgain.Add(v);
            }
            poses = poser.ToURPath(path, roboMesh);
            return poses;
        }


        [ExcludeFromCodeCoverage]
        public List<URPose> DEBUG_FINDBOXPATH(CVMesh cameraMesh)
        {
            CVMesh roboMesh = calibrator.ConvertToRobospace(cameraMesh);
            List<URPose> path = creator.GenerateBoxPath(roboMesh);
            return path;
        }
    }
}
