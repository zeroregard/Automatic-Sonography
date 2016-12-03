using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.Kinect.Fusion;
namespace CalculationLibrary
{
    public class PathCreator
    {
        public CameraToRobotCalibrator calibrator;
        [ExcludeFromCodeCoverage] //only used for calibration
        public List<URPose> GenerateBoxPath(CVMesh m)
        {
            var verticePoints = m.Vertices;
            var box = Extensions.FindBoundingBox(verticePoints);
            List<Vector3> upperBox = new List<Vector3>();
            upperBox.Add(new Vector3() { X = box.x_max, Y = box.y_max, Z = box.z_max });
            upperBox.Add(new Vector3() { X = box.x_min, Y = box.y_max, Z = box.z_max });
            upperBox.Add(new Vector3() { X = box.x_min, Y = box.y_min, Z = box.z_max });
            upperBox.Add(new Vector3() { X = box.x_max, Y = box.y_min, Z = box.z_max });
            List<URPose> poses = new List<URPose>();
            foreach (var p in upperBox)
            {
                float X = p.X;
                float Y = p.Y;
                float Z = p.Z;
                Z += 0.20f;
                double RXpose = Math.PI;
                double RYpose = 0;
                double RZpose = 0;

                poses.Add(new URPose(X, Y, Z, RXpose, RYpose, RZpose));
            }
            return poses;
        }

        public List<VertexIndex> CreatePath(List<Vector3> vertices, float x_absolute_min = float.MaxValue, float x_absolute_max = float.MinValue)
        {
            BoundingBox b = Extensions.FindBoundingBox(vertices, x_absolute_min, x_absolute_max);
            List<Vector3> wave = GenerateSquareWave(b);
            PLYHandler.Output(wave, Environment.CurrentDirectory, "WAVE");
            return IdentifyPath(wave, vertices);
        }

        private List<VertexIndex> IdentifyPath(List<Vector3> wave, List<Vector3> vertices)
        {
            List<VertexIndex> path = new List<VertexIndex>();
            foreach (var w in wave)
            {

                float minDis = float.MaxValue;
                Vector3 bestCandidate = new Vector3 { X = float.NaN };
                Vector3 endPoint = w;
                endPoint.Z += -1;
                int i = 0;
                for (int index = 0; index < vertices.Count; index++)
                {
                    var v = vertices[index];
                    var distanceToLine = DistanceToLine(w, endPoint, v);
                    if (distanceToLine < minDis)
                    {
                        minDis = distanceToLine;
                        bestCandidate = v;
                        i = index;
                    }
                }
                if (float.IsNaN(bestCandidate.X))
                    throw new WarningException("No candidate was found");
                path.Add(new VertexIndex(bestCandidate, i));
            }
            return path;
        }

        public float distance_length = -0.02f;
        public float distance_width = -0.02f;
        private List<Vector3> GenerateSquareWave(BoundingBox b)
        {
            List<Vector3> wave = new List<Vector3>();
            Vector3 lastPoint = new Vector3 { X = b.x_max, Y = b.y_max, Z = 0.5f };
            wave.Add(lastPoint);
            while (true)
            {
                if (OutOfBounds(b.x_min, b.x_max, lastPoint.X))
                {
                    distance_length *= -1;
                    lastPoint.Y += distance_width;
                }
                if (lastPoint.Y < b.y_min)
                    break;
                lastPoint.X += distance_length;
                if (!OutOfBounds(b.x_min, b.x_max, lastPoint.X))
                {
                    Vector3 copyPoint = lastPoint;
                    wave.Add(copyPoint);
                }
            }
            return wave;
        }

        private bool OutOfBounds(float xMin, float xMax, float X)
        {
            return (X < xMin || X > xMax);
        }

        [ExcludeFromCodeCoverage] //For future code
        private List<VertexIndex> IdentifyScanPath(List<Vector3> vertices, BoundingBox b, float waveLength = 0.5f, float step = 0.01f)
        {
            var wave = GenerateScanWave(b, waveLength, step);
            List<VertexIndex> path = new List<VertexIndex>();
            foreach (var w in wave)
            {

                float minDis = float.MaxValue;
                Vector3 bestCandidate = new Vector3 { X = float.NaN }; //cannot set Vector3 to null, so
                Vector3 endPoint = w;
                endPoint.Z += -1;
                int i = 0;
                for (int index = 0; index < vertices.Count; index++)
                {
                    var v = vertices[index];
                    var distanceToLine = DistanceToLine(w, endPoint, v);
                    if (distanceToLine < minDis)
                    {
                        minDis = distanceToLine;
                        bestCandidate = v;
                        i = index;
                    }
                }
                if (float.IsNaN(bestCandidate.X))
                    throw new WarningException("No candidate was found");
                path.Add(new VertexIndex(bestCandidate, i));
            }
            return path;
        }

        [ExcludeFromCodeCoverage] //For future code
        private List<Vector3> GenerateScanWave(BoundingBox b, float waveLength = 0.5f, float step = 0.01f)
        {
            List<Vector3> samples = new List<Vector3>();
            float frequency = (float)(2 * Math.PI / waveLength);
            int points = (int)((b.x_max - b.x_min) / step);
            float amplitude = (b.y_max - b.y_min) / 2f;
            float offset = (b.y_max + b.y_min) / 2f;
            float x = b.x_min;
            float z = 1f;
            for (int i = 0; i < points; i++)
            {
                x += step;
                float y = (float)(amplitude * Math.Sin(frequency * x) + offset);
                samples.Add(new Vector3 { X = x, Y = y, Z = z });
            }
            return samples;
        }

        //http://stackoverflow.com/questions/19878441/point-line-distance-calculation
        public float DistanceToLine(Vector3 lineStart, Vector3 lineFinish, Vector3 point)
        {
            //(|(x_2-x_1)x(x_1-x_0)|)/(|x_2-x_1|)
            var sub1 = Extensions.Subtract(lineFinish, lineStart);
            var sub2 = Extensions.Subtract(lineStart, point);
            var product = Extensions.CrossProduct(sub1, sub2);
            var productLength = Extensions.Magnitude(product);

            var lineDistance = Extensions.Magnitude(sub1);
            return productLength / lineDistance;
        }
    }
}
