using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SharpBoostVoronoi;
using SharpBoostVoronoi.Input;
using SharpBoostVoronoi.Output;
using System;

[CustomEditor(typeof(MedialAxis))]
public class MedialAxisEditor : Editor
{
    BoostVoronoi VoronoiSolution { get; set; }
    float inputPointRadius = 12f;
    float outputPointRadius = 6f;
    int segmentCount = 100000;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", inputPointRadius);
        outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", outputPointRadius);

        if (GUILayout.Button("Bake"))
        {

            Point p0 = new Point(200, 250);
            Point p1 = new Point(400, 250);
            List<Point> InputPoints = new List<Point>();
            List<Segment> InputSegments = new List<Segment> {
                new Segment(new Point(0,0), new Point(0,500)),
                new Segment(new Point(0,0), new Point(500,0)),
                new Segment(new Point(500,0), new Point(500,500)),
                new Segment(new Point(0,500), new Point(500,500)),
                new Segment(new Point(50,50), new Point(50,450)),
                new Segment(new Point(50,50), new Point(450,50)),
                new Segment(new Point(450,50), new Point(450,450)),
                new Segment(new Point(50,450), new Point(450,450)),
                new Segment(new Point(50,50), p0),
                new Segment(new Point(50,450), p0),
                new Segment(p0,p1),
                new Segment(p1,new Point(50,50))
            };
            /*AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA*/
            //Point p0 = new Point(200, 250);
            //Point p1 = new Point(400, 250);

            //List<Point> InputPoints = new List<Point>();
            //List<Segment> InputSegments = new List<Segment> {
            //    new Segment(new Point(0,0), new Point(0,500)),
            //    new Segment(new Point(0,0), new Point(500,0)),
            //    new Segment(new Point(0,0), new Point(500,500)),
            //    new Segment(new Point(500,0), new Point(500,500)),
            //    new Segment(new Point(0,500), new Point(500,500)),
            //};
            /*AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA*/
            //List<Point> InputPoints = new List<Point>() { new Point(250, 250) };
            //List<Segment> InputSegments = new List<Segment> {
            //    new Segment(new Point(0,0), new Point(0,500)),
            //    new Segment(new Point(0,0), new Point(500,0)),
            //    new Segment(new Point(500,0), new Point(500,500)),
            //    new Segment(new Point(0,500), new Point(500,500)),
            //};
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            VoronoiSolution = new BoostVoronoi();
            foreach (var point in InputPoints)
            {
                VoronoiSolution.AddPoint(point.X, point.Y);
            }
            foreach (var segment in InputSegments)
            {
                VoronoiSolution.AddSegment(segment.Start.X, segment.Start.Y, segment.End.X, segment.End.Y);
            }
            VoronoiSolution.Construct();
            watch.Stop();
            Debug.Log("Voronoi Edge Count: " + VoronoiSolution.CountEdges);
            Debug.Log("Voronoi Vertex Count: " + VoronoiSolution.CountVertices);
            Debug.Log("Voronoi Cell Count: " + VoronoiSolution.CountCells);
            Debug.Log("Time: " + watch.ElapsedMilliseconds);
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
        for (long edgeIndex = 0; edgeIndex < VoronoiSolution.CountEdges; edgeIndex++)
        {
            Edge outputSegment = VoronoiSolution.GetEdge(edgeIndex);
            if (!outputSegment.IsFinite)
                continue;
            Vertex start = VoronoiSolution.GetVertex(outputSegment.Start);
            Vertex end = VoronoiSolution.GetVertex(outputSegment.End);

            if (outputSegment.IsLinear)
            {
                var startPoint = new Vector3((float)start.X, (float)start.Y);
                var endPoint = new Vector3((float)end.X, (float)end.Y);
                Handles.DrawSolidDisc(startPoint, Vector3.forward, outputPointRadius);
                Handles.DrawSolidDisc(endPoint, Vector3.forward, outputPointRadius);
                Handles.DrawLine(startPoint, endPoint);
            }
            else
            {
                List<Vertex> discretizedEdge = VoronoiSolution.SampleCurvedEdge(outputSegment, 2);
                for (int i = 1; i < discretizedEdge.Count; i++)
                {
                    float X1 = (float)discretizedEdge[i - 1].X;
                    float Y1 = (float)discretizedEdge[i - 1].Y;
                    float X2 = (float)discretizedEdge[i].X;
                    float Y2 = (float)discretizedEdge[i].Y;
                    Handles.DrawLine(new Vector3(X1, Y1), new Vector3(X2, Y2));
                }
                var startPoint = new Vector3((float)start.X, (float)start.Y);
                var endPoint = new Vector3((float)end.X, (float)end.Y);
                Handles.DrawSolidDisc(startPoint, Vector3.forward, outputPointRadius);
                Handles.DrawSolidDisc(endPoint, Vector3.forward, outputPointRadius);
            }
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

