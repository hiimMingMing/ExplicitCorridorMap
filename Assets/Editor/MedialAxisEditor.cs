using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using ExplicitCorridorMap;

[CustomEditor(typeof(MedialAxis))]
public class MedialAxisEditor : Editor
{

    ECM ecm;
    float inputPointRadius = 6f;
    float outputPointRadius = 3f;
    int segmentCount = 100000;
    bool drawNearestObstaclePoints = true;
    List<Vector2> shortestPath; 
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var ma = (MedialAxis)target;
        var StartPosition = ma.StartPoint.position;
        var EndPosition = ma.EndPoint.position;
        var cubes = ma.Cubes;
        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", inputPointRadius);
        outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", outputPointRadius);
        drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", drawNearestObstaclePoints);

        if (GUILayout.Button("Bake"))
        {
            //populate segment
            var obstacles = new List<RectInt>();
            foreach (Transform cube in cubes)
            {
                int w = (int)cube.localScale.x;
                int h = (int)cube.localScale.y;
                int x = (int)cube.position.x;
                int y = (int)cube.position.y;
                obstacles.Add(new RectInt(x - w / 2, y - h / 2, w, h));
            }

            ecm = new ECM(obstacles);
            //add border
            ecm.AddRect(new RectInt(0, 0, 500, 500));
            ecm.Construct();

            shortestPath = PathFinding.FindPath(ecm, StartPosition, EndPosition);
            
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Test with random segments");
        segmentCount = EditorGUILayout.IntField("Number of Segments", segmentCount);
        if (GUILayout.Button("Test"))
        {
            var points = new List<Vector2Int>();
            var segments = PopulateSegment(100, segmentCount/100);
            ConstructAndMeasure(ref points, ref segments);
        }

    }

    void OnSceneGUI()
    {
        if (ecm == null) return;
        //Draw input point

        Handles.color = Color.yellow;
        foreach (var inputPoint in ecm.InputPoints.Values)
        {
            var position = new Vector3(inputPoint.x, inputPoint.y);
            Handles.DrawSolidDisc(position, Vector3.forward, inputPointRadius);
        }
        //Draw input segment
        foreach (var inputSegment in ecm.InputSegments.Values)
        {
            var startPoint = new Vector3(inputSegment.Start.x, inputSegment.Start.y);
            var endPoint = new Vector3(inputSegment.End.x, inputSegment.End.y);
            Handles.DrawSolidDisc(startPoint, Vector3.forward, inputPointRadius);
            Handles.DrawSolidDisc(endPoint, Vector3.forward, inputPointRadius);
            Handles.DrawLine(startPoint, endPoint);
        }

        //Draw ouput edge and vertex
        //Handles.color = Color.blue;
        //foreach (var edge in ecm.Edges.Values)
        //{
        //    DrawEdge(edge);
        //}
        //Debug.Log(ecm.Vertices.Count);
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

       

    }
    


    
    void DrawPortal(Vector2 begin, Vector2 end)
    {
        Handles.color = Color.white;
        Handles.DrawLine(begin,end);

    }
    void DrawObstaclePoint(Edge edge)
    {
        var startVertex = edge.Start.Position;
        var endVertex = edge.End.Position;
        var begin = startVertex;
        var obsLeft = edge.LeftObstacleStart;
        var obsRight = edge.RightObstacleStart;

        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

        begin = endVertex;
        obsLeft = edge.LeftObstacleEnd;
        obsRight = edge.RightObstacleEnd;
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
    List<Segment> PopulateSegment(int maxX, int maxY)
    {
        List<Segment> segments = new List<Segment>();
        for (int i = 0; i < maxX; i++)
        {
            for (int j = 0; j < maxY; j++)
            {
                segments.Add(new Segment(new Vector2Int(i, j), new Vector2Int(i, j + 1)));
                //segments.Add(new Segment(new Point(i, j), new Point(i + 1, j)));
                //segments.Add(new Segment(new Point(i, j), new Point(i + 1, j + 1)));
            }
        }
        return segments;
    }

    void ConstructAndMeasure(ref List<Vector2Int> inputPoints, ref List<Segment> inputSegments)
        {
            Debug.Log(String.Format("Testing with {0} points and {1} segments", inputPoints.Count, inputSegments.Count));
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var bv = new ECM();
            foreach (var point in inputPoints)
                bv.AddPoint(point.x, point.y);

            foreach (var segment in inputSegments)
                bv.AddSegment(segment);
            
            bv.Construct();

            // Stop timing.
            stopwatch.Stop();
            Debug.Log(String.Format("Vertices: {0}", bv.Vertices.Count));
            Debug.Log("Time elapsed:" + stopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss"));

            //bv.Clear();
            
            inputPoints.Clear();
            inputSegments.Clear();


        }
    
}

