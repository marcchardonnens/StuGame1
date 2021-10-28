using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshGenerator map = (MeshGenerator)target;
        if (DrawDefaultInspector() && map.autoupdate)
        {
            map.GenerateInternal();
        }

        if (GUILayout.Button("Generate"))
        {
            map.GenerateInternal();
        }
    }

}
