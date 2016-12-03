using UnityEngine;
using System.Collections.Generic;

public class Analyzer : MonoBehaviour
{

    public GameObject hitPrefab;
	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Analyze(Transform points)
    {
        foreach (Transform p in points)
        {
            ThrowRay(p.position, p.transform.forward.normalized);
        }
    }

    private void ThrowRay(Vector3 begin, Vector3 direction)
    {
        //layer 8
        RaycastHit hit;
        if (Physics.Raycast(begin, direction, out hit))
        {
            VertexFound(hit.point);
        }
    }

    private void VertexFound(Vector3 vertex)
    {
        Instantiate(hitPrefab, vertex, Quaternion.identity);
    }
}
