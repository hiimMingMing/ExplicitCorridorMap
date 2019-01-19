using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SharpBoostVoronoi;
using SharpBoostVoronoi.Input;
using SharpBoostVoronoi.Output;
using System;
using KdTree;
using KdTree.Math;
[CustomEditor(typeof(MedialAxis))]
public class MedialAxisEditor : Editor
{
    BoostVoronoi VoronoiSolution { get; set; }
    float inputPointRadius = 12f;
    float outputPointRadius = 6f;
    int segmentCount = 100000;
    bool drawNearestObstaclePoints = false;
    int startIndex = 0;
    int goalIndex = 0;
    List<Edge> edgeList = null;
    List<Point> portalsLeft;
    List<Point> portalsRight;
    List<Point> shortestPath; 
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
            AddRect(InputSegments, new RectInt(0, 0, 500, 500));
            AddRect(InputSegments, new RectInt(50, 50, 80, 80));
            AddRect(InputSegments, new RectInt(200, 100, 200, 100));
            AddRect(InputSegments, new RectInt(75, 225, 60, 250));
            AddRect(InputSegments, new RectInt(250, 250, 60, 200));
            AddRect(InputSegments, new RectInt(360, 300, 100, 100));

            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            VoronoiSolution = new BoostVoronoi();
            foreach (var segment in InputSegments)
            {
                VoronoiSolution.AddSegment(segment.Start.X, segment.Start.Y, segment.End.X, segment.End.Y);
            }
            VoronoiSolution.Construct();
            //watch.Stop();
            //Debug.Log("Time: " + watch.ElapsedMilliseconds);

            //startIndex = UnityEngine.Random.Range(0, VoronoiSolution.Vertices.Count);
            //goalIndex = UnityEngine.Random.Range(0, VoronoiSolution.Vertices.Count);

            var start = VoronoiSolution.Vertices[startIndex];
            var goal = VoronoiSolution.Vertices[goalIndex];
            Debug.Log("Find path from " + startIndex + " to " + goalIndex);
            edgeList = Astar.FindPath(VoronoiSolution, start, goal);
            Debug.Log("Path length:" + edgeList.Count);
            //foreach (var e in edgeList)
            //{
            //    Debug.Log(e.Start + "-" + e.End);
            //}
            portalsLeft = new List<Point>();
            portalsRight = new List<Point>();
            ComputePortals(portalsLeft, portalsRight);
            shortestPath = GetShortestPath(portalsLeft, portalsRight);
            //Debug.Log("SP:"+shortestPath.Count);
            //foreach(var p in shortestPath)
            //{
            //    Debug.Log(p);
            //}
            var kdTree = new KdTree<double, Vertex>(2, new DoubleMath());
            foreach(var v in VoronoiSolution.Vertices.Values)
            {
                kdTree.Add(v.GetKDKey(), v);
            }
            var nodes = kdTree.GetNearestNeighbours(new double[] {200,100},4);
            foreach(var node in nodes)
            {
                var vertex = node.Value;
                Debug.Log(vertex.ID + "[" + vertex.X + "," + vertex.Y +"]");
            }
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Test with random segments");
        segmentCount = EditorGUILayout.IntField("Number of Segments", segmentCount);
        if (GUILayout.Button("Test"))
        {
            List<Point> points = new List<Point>();
            List<Segment> segments = new List<Segment>();
            points = new List<Point>();
            segments = PopulateSegment(100, segmentCount/100);
            ConstructAndMeasure(ref points, ref segments);
        }

    }
    //private void OnSceneGUI()
    //{
    //    if (VoronoiSolution == null) return;
    //    var v = VoronoiSolution.Vertices[10];
    //    //if (v == null) return;
    //    //Handles.color = Color.magenta;
    //    //DrawVertex(v);
    //    var e = VoronoiSolution.Edges[v.IncidentEdge];
    //    int c = 0;
    //    do
    //    {
    //        c++;
    //        DrawEdge(e);
    //        e = VoronoiSolution.Edges[e.RotNext];
    //    }
    //    while (e != VoronoiSolution.Edges[v.IncidentEdge]);
    //    Debug.Log(c);
    //    //DrawObstaclePoint(e);
    //    //var e2 = VoronoiSolution.Edges[e.Twin];
    //    //DrawEdge(e2);


    //    /*CELL*/
    //    //var c = VoronoiSolution.Cells[e.Cell];
    //    //var ie = VoronoiSolution.Edges[c.IncidentEdge];
    //    //int count = 0;
    //    //do
    //    //{
    //    //    count++;
    //    //    DrawEdge(ie);
    //    //    DrawObstaclePoint(ie);

    //    //    ie = VoronoiSolution.Edges[ie.Next];
    //    //}
    //    //while (ie != VoronoiSolution.Edges[c.IncidentEdge]);
    //    //var e2 = VoronoiSolution.Edges[e.Next];
    //    //DrawEdge(e2);
    //}
    void OnSceneGUI()
    {
        if (VoronoiSolution == null) return;
        //Draw input point
        Handles.color = Color.yellow;
        foreach (var inputPoint in VoronoiSolution.InputPoints.Values)
        {
            var position = new Vector3(inputPoint.X, inputPoint.Y);
            Handles.DrawSolidDisc(position, Vector3.forward, inputPointRadius);
        }
        //Draw input segment
        foreach (var inputSegment in VoronoiSolution.InputSegments.Values)
        {
            var startPoint = new Vector3(inputSegment.Start.X, inputSegment.Start.Y);
            var endPoint = new Vector3(inputSegment.End.X, inputSegment.End.Y);
            Handles.DrawSolidDisc(startPoint, Vector3.forward, inputPointRadius);
            Handles.DrawSolidDisc(endPoint, Vector3.forward, inputPointRadius);
            Handles.DrawLine(startPoint, endPoint);
        }

        //Draw ouput edge and vertex
        Handles.color = Color.blue;
        foreach (var edge in VoronoiSolution.Edges.Values)
        {
            DrawEdge(edge);
        }

        //Draw Nearest Obstacle Point
        if (drawNearestObstaclePoints)
        {
            foreach (var edge in VoronoiSolution.Edges.Values)
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

            var path = shortestPath.ConvertAll(x => new Vector3(x.X, x.Y)).ToArray();
            Handles.color = Color.red;
            Handles.DrawPolyLine(path);
        }
    }
    int CrossProduct(Point a,Point b, Point c)
    {
        int ax = b.X - a.X;
        int ay = b.Y - a.Y;
        int bx = c.X - a.X;
        int by = c.Y - a.Y;
        return bx * ay - ax * by;
    }


    List<Point> GetShortestPath(List<Point> portalsLeft, List<Point> portalsRight)
    {
        List<Point> path = new List<Point>();
        if (portalsLeft.Count == 0) return path;
        Point portalApex, portalLeft, portalRight;
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
            if (CrossProduct(portalApex,portalRight,right) <= 0)
            {
                if(portalApex.Equals(portalRight) || CrossProduct(portalApex, portalLeft, right) > 0)
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
            if (CrossProduct(portalApex, portalLeft, left) >= 0)
            {
                if (portalApex.Equals(portalLeft) || CrossProduct(portalApex, portalRight, left) < 0)
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
    void ComputePortals(List<Point> portalsLeft, List<Point> portalsRight)
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
                var start =VoronoiSolution.Vertices[ edge.Start];
                var point = new Point(start);
                portalsLeft.Add(point);
                portalsRight.Add(point);
            }
            if (i == edgeList.Count - 1)
            {
                var end = VoronoiSolution.Vertices[edge.End];
                var point = new Point(end);
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
    void DrawPortal(Point begin, Point end)
    {
        Handles.color = Color.white;
        Handles.DrawLine(new Vector3(begin.X,begin.Y),new Vector3(end.X,end.Y));

    }
    void DrawObstaclePoint(Edge edge)
    {
        if (!edge.IsFinite || !edge.IsPrimary) return ;
        var startVertex = VoronoiSolution.Vertices[edge.Start];
        var endVertex = VoronoiSolution.Vertices[edge.End];
        var begin = new Vector3((int)startVertex.X, (int)startVertex.Y);
        var obsLeft = new Vector3(edge.LeftObstacleStart.X, edge.LeftObstacleStart.Y);
        var obsRight = new Vector3(edge.RightObstacleStart.X, edge.RightObstacleStart.Y);

        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

        begin = new Vector3((int)endVertex.X, (int)endVertex.Y);
        obsLeft = new Vector3(edge.LeftObstacleEnd.X, edge.LeftObstacleEnd.Y);
        obsRight = new Vector3(edge.RightObstacleEnd.X, edge.RightObstacleEnd.Y);
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
    void DrawEdge(Edge outputSegment)
    {
        //if (!outputSegment.IsFinite) return;
        if (!outputSegment.IsFinite || !outputSegment.IsPrimary)
            return;
        Vertex start = VoronoiSolution.Vertices[outputSegment.Start];
        Vertex end = VoronoiSolution.Vertices[outputSegment.End];

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
            List<Vertex> discretizedEdge = VoronoiSolution.SampleCurvedEdge(outputSegment, 10);
            for (int i = 1; i < discretizedEdge.Count; i++)
            {
                float X1 = (float)discretizedEdge[i - 1].X;
                float Y1 = (float)discretizedEdge[i - 1].Y;
                float X2 = (float)discretizedEdge[i].X;
                float Y2 = (float)discretizedEdge[i].Y;
                Handles.color = Color.blue;
                Handles.DrawLine(new Vector3(X1, Y1), new Vector3(X2, Y2));
            }
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
                segments.Add(new Segment(new Point(i, j), new Point(i, j + 1)));
                //segments.Add(new Segment(new Point(i, j), new Point(i + 1, j)));
                //segments.Add(new Segment(new Point(i, j), new Point(i + 1, j + 1)));
            }
        }
        return segments;
    }

    void ConstructAndMeasure(ref List<Point> inputPoints, ref List<Segment> inputSegments)
        {
            Debug.Log(String.Format("Testing with {0} points and {1} segments", inputPoints.Count, inputSegments.Count));
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            using (BoostVoronoi bv = new BoostVoronoi())
            {
                foreach (var point in inputPoints)
                    bv.AddPoint(point.X, point.Y);

                foreach (var segment in inputSegments)
                    bv.AddSegment(segment.Start.X, segment.Start.Y, segment.End.X, segment.End.Y);



                bv.Construct();

                // Stop timing.
                stopwatch.Stop();
                Debug.Log(String.Format("Vertices: {0}, Edges: {1}, Cells: {2}", bv.CountVertices, bv.CountEdges, bv.CountCells));
                Debug.Log("Time elapsed:" + stopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss"));

                //bv.Clear();
            }
            inputPoints.Clear();
            inputSegments.Clear();


        }
    
}

