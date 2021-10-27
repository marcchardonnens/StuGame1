using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainBuilder))]
public class TerrainBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainBuilder terrainBuilder = (TerrainBuilder)target;
        if (DrawDefaultInspector() && terrainBuilder.autoupdate)
        {
            terrainBuilder.MakeTerrain();
        }

        if (GUILayout.Button("Generate"))
        {
            terrainBuilder.MakeTerrain();
        }
    }
}
