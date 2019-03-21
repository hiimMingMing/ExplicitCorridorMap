using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using RBush;
using ExplicitCorridorMap;

[CustomEditor(typeof(MedialAxis))]
public class MedialAxisEditor : Editor
{

    ECM ecm;
    float inputPointRadius = 6f;
    float outputPointRadius = 3f;
    bool drawNearestObstaclePoints = true;
    List<Vector2> shortestPath = null;
    List<Vector2> portalsLeft;
    List<Vector2> portalsRight;
    List<Edge> selectedEdge;
    List<Segment> segments = new List<Segment>();
    int ObstacleToDelete = 0;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var ma = (MedialAxis)target;
        var startPosition = ma.StartPoint.position;
        var endPosition = ma.EndPoint.position;
        var cubes = ma.Cubes;
        var dynamicObstacle = ma.DynamicObstacle;
        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", inputPointRadius);
        outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", outputPointRadius);
        drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", drawNearestObstaclePoints);

        if (GUILayout.Button("Bake"))
        {
            //populate segment
            var obstacles = new List<Obstacle>();
            foreach (Transform cube in cubes)
            {
                obstacles.Add(new Obstacle(Geometry.ConvertToRect(cube)));
            }

            ecm = new ECM(obstacles, new Obstacle(new RectInt(0, 0, 500, 500)));
            ecm.Construct();

            shortestPath = PathFinding.FindPathDebug(ecm, startPosition, endPosition, out portalsLeft,out portalsRight);

        }
        if (GUILayout.Button("Add Obstacle"))
        {
            if (ecm == null)
            {
                Debug.Log("Ecm must be baked");
            }
            else
            {
                ecm.AddPolygonDynamic(new Obstacle(Geometry.ConvertToRect(dynamicObstacle)));
                shortestPath = PathFinding.FindPathDebug(ecm, startPosition, endPosition, out portalsLeft, out portalsRight);
            }
        }

        ObstacleToDelete = EditorGUILayout.IntField("ObstacleToDelete", ObstacleToDelete);
        if (GUILayout.Button("Delete Obstacle"))
        {
            if (ecm == null)
            {
                Debug.Log("Ecm must be baked");
            }
            else
            {
                ecm.DeletePolygonDynamic(ObstacleToDelete);
                shortestPath = PathFinding.FindPathDebug(ecm, startPosition, endPosition, out portalsLeft, out portalsRight);
            }
        }
    }

    void OnSceneGUI()
    {

        //Draw input point
        if (ecm == null) return;

        foreach(var obs in ecm.Obstacles.Values)
        {
            Handles.Label(new Vector2((obs.Envelope.MinX+obs.Envelope.MaxX)/2, (obs.Envelope.MinY + obs.Envelope.MaxY) / 2), obs.ID.ToString());
        }
        //Draw input segment
        Handles.color = Color.yellow;
        foreach (var inputSegment in ecm.InputSegments.Values)
        {
            var startPoint = new Vector3(inputSegment.Start.x, inputSegment.Start.y);
            var endPoint = new Vector3(inputSegment.End.x, inputSegment.End.y);
            Handles.Label((startPoint+endPoint)/2, inputSegment.ID.ToString());

            Handles.DrawSolidDisc(startPoint, Vector3.forward, inputPointRadius);
            Handles.DrawSolidDisc(endPoint, Vector3.forward, inputPointRadius);
            Handles.DrawLine(startPoint, endPoint);
        }

        //Draw ouput edge and vertex
        foreach (var vertex in ecm.Vertices.Values)
        {
            foreach (var edge in vertex.Edges)
            {
                DrawEdge(edge);
            }
        }
        
        //Draw Nearest Obstacle Point
        if (drawNearestObstaclePoints)
        {
            foreach (var edge in ecm.Edges.Values)
            {
                DrawObstaclePoint(edge);
            }
        }
        if ( shortestPath != null)
        {
            var path = shortestPath.ConvertAll(x => new Vector3(x.x, x.y)).ToArray();
            Handles.color = Color.red;
            Handles.DrawPolyLine(path);
        }
        if(portalsLeft != null)
        {
            for(int i = 0; i < portalsLeft.Count; i++)
            {
                DrawPortal(portalsLeft[i], portalsRight[i]);
            }
        }
    }
    


    
    void DrawPortal(Vector2 begin, Vector2 end)
    {
        Handles.color = Color.magenta;
        Handles.DrawLine(begin,end);

    }
    void DrawObstaclePoint(Edge edge)
    {
        var startVertex = edge.Start.Position;
        var endVertex = edge.End.Position;
        var begin = startVertex;
        var obsLeft = edge.LeftObstacleOfStart;
        var obsRight = edge.RightObstacleOfStart;

        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

        begin = endVertex;
        obsLeft = edge.LeftObstacleOfEnd;
        obsRight = edge.RightObstacleOfEnd;
        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

    }
    void DrawVertex(Vertex vertex)
    {
        var position = new Vector3((float)vertex.X, (float)vertex.Y);
        Handles.DrawSolidDisc(position, Vector3.forward, outputPointRadius);
        Handles.Label(position,vertex.ID+"");
    }
    void DrawEdge(Edge edge)
    {
        //if (!outputSegment.IsFinite) return;
        //if (!outputSegment.IsFinite || !outputSegment.IsPrimary)
        //    return;
        Vertex start = edge.Start;
        Vertex end = edge.End;

        if (edge.IsLinear)
        {
            var startPoint = new Vector3((float)start.X, (float)start.Y);
            var endPoint = new Vector3((float)end.X, (float)end.Y);
            Handles.color = Color.magenta;
            DrawVertex(start);
            Handles.color = Color.red;
            DrawVertex(end);
            Handles.color = Color.blue;
            Handles.DrawLine(startPoint, endPoint);
        }
        else
        {
            List<Vector2> discretizedEdge = ecm.SampleCurvedEdge(edge, 10);
            var curve = discretizedEdge.ConvertAll(x => (Vector3)x).ToArray();
            Handles.color = Color.blue;
            Handles.DrawPolyLine(curve);

            Handles.color = Color.magenta;
            DrawVertex(start);
            Handles.color = Color.red;
            DrawVertex(end);
        }
    }
    
    
}



