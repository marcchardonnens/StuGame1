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
        StageManager stageManager = (StageManager)target;
        
        if (GUILayout.Button("Generate"))
        {
            stageManager.SetupStage();
            //map.GenerateSimple();
        }

        if (DrawDefaultInspector() && stageManager.autoupdate)
        {
            stageManager.SetupStage();
            //map.GenerateSimple();
        }

    }
}

#endif