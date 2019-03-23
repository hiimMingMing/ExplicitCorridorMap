using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExplicitCorridorMap;
using RBush;

[CustomEditor(typeof(ECMMap))]
public class ECMMapEditor : Editor
{

    ECMMap map;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        map = (ECMMap)target;
        map.drawGraph = EditorGUILayout.Toggle("Show Graph", map.drawGraph);

        if (map.drawGraph)
        {
            map.inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", map.inputPointRadius);
            map.outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", map.outputPointRadius);
            map.drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", map.drawNearestObstaclePoints);
            map.drawShortestPath = EditorGUILayout.Toggle("Draw Path", map.drawShortestPath);

        }
        if (GUILayout.Button("Bake"))
        {
            map.Bake();
        }
        if (map.drawGraph)
        {
            if (GUILayout.Button("Add Obstacle"))
            {
                map.AddObstacle();
            }

            map.obstacleToDelete = EditorGUILayout.IntField("ObstacleToDelete", map.obstacleToDelete);
            if (GUILayout.Button("Delete Obstacle"))
            {
                map.DeleteObstacle();
            }
        }
    }
    void OnSceneGUI()
    {
        var ecm = map.ecm;
        if (ecm == null || !map.drawGraph) return;
        foreach (var obs in ecm.Obstacles.Values)
        {
            Handles.Label(new Vector2((obs.Envelope.MinX + obs.Envelope.MaxX) / 2, (obs.Envelope.MinY + obs.Envelope.MaxY) / 2), obs.ID.ToString());
        }
        foreach (var vertex in ecm.Vertices.Values)
        {
            Handles.Label(vertex.Position, vertex.ID + "");
        }
    }


}



