using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Voronoi))]
public class VoronoiEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Voronoi myScript = (Voronoi)target;
        if (GUILayout.Button("Bake"))
        {
            myScript.Bake();
        }
    }
}
