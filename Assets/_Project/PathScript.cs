using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathScript : MonoBehaviour
{

    public GameObject[] waypoints;
    public int current = -1;
    
    private void Awake()
    {
        waypoints = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i).gameObject;
        }

        for (int i = waypoints.Length - 1; i >= 0; i--)
        {
            waypoints[i].transform.parent = GameObject.Find("ZONE").transform;
        }
    }

    public GameObject Next()
    {
        if (current + 1 >= waypoints.Length) current = 0;
        else current = current + 1;
        return waypoints[current]; 
    }

    public GameObject Current()
    {
        if (current < 0) current = 0;
        return waypoints[current];
    }
    
    private void OnDrawGizmos() {
        for (int i = 0; i < transform.childCount; i++)
        {
            int h = GetNextIndex(i);
            Gizmos.DrawSphere(GetWaypoint(i), 0.3f);
            Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(h));
        }
    }

    private int GetNextIndex(int i) {
        if (i + 1 == transform.childCount) {
            return 0;
        }
        return i + 1;
    }

    private Vector3 GetWaypoint(int i)
    {
        return transform.GetChild(i).position;
    }
}
