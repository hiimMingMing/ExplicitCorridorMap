using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Line))]
public class LineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Line myScript = (Line)target;
        if (GUILayout.Button("Bake"))
        {
            myScript.Bake();
        }
    }
}
