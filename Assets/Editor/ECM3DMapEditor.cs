//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using ExplicitCorridorMap;
//using System;

//[CustomEditor(typeof(ECM3DMap))]
//public class ECM3DMapEditor : Editor
//{

//    ECM3DMap map;
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        map = (ECM3DMap)target;
//        map.drawGraph = EditorGUILayout.Toggle("Show Graph", map.drawGraph);

//        if (map.drawGraph)
//        {
//            map.inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", map.inputPointRadius);
//            map.outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", map.outputPointRadius);
//            map.drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", map.drawNearestObstaclePoints);
//            map.drawShortestPath = EditorGUILayout.Toggle("Draw Path", map.drawShortestPath);

//        }
//        if (GUILayout.Button("Bake"))
//        {
//            map.Bake();
//        }
//        if (map.drawGraph)
//        {
//            if (GUILayout.Button("Add Obstacle"))
//            {
//                map.AddObstacle();
//            }

//            map.obstacleToDelete = EditorGUILayout.IntField("ObstacleToDelete", map.obstacleToDelete);
//            if (GUILayout.Button("Delete Obstacle"))
//            {
//                map.DeleteObstacle();
//            }
//        }
//    }
//    void OnSceneGUI()
//    {
//        var ecm = map?.ecm;
//        if (ecm == null || !map.drawGraph) return;
//        foreach (var obs in ecm.Obstacles.Values)
//        {
//            var x = ((float)obs.mBRectangle.LeftTop.X + (float)obs.mBRectangle.RightBottom.X) / 2;
//            var y = ((float)obs.mBRectangle.LeftTop.Y + (float)obs.mBRectangle.RightBottom.Y) / 2;

//            Handles.Label(new Vector2(x, y).to3D(), obs.ID.ToString());
//        }
//        foreach (var vertex in ecm.Vertices.Values)
//        {
//            Handles.Label(vertex.Position.to3D(), vertex.ID + "");
//        }
//    }


//}



