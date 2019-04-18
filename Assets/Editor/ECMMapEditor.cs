using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExplicitCorridorMap;
using System;

[CustomEditor(typeof(ECMMap))]
public class ECMMapEditor : Editor
{

    ECMMap map;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        map = (ECMMap)target;
        map.Grouping = EditorGUILayout.Toggle("Grouping", map.Grouping);
        if(map.Grouping) map.GroupBehavior = EditorGUILayout.Toggle("Group Behavior", map.GroupBehavior);
        map.CrowDensity = EditorGUILayout.Toggle("Crow Density", map.CrowDensity);

        map.drawGraph = EditorGUILayout.Toggle("Show Graph", map.drawGraph);

        if (map.drawGraph)
        {
            map.drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", map.drawNearestObstaclePoints);
            map.drawVertexLabel = EditorGUILayout.Toggle("Draw Vertex Label", map.drawVertexLabel);
        }
    }
    void OnSceneGUI()
    {
        var ecm = map?.ECMGraph;
        if (ecm == null || !map.drawGraph) return;
        //foreach (var obs in ecm.Obstacles.Values)
        //{
        //    var x = ((float)obs.mBRectangle.LeftTop.X + (float)obs.mBRectangle.RightBottom.X) / 2;
        //    var y = ((float)obs.mBRectangle.LeftTop.Y + (float)obs.mBRectangle.RightBottom.Y) / 2;

        //    Handles.Label(new Vector2(x, y).to3D(), obs.ID.ToString());
        //}
        if (map.drawVertexLabel)
        {
            foreach (var vertex in ecm.Vertices.Values)
            {
                Handles.Label(vertex.Position.To3D(), vertex.ID.ToString());
            }
        }
    }


}



