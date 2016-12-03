using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Fusion;

namespace CalculationLibrary
{
    //Code ripped from here:
    //http://wiki.unity3d.com/index.php?title=MeshSmoother
    public class Smoother
    {
        public Vector3[] LaplacianFilter(Vector3[] sv, int[] t)
        {
            Vector3[] wv = new Vector3[sv.Length];
            List<Vector3> adjacentVertices = new List<Vector3>();

            float dx = 0.0f;
            float dy = 0.0f;
            float dz = 0.0f;

            for (int vi = 0; vi < sv.Length; vi++)
            {
                // Find the sv neighboring vertices
                adjacentVertices = findAdjacentNeighbors(sv, t, sv[vi]);

                if (adjacentVertices.Count != 0)
                {
                    dx = 0.0f;
                    dy = 0.0f;
                    dz = 0.0f;

                    //Debug.Log("Vertex Index Length = "+vertexIndexes.Length);
                    // Add the vertices and divide by the number of vertices
                    for (int j = 0; j < adjacentVertices.Count; j++)
                    {
                        dx += adjacentVertices[j].X;
                        dy += adjacentVertices[j].Y;
                        dz += adjacentVertices[j].Z;
                    }

                    wv[vi].X = dx / adjacentVertices.Count;
                    wv[vi].Y = dy / adjacentVertices.Count;
                    wv[vi].Z = dz / adjacentVertices.Count;
                }
            }

            return wv;
        }
        /// <summary>
        ///Finds a set of adjacent vertices for a given vertex. Note the success of this routine expects only the set of neighboring faces to eacn contain one vertex corresponding to the vertex in question
        /// </summary>
        /// <param name="v">All vertices</param>
        /// <param name="t">All triangle indices</param>
        /// <param name="vertex">Vertex in question</param>
        public static List<Vector3> findAdjacentNeighbors(Vector3[] v, int[] t, Vector3 vertex)
        {
            List<Vector3> adjacentV = new List<Vector3>();
            List<int> facemarker = new List<int>();
            int facecount = 0;

            // Find matching vertices
            for (int i = 0; i < v.Length; i++)
                if (Approximately(vertex.X, v[i].X) &&
                    Approximately(vertex.Y, v[i].Y) &&
                    Approximately(vertex.Z, v[i].Z))
                {
                    // Find vertex indices from the triangle array
                    for (int k = 0; k < t.Length; k = k + 3)
                        if (facemarker.Contains(k) == false)
                        {
                            var v1 = 0;
                            var v2 = 0;
                            var marker = false;

                            if (i == t[k])
                            {
                                v1 = t[k + 1];
                                v2 = t[k + 2];
                                marker = true;
                            }

                            if (i == t[k + 1])
                            {
                                v1 = t[k];
                                v2 = t[k + 2];
                                marker = true;
                            }

                            if (i == t[k + 2])
                            {
                                v1 = t[k];
                                v2 = t[k + 1];
                                marker = true;
                            }

                            facecount++;
                            if (marker)
                            {
                                // Once face has been used mark it so it does not get used again
                                facemarker.Add(k);

                                // Add non duplicate vertices to the list
                                if (doesVertexExist(adjacentV, v[v1]) == false)
                                    adjacentV.Add(v[v1]);

                                if (doesVertexExist(adjacentV, v[v2]) == false)
                                    adjacentV.Add(v[v2]);
                            }
                        }
                }

            //Debug.Log("Faces Found = " + facecount);

            return adjacentV;
        }


        /// <summary>
        /// Does the vertex v exist in the list of vertices
        /// </summary>
        private static bool doesVertexExist(List<Vector3> adjacentV, Vector3 v)
        {
            bool marker = false;
            foreach (Vector3 vec in adjacentV)
                if (Approximately(vec.X, v.X) && Approximately(vec.Y, v.Y) && Approximately(vec.Z, v.Z))
                {
                    marker = true;
                    break;
                }

            return marker;
        }

        public static float ApproximateMinDiff = 0.00001f;
        private static bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) <= ApproximateMinDiff;
        }
    }
}
