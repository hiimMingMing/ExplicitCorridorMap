using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameAgent))]
public class AgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ECMMap = FindObjectOfType<ECMMap>();
        DrawDefaultInspector();
        var agent = (GameAgent)target;
        var radiusList = ECMMap.AgentRadiusList;
        var options = new string[radiusList.Count];
        for (int i = 0; i < radiusList.Count; i++)
        {
            options[i] = radiusList[i].ToString();
        }
        agent.RadiusIndex = EditorGUILayout.Popup("Radius", agent.RadiusIndex, options);
        agent.Radius = radiusList[agent.RadiusIndex];
    }
}
