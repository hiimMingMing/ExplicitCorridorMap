using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpBoostVoronoi;
using SharpBoostVoronoi.Input;
using System.Runtime.InteropServices;
using System;

public class OpenVoronoi : MonoBehaviour
{


    public void Start()
    {

        Point p0 = new Point(200, 250);
        Point p1 = new Point(400, 250);
        List<Point> inputPoints = new List<Point>();
        List<Segment> segments = new List<Segment> {
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
        var VoronoiSolution = new BoostVoronoi();

        //Populate the input
        foreach (var point in inputPoints)
        {
            //InputPoints.Add(point);
            VoronoiSolution.AddPoint(point.X, point.Y);
        }

        foreach (var segment in segments)
        {
            //InputSegments.Add(segment);
            VoronoiSolution.AddSegment(segment.Start.X, segment.Start.Y, segment.End.X, segment.End.Y);
        }

        //Construct
        VoronoiSolution.Construct();

        var x = VoronoiSolution.GetCell(1);
        var n = x;
        Debug.Log("VoronoiEdge:" + VoronoiSolution.CountEdges);
        Debug.Log("VoronoiVertex:" + VoronoiSolution.CountVertices);
        Debug.Log("VoronoiCell:" + VoronoiSolution.CountCells);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
