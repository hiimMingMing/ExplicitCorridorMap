using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SharpBoostVoronoi;
using SharpBoostVoronoi.Input;
using SharpBoostVoronoi.Output;
[CustomEditor(typeof(MedialAxis))]
public class MedialAxisEditor : Editor
{
    BoostVoronoi VoronoiSolution { get; set; }
    float inputPointRadius = 12f;
    float outputPointRadius = 6f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", inputPointRadius);
        outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", outputPointRadius);

        if (GUILayout.Button("Bake"))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

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
    }
    void OnSceneGUI()
    {
        if (VoronoiSolution == null) return;
        //Draw input point
        Handles.color = Color.yellow;
        foreach (var inputPoint in VoronoiSolution.InputPoints.Values)
        {
            var position = new Vector3(inputPoint.X, inputPoint.Y);
            Handles.DrawSolidDisc(position , Vector3.forward, inputPointRadius);
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
                var startPoint = new Vector3((float)start.X,(float) start.Y);
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
                    Handles.DrawLine(new Vector3(X1,Y1),new Vector3(X2,Y2));
                }
                var startPoint = new Vector3((float)start.X, (float)start.Y);
                var endPoint = new Vector3((float)end.X, (float)end.Y);
                Handles.DrawSolidDisc(startPoint, Vector3.forward, outputPointRadius);
                Handles.DrawSolidDisc(endPoint, Vector3.forward, outputPointRadius);
            }
        }
    }
}
