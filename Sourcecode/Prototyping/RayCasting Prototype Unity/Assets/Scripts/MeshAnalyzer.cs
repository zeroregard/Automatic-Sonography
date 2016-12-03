using UnityEngine;
using System.Collections.Generic;

public class Triangle
{
    public Vector3 first;
    public Vector3 second;
    public Vector3 third;
    public Normal normal;

    public Triangle(Vector3 f, Vector3 s, Vector3 t)
    {
        first = f;
        second = s;
        third = t;
    }
}

public class Normal
{
    public Vector3 position;
    public Vector3 direction;

    public Normal(Vector3 p, Vector3 d)
    {
        position = p;
        direction = d;
    }

    public Vector3 EndPoint(float offset)
    {
        return position + direction*offset;
    }
}

public class MeshAnalyzer : MonoBehaviour
{
    public Mesh mesh;
    public List<Triangle> Triangles = new List<Triangle>();
    public List<Normal> Normals = new List<Normal>();
    public float NormalOffset;

    

    void Start()
    {
        FindTriangles();
        FindNormals();
        RemoveUnnecesaryTriangles();
        ShowNormals();
    }

    void RemoveUnnecesaryTriangles()
    {
        List<Triangle> newTriangleList = new List<Triangle>();
        foreach (var t in Triangles)
        {
            if(t.normal.direction.y >= 0)
            {
                newTriangleList.Add(t);
            }
        }
        Triangles = newTriangleList;
    }

    void FindTriangles()
    {
        Vector3 first = Vector3.zero;
        Vector3 second = Vector3.zero;
        Vector3 third = Vector3.zero;
        int count = 3;
        Debug.Log(count % 3);
        foreach (var t in mesh.triangles)
        {
            if (count % 3 == 0)
                first = mesh.vertices[t];
            else if (count % 3 == 1)
                second = mesh.vertices[t];
            else if (count % 3 == 2)
            {
                third = mesh.vertices[t];
                Triangles.Add(new Triangle(first, second, third));
            }
            count++;
        }
    }

    void FindNormals()
    {
        foreach (var t in Triangles)
        {
            t.normal = new Normal(center(t), normal(t));
        }
    }

    void ShowNormals()
    {
        foreach (var t in Triangles)
        {
            Debug.DrawLine(t.normal.position, t.normal.EndPoint(NormalOffset), Color.red, 9000);
            //var endPoint = Instantiate(NormalEndPoint, t.normal.EndPoint(), Quaternion.identity) as GameObject;
            //endPoint.transform.Rotate(n.direction);
        }
    }

    Vector3 center(Triangle t)
    {
        return (t.first + t.second + t.third) / 3;
    }

    Vector3 normal(Triangle t)
    {
        return Vector3.Cross(t.first - t.second, t.first - t.third).normalized;
    }
}
