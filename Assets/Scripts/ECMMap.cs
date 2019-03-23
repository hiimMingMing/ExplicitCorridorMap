using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECMMap : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;
    public Transform Obstacles;
    public Transform DynamicObstacle;

    public ECM ecm;
    public List<float> AgentRadiusList;

    void Awake()
    {
        var obsList = new List<Obstacle>();
        foreach (Transform obs in Obstacles)
        {
            obsList.Add(new Obstacle(Geometry.ConvertToRect(obs)));
        }
        ecm = new ECM(obsList, new Obstacle(new RectInt(0, 0, 500, 500)));
        ecm.Construct();
        ecm.AddAgentRadius(AgentRadiusList);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region DrawGizmos
    [HideInInspector]
    public bool drawGraph = true;
    [HideInInspector]
    public float inputPointRadius = 2f;
    [HideInInspector]
    public float outputPointRadius = 2f;
    [HideInInspector]
    public bool drawNearestObstaclePoints = false;
    [HideInInspector]
    public bool drawShortestPath = false;
    [HideInInspector]
    public int obstacleToDelete = 0;

    List<Vector2> shortestPath = null;
    List<Vector2> portalsLeft;
    List<Vector2> portalsRight;
    List<Edge> selectedEdge;
    List<Segment> segments = new List<Segment>();
    List<List<Vector2>> curveEdges = new List<List<Vector2>>();
    public void Bake()
    {
        //populate segment
        var obstacles = new List<Obstacle>();
        foreach (Transform obs in Obstacles)
        {
            obstacles.Add(new Obstacle(Geometry.ConvertToRect(obs)));
        }

        ecm = new ECM(obstacles, new Obstacle(new RectInt(0, 0, 500, 500)));
        ecm.Construct();
        ComputeCurveEdge();
        foreach (var r in AgentRadiusList)
        {
            ecm.AddAgentRadius(r);

        }
        shortestPath = PathFinding.FindPathDebug(ecm, StartPoint.position, EndPoint.position, out portalsLeft, out portalsRight);
    }
    public void AddObstacle()
    {
        if (ecm == null)
        {
            Debug.Log("Ecm must be baked");
        }
        else
        {
            ecm.AddPolygonDynamic(new Obstacle(Geometry.ConvertToRect(DynamicObstacle)));
            ComputeCurveEdge();
            shortestPath = PathFinding.FindPathDebug(ecm, StartPoint.position, EndPoint.position, out portalsLeft, out portalsRight);
        }
    }
    public void DeleteObstacle()
    {
        if (ecm == null)
        {
            Debug.Log("Ecm must be baked");
        }
        else
        {
            ecm.DeletePolygonDynamic(obstacleToDelete);
            ComputeCurveEdge();
            shortestPath = PathFinding.FindPathDebug(ecm, StartPoint.position, EndPoint.position, out portalsLeft, out portalsRight);
        }
    }
    void OnDrawGizmos()
    {

        //Draw input point
        if (ecm == null || !drawGraph) return;
        
        //Draw input segment
        Gizmos.color = Color.yellow;
        foreach (var inputSegment in ecm.InputSegments.Values)
        {
            var startPoint = new Vector3(inputSegment.Start.x, inputSegment.Start.y);
            var endPoint = new Vector3(inputSegment.End.x, inputSegment.End.y);

            Gizmos.DrawSphere(startPoint, inputPointRadius);
            Gizmos.DrawSphere(endPoint, inputPointRadius);
            Gizmos.DrawLine(startPoint, endPoint);
        }

        //Draw ouput edge and vertex
        foreach (var vertex in ecm.Vertices.Values)
        {
            DrawVertex(vertex);
            foreach (var edge in vertex.Edges)
            {
                DrawEdge(edge);
            }
        }
        Gizmos.color = Color.blue;
        foreach (var l in curveEdges)
        {
            DrawPolyLine(l);
        }
        //Draw Nearest Obstacle Point
        if (drawNearestObstaclePoints)
        {
            foreach (var edge in ecm.Edges.Values)
            {
                DrawObstaclePoint(edge);
            }
        }
        if (shortestPath != null && drawShortestPath)
        {
            Gizmos.color = Color.red;
            DrawPolyLine(shortestPath);
        }
        if (portalsLeft != null && drawShortestPath)
        {
            for (int i = 0; i < portalsLeft.Count; i++)
            {
                DrawPortal(portalsLeft[i], portalsRight[i]);
            }
        }
    }




    void DrawPortal(Vector2 begin, Vector2 end)
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(begin, end);

    }
    void DrawObstaclePoint(Edge edge)
    {
        var startVertex = edge.Start.Position;
        var endVertex = edge.End.Position;
        var begin = startVertex;
        var obsLeft = edge.LeftObstacleOfStart;
        var obsRight = edge.RightObstacleOfStart;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(begin, obsLeft);
        Gizmos.DrawLine(begin, obsRight);

        begin = endVertex;
        obsLeft = edge.LeftObstacleOfEnd;
        obsRight = edge.RightObstacleOfEnd;
        Gizmos.DrawLine(begin, obsLeft);
        Gizmos.DrawLine(begin, obsRight);

    }
    void DrawObstaclePointProperty(Edge edge, int index)
    {
        var startVertex = edge.Start.Position;
        var endVertex = edge.End.Position;
        var begin = startVertex;
        var obsLeft = edge.EdgeProperties[index].LeftObstacleOfStart;
        var obsRight = edge.EdgeProperties[index].RightObstacleOfStart;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(begin, obsLeft);
        Gizmos.DrawLine(begin, obsRight);

        begin = endVertex;
        obsLeft = edge.EdgeProperties[index].LeftObstacleOfEnd;
        obsRight = edge.EdgeProperties[index].RightObstacleOfEnd;
        Gizmos.DrawLine(begin, obsLeft);
        Gizmos.DrawLine(begin, obsRight);
    }
    void DrawVertex(Vertex vertex)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vertex.Position, outputPointRadius);
    }
    void DrawEdge(Edge edge)
    {

        if (edge.IsLinear)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(edge.Start.Position, edge.End.Position);
        }
    }

    void ComputeCurveEdge()
    {
        curveEdges.Clear();
        foreach (var vertex in ecm.Vertices.Values)
        {
            foreach (var e in vertex.Edges)
            {
                if (!e.IsLinear)
                {
                    List<Vector2> discretizedEdge = ecm.SampleCurvedEdge(e, 10);
                    curveEdges.Add(discretizedEdge);
                }
            }
        }
    }
    void DrawPolyLine(List<Vector2> l)
    {
        for (int i = 0; i < l.Count - 1; i++)
        {
            Gizmos.DrawLine(l[i], l[i + 1]);
        }
    }
    #endregion
}
