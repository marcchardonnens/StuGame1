using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



#if UNITY_EDITOR 

[CustomEditor(typeof(TerrainBuilder))]
public class TerrainBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainBuilder terrainBuilder = (TerrainBuilder)target;
        if (GUILayout.Button("Generate"))
        {
            if (terrainBuilder.IsHubMode)
            {
                terrainBuilder.MakeHub();
            }
            else
            {
                terrainBuilder.MakeTerrain();
            }
        }
        //if (terrainBuilder.autoupdate)
        //{
        //    terrainBuilder.MakeTerrain();
        //}

        base.OnInspectorGUI();
    }
}
#endif