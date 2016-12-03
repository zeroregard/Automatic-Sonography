using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scanner : MonoBehaviour
{
    public GameObject PointPrefab;
    public Vector3 Offset;
    public int YSteps;
    public float AngleSteps = 0.01f;
    public float RadiusLower;
    public Analyzer analyzer;
    private GameObject Points;
	// Use this for initialization
	void Start ()
    {
        Points = new GameObject();
        Points.name = "Points";
        Points.transform.position = Offset;
        var heights = GenerateCircleHeights(Offset, RadiusLower);
        heights.Reverse();
        heights.RemoveRange(0, heights.Count / 2);
        var topDownCirclePosition = Offset;
        for (int i = 0; i < heights.Count; i++)
        {
            //Debug.Log(heights[i]);
            var radius = heights[i].y - Offset.x;
            //Debug.Log(radius);
            topDownCirclePosition.y = Offset.y + heights[i].x;
            Debug.Log(topDownCirclePosition.y);
            Circle(topDownCirclePosition, radius);
        }
        analyzer.Analyze(Points.transform);
    }

    List<Vector2> GenerateCircleHeights(Vector3 centerOffset, float radius)
    {
        float x, y;
        float angle = 0.0f;
        var heights = new List<Vector2>();
        while (angle < Mathf.PI)
        {
            x = radius * Mathf.Cos(angle);
            y = radius * Mathf.Sin(angle);
            angle += AngleSteps;
            heights.Add(new Vector2(x,y));
        }
        return heights;
    }
    

    void Circle(Vector3 centerOffset, float radius)
    {
        float x, z;
        float angle = 0.0f;

        // go through all angles from 0 to 2 * PI radians
        while (angle < 2 * Mathf.PI)
        {
            // calculate x, y from a vector with known length and angle
            x = radius * Mathf.Cos(angle);
            z = radius * Mathf.Sin(angle);
            Vector3 position = new Vector3(x, 0, z);
            position.x += centerOffset.x;
            position.y += centerOffset.y;
            position.z += centerOffset.z;
            var instance = Instantiate(PointPrefab, position, Quaternion.identity) as GameObject;
            instance.transform.LookAt(Offset);
            instance.transform.SetParent(Points.transform);
            angle += AngleSteps;
        }
    }
}
