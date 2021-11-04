using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainBuilder))]
public class TerrainBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainBuilder terrainBuilder = (TerrainBuilder)target;
        if (GUILayout.Button("Generate"))
        {
            terrainBuilder.MakeTerrain();
        }
        //if (terrainBuilder.autoupdate)
        //{
        //    terrainBuilder.MakeTerrain();
        //}

        base.OnInspectorGUI();
    }
}
