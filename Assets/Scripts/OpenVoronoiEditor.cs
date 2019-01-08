using System;
using System.Collections;
using System.Collections.Generic;
using OpenVoronoiCSharp;
using OpenVoronoiCSharp.Internals;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(OpenVoronoi))]
public class OpenVoronoiEditor : Editor
{
    //private VoronoiDiagram vd;
    public float vWidth = 0.01f;
    VoronoiDiagram vd;
    Rect box;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        vWidth = EditorGUILayout.FloatField("Vertex Width",vWidth);

        if (GUILayout.Button("Bake"))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            float smallSize = 3f;
            float largeSize = 10f;
            vd = new VoronoiDiagram(10);
            var v1 = vd.insert_point_site(new Point(-smallSize, smallSize));
            var v2 = vd.insert_point_site(new Point(smallSize, smallSize));
            var v3 = vd.insert_point_site(new Point(smallSize, -smallSize));
            var v4 = vd.insert_point_site(new Point(2*smallSize, -smallSize));
            var v5 = vd.insert_point_site(new Point(2*smallSize, -2*smallSize));
            var v6 = vd.insert_point_site(new Point(-smallSize, -2*smallSize));
            var v11 = vd.insert_point_site(new Point(-largeSize, largeSize));
            var v22 = vd.insert_point_site(new Point(largeSize, largeSize));
            var v33 = vd.insert_point_site(new Point(largeSize, -largeSize));
            var v44 = vd.insert_point_site(new Point(-largeSize, -largeSize));

            box = new Rect(-largeSize,-largeSize,largeSize*2+0.1f,largeSize*2+0.1f);

            vd.insert_line_site(v1, v2);
            vd.insert_line_site(v2, v3);
            vd.insert_line_site(v3, v4);
            vd.insert_line_site(v4, v5);
            vd.insert_line_site(v5, v6);
            vd.insert_line_site(v6, v1);
            vd.insert_line_site(v11, v22);
            vd.insert_line_site(v22, v33);
            vd.insert_line_site(v33, v44);
            vd.insert_line_site(v44, v11);

            vd.filter(new MedialAxisFilter());

            watch.Stop();
            Debug.Log(watch.ElapsedMilliseconds);
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
        if (!box.Contains(new Vector2((float)src_p.x, (float)src_p.y))||
            !box.Contains(new Vector2((float)trg_p.x, (float)trg_p.y)) )
            return;


        List<Point> points = new List<Point>();
        Handles.color = Color.white;

        if (e.type == EdgeType.LINESITE)
        {
            Handles.color = Color.blue;
        }
        if (e.type == EdgeType.SEPARATOR ||
            e.type == EdgeType.LINE ||
            e.type == EdgeType.LINELINE ||
            e.type == EdgeType.LINESITE ||
            e.type == EdgeType.OUTEDGE||
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
        for (int i = 0; i < line.Length; i++)
        {
            line[i] = new Vector3((float)points[i].x, (float)points[i].y);
        }
        Handles.DrawPolyLine(line);
    }
    void DrawVertex(Vertex v)
    {
        //if (v.type != VertexType.SEPPOINT) return;
        Point p = v.position;
        if (!box.Contains(new Vector2((float)p.x, (float)p.y))) return;

        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(new Vector3((float)p.x, (float)p.y), new Vector3(0, 0, 1), vWidth * 1.5f);
        Handles.Label(new Vector3((float)p.x, (float)p.y), "x:"+p.x + "\n" +"y:"+ p.y);
        if (v.status == VertexStatus.NEW)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(new Vector3((float)p.x, (float)p.y), new Vector3(0, 0, 1), vWidth * 2f);
        }
        else if (v.status == VertexStatus.IN)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(new Vector3((float)p.x, (float)p.y), new Vector3(0, 0, 1), vWidth * 3f);
        }
    }
}
