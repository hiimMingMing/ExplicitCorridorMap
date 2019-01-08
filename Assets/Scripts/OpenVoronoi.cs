using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenVoronoiCSharp;
public class OpenVoronoi : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VoronoiDiagram vd = new VoronoiDiagram();
        Vertex v1 = vd.insert_point_site(new Point(-0.4, -0.2));
        Vertex v2 = vd.insert_point_site(new Point(0, 0.4));
        Vertex v3 = vd.insert_point_site(new Point(0.4, -0.2));
        vd.insert_line_site(v1, v2);
        vd.insert_line_site(v2, v3);
        vd.insert_line_site(v3, v1);
        Debug.Log(vd.num_vertices() + " " + vd.num_faces());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void DrawVoronoi(VoronoiDiagram voronoiDiagram)
    {

    }
}
