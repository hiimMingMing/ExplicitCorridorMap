//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(Player))]
//public class AgentEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        var agent = (Player)target;
//        var radiusList = agent.ecmMap.AgentRadiusList;
//        var options = new string[radiusList.Count];
//        for(int i=0;i<radiusList.Count;i++)
//        {
//            options[i] = radiusList[i].ToString();
//        }
//        agent.RadiusIndex = EditorGUILayout.Popup("Radius", agent.RadiusIndex, options);
//        agent.Radius = radiusList[agent.RadiusIndex];
//    }
//}
