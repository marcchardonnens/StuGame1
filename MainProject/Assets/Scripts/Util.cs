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

    public static float HalfRandomRange(float value)
    {
        return Random.Range(-value, value) / 2f;
    } 

    public static void SetChildrenActive (Transform transform, bool active) {
        SetChildrenActive (transform.gameObject, active);
    }
 
    private static void SetChildrenActive (GameObject obj, bool active) {
        for (int i=0; i < obj.transform.childCount; i++) {
            GameObject childObj = obj.transform.GetChild(i).gameObject;
            childObj.SetActive (active);
            SetChildrenActive (childObj, active);
        }
    }
 
}
