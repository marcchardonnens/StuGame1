using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(MapTexture))]
public class MapTextureEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        MapTexture map = (MapTexture)target;
        if(DrawDefaultInspector() && map.autoupdate)
        {
            map.Generate();
            //map.GenerateSimple();
        }
        
        if(GUILayout.Button("Generate"))
        {
            map.Generate();
            //map.GenerateSimple();
        }
    }
}
