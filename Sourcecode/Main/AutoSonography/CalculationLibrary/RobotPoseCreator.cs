using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.Kinect.Fusion;

namespace CalculationLibrary
{
    public class RobotPoseCreator
    {
        public static float UR_PROBE_OFFSET = 0.10f;
        public List<URPose> ToURPath(List<VertexIndex> vectorPath, CVMesh mesh)
        {
            List<URPose> poses = new List<URPose>();
            foreach (var v in vectorPath)
            {
                Vector3 vertexNormal = FindVertexNormal(v, mesh);
                //vertexNormal.X = 0.00001f;
                vertexNormal = Extensions.Normalize(vertexNormal);
                Vector3 urPos = FindURPosition(v.Vector, vertexNormal);
                //Vector3 smoothedNormal = Extensions.Normalize(Extensions.AvgVector(new List<Vector3> {vertexNormal, new Vector3 {X=0, Y=0, Z=1}}));
                Vector3 rotationNormal = Extensions.Multiply(vertexNormal, -1);
                //Vector3 urRot = ToRotationVector(rotationNormal);

                URPose pose = new URPose(urPos.X, urPos.Y, urPos.Z, rotationNormal.X, rotationNormal.Y, rotationNormal.Z);
                poses.Add(pose);
            }
            int percent = (int)Math.Floor(poses.Count * 0.12);
            var snippet = poses.GetRange(percent, poses.Count - percent * 2);
            PLYHandler.Output(snippet, Environment.CurrentDirectory);
            ConvertDirectionVectors(snippet);
            return snippet;
        }

        private void ConvertDirectionVectors(List<URPose> poses)
        {
            foreach (var urPose in poses)
            {
                Vector3 dir = new Vector3
                {
                    X = (float)urPose.RXpose,
                    Y = (float)urPose.RYpose,
                    Z = (float)urPose.RZpose
                };
                Vector3 rpy = ToRollPitchYaw(dir);
                Vector3 rot = ToRotVector(rpy);
                urPose.RXpose = rot.X;
                urPose.RYpose = rot.Y;
                urPose.RZpose = rot.Z;
            }
        }

        /// <summary>
        /// Gets the roll pitch and yaw from a direction vector. 0,0,0 is up.
        /// </summary>
        /// <param name="d">Direction vector. Must have a length of 1</param>
        public Vector3 ToRollPitchYaw(Vector3 d)
        {
            d = Extensions.Normalize(d);
            double roll, pitch = 0, yaw = 0;
            double pi = Math.PI;
            roll = Math.Acos(d.Z);
            if (d.X == 0 && d.Y == 0)
                yaw = 0;
            else
            {
                Vector3 noZ = new Vector3 { X = d.X, Y = d.Y, Z = 0 };
                noZ = Extensions.Normalize(noZ);
                if (d.X <= 0)
                    yaw = -Math.Acos(-noZ.Y);
                else
                    yaw = Math.Acos(-noZ.Y);
            }
            Vector3 rpy = new Vector3 { X = (float)roll, Y = (float)pitch, Z = (float)yaw };
            return rpy;
        }

        /// <summary>
        /// Converts a roll pitch yaw vector to a rotation vector.
        /// </summary>
        /// <param name="d">A vector where X=Roll, Y=Pitch, Z=Yaw</param>
        /// <returns>A rotation vector with rx, ry and rz used to rotate the TCP of UR10</returns>
        public Vector3 ToRotVector(Vector3 rpy)
        {
            float roll = rpy.X;
            float pitch = rpy.Y;
            float yaw = rpy.Z;
            if (roll == 0 && pitch == 0 && yaw == 0)
                return new Vector3 { X = 0, Y = 0, Z = 0 };
            Matrix3 RollM = new Matrix3();
            RollM.M00 = 1; RollM.M01 = 0; RollM.M02 = 0;
            RollM.M10 = 0; RollM.M11 = Math.Cos(roll); RollM.M12 = -Math.Sin(roll);
            RollM.M20 = 0; RollM.M21 = Math.Sin(roll); RollM.M22 = Math.Cos(roll);

            Matrix3 PitchM = new Matrix3();
            PitchM.M00 = Math.Cos(pitch); PitchM.M01 = 0; PitchM.M02 = Math.Sin(pitch);
            PitchM.M10 = 0; PitchM.M11 = 1; PitchM.M12 = 0;
            PitchM.M20 = -Math.Sin(pitch); PitchM.M21 = 0; PitchM.M22 = Math.Cos(pitch);

            Matrix3 YawM = new Matrix3();
            YawM.M00 = Math.Cos(yaw); YawM.M01 = -Math.Sin(yaw); YawM.M02 = 0;
            YawM.M10 = Math.Sin(yaw); YawM.M11 = Math.Cos(yaw); YawM.M12 = 0;
            YawM.M20 = 0; YawM.M21 = 0; YawM.M22 = 1;

            Matrix3 Rot = new Matrix3();

            //rot = yaw * pitch * roll
            Rot = Matrix3.Dot(YawM, Matrix3.Dot(PitchM, RollM));

            double rotSum = Rot.M00 + Rot.M11 + Rot.M22 - 1;
            double alpha = Math.Acos(rotSum / 2);
            double theta = 0;
            //if (roll >= 0)
            theta = alpha;
            //else
            //theta = 2 * Math.PI - alpha;
            double my = 1d / (2 * Math.Sin(theta));

            double rx = my * (Rot.M21 - Rot.M12) * theta;
            double ry = my * (Rot.M02 - Rot.M20) * theta;
            double rz = my * (Rot.M10 - Rot.M01) * theta;

            Vector3 rotationVector = new Vector3();
            rotationVector.X = (float)rx;
            rotationVector.Y = (float)ry;
            rotationVector.Z = (float)rz;
            return rotationVector;
        }

        public Vector3 FindURPosition(Vector3 vertexPosition, Vector3 vertexNormal)
        {
            Vector3 pos = vertexPosition;
            Vector3 offset = Extensions.Multiply(vertexNormal, UR_PROBE_OFFSET);
            Vector3 offsetPos = Extensions.Add(pos, offset);
            return offsetPos;
        }

        public Vector3 FindVertexNormal(VertexIndex vertex, CVMesh mesh)
        {
            mesh.Faces = Extensions.ToFaces(mesh.TriangleIndeces);
            int index = vertex.Index;
            List<Vector3> faceNormals = new List<Vector3>();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Face f = mesh.Faces[i];
                if (f.index1 == index || f.index2 == index || f.index3 == index)
                {
                    faceNormals.Add(GetFaceNormal(i, mesh));
                }
            }
            if (faceNormals.Count == 0)
                return new Vector3 { X = 0, Y = 0, Z = 1 };
            return Extensions.Normalize(Extensions.AvgVector(faceNormals));
        }

        public Vector3 GetFaceNormal(int i, CVMesh m)
        {
            Vector3 v1 = m.Vertices[m.Faces[i].index1];
            Vector3 v2 = m.Vertices[m.Faces[i].index2];
            Vector3 v3 = m.Vertices[m.Faces[i].index3];
            Vector3 normal = Extensions.FaceNormal(v1, v2, v3);
            normal.Z = normal.Z >= 0 ? normal.Z : 0;
            normal = Extensions.Normalize(normal);
            return normal;
        }


    }
}
