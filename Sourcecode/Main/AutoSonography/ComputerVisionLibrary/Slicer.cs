using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.Kinect.Fusion;
namespace ComputerVisionLibrary
{
    public class Slicer
    {
        //Assumes the following pipeline:
        // 1: ColorMesh outputted from Kinect API
        // 2: ColorMesh turned into CVMesh by KinectFusionizer
        // 3: No further edits have been made
        public CVMesh Slice(CVMesh meshIn, double y_min, double y_max, bool inverted = true)
        {
            if (y_min >= y_max)
                throw new Exception("Minimum value must be lower than maximum value");
            if (meshIn == null)
                throw new Exception("Mesh is null");
            if (meshIn.Vertices.Count == 0)
                throw new Exception("Mesh has no vertices");
            CVMesh meshOut = CVMesh.Clone(meshIn);
            //the depth image is flipped in comparison to output image
            if (inverted)
            {
                double yMinTemp = y_min;
                y_min = y_max;
                y_max = yMinTemp;
                y_min *= -1;
                y_max *= -1;
            }
            yMin = y_min;
            yMax = y_max;
            //only add the same vector if it is unique. the vector itself is the key.
            //the value is the original index the vector had
            meshOut.Faces = Extensions.ToFaces(meshOut.TriangleIndeces);
            meshOut = RemoveNonSurfaceFaces(meshOut);
            Dictionary<Vector3, int> uniques = new Dictionary<Vector3, int>();
            Dictionary<int, int> oldToNewIndices = new Dictionary<int, int>();
            //We can only delete something once, but multiple deleted vertices can
            //refer to one unique vertice, so:
            //Key = deleted, value = unique vertice
            Dictionary<int, int> deleted = new Dictionary<int, int>();
            SplitVertices(ref uniques, ref deleted, ref oldToNewIndices, meshOut.Vertices, meshOut);
            meshOut.Faces = AssignFacesNewIndices(deleted, oldToNewIndices, meshOut.Faces);
            meshOut.Vertices = new List<Vector3>();
            foreach (var unique in uniques)
            {
                meshOut.Vertices.Add(unique.Key);
            }
            meshOut.TriangleIndeces = Extensions.ToTriangleIndices(meshOut.Faces);
            string location = Environment.CurrentDirectory;
            PLYHandler.Output(meshOut, ref location, false);
            return meshOut;
        }

        //A 3D camera mounted to a ceiling should only be able to detect faces pointing up
        
        public CVMesh RemoveNonSurfaceFaces(CVMesh mesh)
        {
            List<Face> toSave = new List<Face>();
            foreach (var meshFace in mesh.Faces)
            {
                Vector3 v1 = mesh.Vertices[meshFace.index1];
                Vector3 v2 = mesh.Vertices[meshFace.index2];
                Vector3 v3 = mesh.Vertices[meshFace.index3];
                Vector3 normal = Extensions.FaceNormal(v1, v2, v3);
                if(normal.Z <= -0.5) //Note that the mesh output from a 3D camera is 'rotated' 180 degrees, that's why this is < 0 instead of > 0 
                    toSave.Add(meshFace);
            }
            mesh.Faces = toSave;
            return mesh;
        }
        /// <summary>
        /// Splits vertices into two dictionaries, the unique ones, and the ones that are not unique and should be deleted 
        /// </summary>
        /// <param name="uniques">Unique vertices</param>
        /// <param name="deleted">Vertices marked for deletion</param>
        private void SplitVertices(ref Dictionary<Vector3, int> uniques, ref Dictionary<int, int> deleted, ref Dictionary<int, int> oldToNewIndices, List<Vector3> vertices, CVMesh mesh)
        {
            //Given vertices 01234567:
            //01, 24, 67, are the same
            //That will give us the uniques 0,2,3,5,6, but in the end they need to have the index 0,1,2,3,4, so we need a uniqueCounter
            int uniqueCounter = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                if (!uniques.ContainsKey(v) && WithInBounds(v) && BelongsToAFace(i, mesh.Faces))
                {
                    uniques.Add(v, uniqueCounter);
                    oldToNewIndices.Add(i, uniqueCounter);
                    uniqueCounter++;
                }
                else
                {
                    int uniqueIndex = -1; //face needs to be deleted if this is -1 still
                    if (WithInBounds(v))
                        uniques.TryGetValue(v, out uniqueIndex);
                    deleted.Add(i, uniqueIndex);
                }
            }
        }

        private bool BelongsToAFace(int index, List<Face> faces)
        {
            foreach (var face in faces)
            {
                if (face.index1 == index || face.index2 == index || face.index3 == index)
                    return true;
            }
            return false;
        }

        public static double xMin = -0.4;
        public static double xMax = 0.25;
        public double yMin = -0.3;
        public double yMax = 0.3;
        private bool WithInBounds(Vector3 v)
        {
            return (v.Y >= yMin && v.Y <= yMax && v.X >= xMin && v.X <= xMax);
        }

        /// <summary>
        /// For every deleted vertice, assign the index of the unique vertice instead
        /// </summary>
        /// <param name="deleted">Vertices that have been deleted</param>
        /// <param name="Faces">Faces of a mesh</param>
        /// <returns></returns>
        private List<Face> AssignFacesNewIndices(Dictionary<int, int> deleted, Dictionary<int, int> oldToNewIndices, List<Face> Faces)
        {
            //Assuming every 3 triangle indices have been converted to a Face
            //Was the original index deleted? Get the 'new' unique index instead. Otherwise assign the old value
            List<Face> prunedFaces = new List<Face>();
            foreach (var face in Faces)
            {
                int originalIndex1 = face.index1;
                int originalIndex2 = face.index2;
                int originalIndex3 = face.index3;
                int index1, index2, index3;
                bool deleted1 = deleted.TryGetValue(originalIndex1, out index1);
                bool deleted2 = deleted.TryGetValue(originalIndex2, out index2);
                bool deleted3 = deleted.TryGetValue(originalIndex3, out index3);
                if (!deleted1) oldToNewIndices.TryGetValue(originalIndex1, out index1);
                if (!deleted2) oldToNewIndices.TryGetValue(originalIndex2, out index2);
                if (!deleted3) oldToNewIndices.TryGetValue(originalIndex3, out index3);
                face.index1 = index1;
                face.index2 = index2;
                face.index3 = index3;

                if (face.index1 != -1 && face.index2 != -1 && face.index3 != -1)
                {
                    prunedFaces.Add(face);
                }
            }
            return prunedFaces;
        }
    }
}
