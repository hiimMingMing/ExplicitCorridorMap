﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OpenVoronoiCSharp;
[CustomEditor(typeof(OpenVoronoi))]
public class OpenVoronoiEditor : Editor
{
    private VoronoiDiagram vd;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        if (GUILayout.Button("Bake"))
        {
            vd = new VoronoiDiagram();
            Vertex v1 = vd.insert_point_site(new Point(-0.4, -0.2));
            Vertex v2 = vd.insert_point_site(new Point(0, 0.4));
            Vertex v3 = vd.insert_point_site(new Point(0.4, -0.2));
            //vd.insert_line_site(v1, v2);
            vd.insert_line_site(v2, v3);
            //vd.insert_line_site(v3, v1);
            Debug.Log(vd.num_vertices() + " " + vd.num_faces());
        }
    }
    void OnSceneGUI()
    {
        if (vd == null) return;
        HalfEdgeDiagram g = vd.get_graph_reference(); ;
        foreach (Edge e in g.edges)
        {
            if (e.valid)
            {
                DrawEdge(e);
            }
        }
        foreach (Vertex v in g.vertices)
        {
            DrawVertex(v);
        }
    }
    void DrawEdge(Edge e)
    {
        Vertex src = e.source;
        Vertex trg = e.target;
        Point src_p = src.position;
        Point trg_p = trg.position;


        List<Point> points = new List<Point>();
        if (e.type == EdgeType.SEPARATOR ||
            e.type == EdgeType.LINE ||
            e.type == EdgeType.LINESITE ||
            e.type == EdgeType.OUTEDGE ||
            e.type == EdgeType.LINELINE ||
            e.type == EdgeType.PARA_LINELINE)
        {
            // edge drawn as two points
            points.Add(src_p);
            points.Add(trg_p);
        }
        else if (e.type == EdgeType.PARABOLA)
        {
            double t_src = src.dist();
            double t_trg = trg.dist();
            double t_min = Math.Min(t_src, t_trg);
            double t_max = Math.Max(t_src, t_trg);
            int nmax = 40;
            for (int n = 0; n < nmax; n++)
            {
                double t = t_min + ((t_max - t_min) / ((nmax - 1) * (nmax - 1))) * n * n;
                points.Add(e.point(t));
            }
        }
        Vector3[] line = new Vector3[points.Count];
        for(int i = 0; i < line.Length; i++)
        {
            line[i] = new Vector3((float)points[i].x, (float)points[i].y);
        }
        Handles.color = Color.white;
        Handles.DrawPolyLine(line);
    }
    void DrawVertex(Vertex v)
    {
        Point p = v.position;
        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(new Vector3((float)p.x, (float)p.y), new Vector3(0, 0, 1), 0.01f);
        
    }
}
