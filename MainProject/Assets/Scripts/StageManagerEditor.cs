using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR

[CustomEditor(typeof(StageManager))]
public class StageManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StageManager map = (StageManager)target;
        
        if (GUILayout.Button("Generate"))
        {
            map.MakeStage();
            //map.GenerateSimple();
        }

        if (DrawDefaultInspector() && map.autoupdate)
        {
            map.MakeStage();
            //map.GenerateSimple();
        }

    }
}

#endif