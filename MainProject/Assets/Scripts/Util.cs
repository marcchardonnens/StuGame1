using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static int SecondsToMillis(float s)
    {
        return (int)(s * 1000);
    }

    public static Vector2 VectorXZ(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
}
