using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using KdTree;
using KdTree.Math;
using ExplicitCorridorMap;
using Advanced.Algorithms.Geometry;
[CustomEditor(typeof(MedialAxis))]
public class MedialAxisEditor : Editor
{
    ECM ecm;
    float inputPointRadius = 12f;
    float outputPointRadius = 6f;
    int segmentCount = 100000;
    bool drawNearestObstaclePoints = false;
    int startIndex = 0;
    int goalIndex = 0;
    List<Edge> edgeList = null;
    List<Vector2> portalsLeft;
    List<Vector2> portalsRight;
    List<Vector2> shortestPath; 
    void AddRect(List<Segment> segments, RectInt rect)
    {

        segments.Add(new Segment(rect.x, rect.y, rect.x, rect.yMax));
        segments.Add(new Segment(rect.x, rect.yMax, rect.xMax, rect.yMax));
        segments.Add(new Segment(rect.xMax, rect.yMax, rect.xMax, rect.y));
        segments.Add(new Segment(rect.xMax, rect.y, rect.x, rect.y));

    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", inputPointRadius);
        outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", outputPointRadius);
        drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", drawNearestObstaclePoints);
        startIndex = EditorGUILayout.IntField("StarIndex", startIndex);
        goalIndex = EditorGUILayout.IntField("GoalIndex", goalIndex);

        if (GUILayout.Button("Bake"))
        {
            //populate segment
            List<Segment> InputSegments = new List<Segment>();
            var obstacles = new List<RectInt>();
            obstacles.Add( new RectInt(50, 50, 80, 80));
            obstacles.Add( new RectInt(200, 100, 200, 100));
            obstacles.Add( new RectInt(75, 225, 60, 250));
            obstacles.Add( new RectInt(250, 250, 60, 200));
            obstacles.Add( new RectInt(360, 300, 100, 100));

            //add border
            AddRect(InputSegments,new RectInt(0, 0, 500, 500));
            foreach (var r in obstacles)
            {
                AddRect(InputSegments, r);
            }
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            ecm = new ECM(obstacles);
            foreach (var segment in InputSegments)
            {
                ecm.AddSegment(segment.Start.x, segment.Start.y, segment.End.x, segment.End.y);
            }
            ecm.Construct();
            //watch.Stop();
            //Debug.Log("Time: " + watch.ElapsedMilliseconds);

            //startIndex = UnityEngine.Random.Range(0, ecm.Vertices.Count);
            //goalIndex = UnityEngine.Random.Range(0, ecm.Vertices.Count);

            var start = ecm.Vertices[startIndex];
            var goal = ecm.Vertices[goalIndex];
            Debug.Log("Find path from " + startIndex + " to " + goalIndex);
            edgeList = Astar.FindPath(ecm, start, goal);
            Debug.Log("Path length:" + edgeList.Count);
            //foreach (var e in edgeList)
            //{
            //    Debug.Log(e.Start + "-" + e.End);
            //}
            portalsLeft = new List<Vector2>();
            portalsRight = new List<Vector2>();
            ComputePortals(portalsLeft, portalsRight);
            shortestPath = GetShortestPath(portalsLeft, portalsRight);

            //var kdTree = new KdTree<double, Vertex>(2, new DoubleMath());
            //foreach(var v in ecm.Vertices.Values)
            //{
            //    kdTree.Add(v.GetKDKey(), v);
            //}
            //var nodes = kdTree.GetNearestNeighbours(new double[] {200,100},4);
            //foreach(var node in nodes)
            //{
            //    var vertex = node.Value;
            //    Debug.Log(vertex.ID + "[" + vertex.X + "," + vertex.Y +"]");
            //}
            //var a = new Vertex(1,Tuple.Create(3.14f, 3.14f, 1L));
            //var b = new Vertex(2, Tuple.Create(3.14f, 3.14f, 1L));
            //Debug.Log(a == b);
            //Debug.Log(a.Equals(b));
            var ps = new List<Point>();
            ps.Add(new Point(0, 0));
            ps.Add(new Point(0, 100));
            ps.Add(new Point(100, 100));
            ps.Add(new Point(100, 0));
            var polygon = new Polygon(ps);
            Debug.Log(PointInsidePolygon.IsInside(polygon, new Point(0, 0)));
            Debug.Log(PointInsidePolygon.IsInside(polygon, new Point(0, 100)));
            Debug.Log(PointInsidePolygon.IsInside(polygon, new Point(100, 100)));
            Debug.Log(PointInsidePolygon.IsInside(polygon, new Point(100, 0)));

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
    //private void OnSceneGUI()
    //{
    //    if (ecm == null) return;
    //    var v = ecm.Vertices[10];
    //    //if (v == null) return;
    //    //Handles.color = Color.magenta;
    //    //DrawVertex(v);
    //    var e = ecm.Edges[v.IncidentEdge];
    //    int c = 0;
    //    do
    //    {
    //        c++;
    //        DrawEdge(e);
    //        e = ecm.Edges[e.RotNext];
    //    }
    //    while (e != ecm.Edges[v.IncidentEdge]);
    //    Debug.Log(c);
    //    //DrawObstaclePoint(e);
    //    //var e2 = ecm.Edges[e.Twin];
    //    //DrawEdge(e2);


    //    /*CELL*/
    //    //var c = ecm.Cells[e.Cell];
    //    //var ie = ecm.Edges[c.IncidentEdge];
    //    //int count = 0;
    //    //do
    //    //{
    //    //    count++;
    //    //    DrawEdge(ie);
    //    //    DrawObstaclePoint(ie);

    //    //    ie = ecm.Edges[ie.Next];
    //    //}
    //    //while (ie != ecm.Edges[c.IncidentEdge]);
    //    //var e2 = ecm.Edges[e.Next];
    //    //DrawEdge(e2);
    //}
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
        Handles.color = Color.blue;
        foreach (var edge in ecm.Edges.Values)
        {
            DrawEdge(edge);
        }

        //Draw Nearest Obstacle Point
        if (drawNearestObstaclePoints)
        {
            foreach (var edge in ecm.Edges.Values)
            {
                DrawObstaclePoint(edge);
            }
        }
        if (edgeList != null && portalsLeft!=null && shortestPath!= null)
        {
            //foreach (var edge in edgeList)
            //{
            //    DrawObstaclePoint(edge);
            //}

            ////Debug.Log("PorCountt:" + portalsLeft.Count);
            for (int i = 0; i < portalsLeft.Count; i++)
            {
                DrawPortal(portalsLeft[i], portalsRight[i]);
            }

            var path = shortestPath.ConvertAll(x => new Vector3(x.x, x.y)).ToArray();
            Handles.color = Color.red;
            Handles.DrawPolyLine(path);
        }
    }
    float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
    {
        float ax = b.x - a.x;
        float ay = b.y - a.y;
        float bx = c.x - a.x;
        float by = c.y - a.y;
        return bx * ay - ax * by;
    }


    List<Vector2> GetShortestPath(List<Vector2> portalsLeft, List<Vector2> portalsRight)
    {
        List<Vector2> path = new List<Vector2>();
        if (portalsLeft.Count == 0) return path;
        Vector2 portalApex, portalLeft, portalRight;
        int apexIndex = 0, leftIndex = 0, rightIndex = 0;
        portalApex = portalsLeft[0];
        portalLeft = portalsLeft[0];
        portalRight = portalsRight[0];
        path.Add(portalApex);
        for(int i = 1; i < portalsLeft.Count; i++)
        {
            var left = portalsLeft[i];
            var right = portalsRight[i];
            // Update right vertex.
            if (CrossProduct(portalApex,portalRight,right) <= 0.0f)
            {
                if(portalApex.Equals(portalRight) || CrossProduct(portalApex, portalLeft, right) > 0.0f)
                {
                    // Tighten the funnel.
                    portalRight = right;
                    rightIndex = i;
                }
                else
                {
                    path.Add( portalLeft);
                    // Make current left the new apex.
                    portalApex = portalLeft;
                    apexIndex = leftIndex;
                    // Reset portal
                    portalLeft = portalApex;
                    portalRight = portalApex;
                    leftIndex = apexIndex;
                    rightIndex = apexIndex;
                    // Restart scan
                    i = apexIndex;
                    continue;
                }
            }
            // Update left vertex.
            if (CrossProduct(portalApex, portalLeft, left) >= 0.0f)
            {
                if (portalApex.Equals(portalLeft) || CrossProduct(portalApex, portalRight, left) < 0.0f)
                {
                    // Tighten the funnel.
                    portalLeft = left;
                    leftIndex = i;
                }
                else
                {
                    // Left over right, insert right to path and restart scan from portal right point.
                    path.Add(portalRight);
                    // Make current right the new apex.
                    portalApex = portalRight;
                    apexIndex = rightIndex;
                    // Reset portal
                    portalLeft = portalApex;
                    portalRight = portalApex;
                    leftIndex = apexIndex;
                    rightIndex = apexIndex;
                    // Restart scan
                    i = apexIndex;
                    continue;
                }
            }//if
        }//for
        path.Add(portalsLeft[portalsLeft.Count - 1]);
        return path;
    }//funtion
    void ComputePortals(List<Vector2> portalsLeft, List<Vector2> portalsRight)
    {

        for (int i = 0; i < edgeList.Count; i++)
        {
            var edge = edgeList[i];
            if (i != 0)
            {
                var left1 = edge.LeftObstacleStart;
                var right1 = edge.RightObstacleStart;
                portalsLeft.Add(left1);
                portalsRight.Add(right1);
            }
            else
            {
                var start = ecm.Vertices[ edge.Start];
                var point = start.Position;
                portalsLeft.Add(point);
                portalsRight.Add(point);
            }
            if (i == edgeList.Count - 1)
            {
                var end = ecm.Vertices[edge.End];
                var point = end.Position;
                portalsLeft.Add(point);
                portalsRight.Add(point);
                break;
            }
            var edgeNext = edgeList[i + 1];
            if (edge.LeftObstacleEnd.Equals(edgeNext.LeftObstacleStart) ||
                edge.RightObstacleEnd.Equals(edgeNext.RightObstacleStart))
            {
                continue;
            }
            var left2 = edge.LeftObstacleEnd;
            var right2 = edge.RightObstacleEnd;
            portalsLeft.Add(left2);
            portalsRight.Add(right2);
        }
    }
    void DrawPortal(Vector2 begin, Vector2 end)
    {
        Handles.color = Color.white;
        Handles.DrawLine(begin,end);

    }
    void DrawObstaclePoint(Edge edge)
    {
        if (!edge.IsFinite || !edge.IsPrimary) return ;
        var startVertex = ecm.Vertices[edge.Start].Position;
        var endVertex = ecm.Vertices[edge.End].Position;
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
        Handles.Label(position,vertex.ID+" "+vertex.isInside);
    }
    void DrawEdge(Edge outputSegment)
    {
        //if (!outputSegment.IsFinite) return;
        if (!outputSegment.IsFinite || !outputSegment.IsPrimary)
            return;
        Vertex start = ecm.Vertices[outputSegment.Start];
        Vertex end = ecm.Vertices[outputSegment.End];

        if (outputSegment.IsLinear)
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
            List<Vector2> discretizedEdge = ecm.SampleCurvedEdge(outputSegment, 10);
            var curve = discretizedEdge.ConvertAll(x=>(Vector3)x).ToArray();
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
                bv.AddSegment(segment.Start.x, segment.Start.y, segment.End.x, segment.End.y);



            bv.Construct();

            // Stop timing.
            stopwatch.Stop();
            Debug.Log(String.Format("Vertices: {0}, Edges: {1}, Cells: {2}", bv.Vertices.Count, bv.Edges.Count, bv.Cells.Count));
            Debug.Log("Time elapsed:" + stopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss"));

            //bv.Clear();
            
            inputPoints.Clear();
            inputSegments.Clear();


        }
    
}

