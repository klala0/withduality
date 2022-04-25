using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float ZeroYDistance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(
            new Vector3(a.x, 0, a.z),
            new Vector3(b.x, 0, b.z));
    }
}