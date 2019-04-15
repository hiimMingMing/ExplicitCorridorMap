using DebugUtils;
using ExplicitCorridorMap;
using ExplicitCorridorMap.Maths;
using RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obstacle = ExplicitCorridorMap.Obstacle;
using Vector2 = UnityEngine.Vector2;

public class ECMMap : MonoBehaviour
{
    public Transform ObstaclesTransform;
    public Transform GroundPlane;
    public List<float> AgentRadiusList;
    public bool Grouping = false;
    public ECM ECMGraph { get; protected set; }
    private List<Obstacle> Obstacles;

    public Dictionary<int, GameAgent> AgentMap;

    void Awake()
    {
        Obstacles = new List<Obstacle>();
        AgentMap = new Dictionary<int, GameAgent>();
        foreach (Transform obs in ObstaclesTransform)
        {
            Obstacles.Add(Geometry.ConvertToObstacle(obs));
        }
        var mesh = GroundPlane.transform.GetComponent<MeshRenderer>();
        float width = mesh.bounds.size.x;
        float height = mesh.bounds.size.z;
        ECMGraph = new ECM(Obstacles, new Obstacle(new RectInt((int)(GroundPlane.transform.position.z - (width / 2)), (int)(GroundPlane.transform.position.x - (height / 2)), (int)width, (int)height)));
        ECMGraph.Construct();
        ECMGraph.AddAgentRadius(AgentRadiusList);
#if UNITY_EDITOR
        ComputeCurveEdge();
#endif
        //RVO
        Simulator.Instance.setTimeStep(1f);
        Simulator.Instance.setAgentDefaults(200.0f, 50, 0.1f, 0.05f, 10.0f, 10.0f, new RVO.Vector2(0.0f, 0.0f));
        foreach (Transform obs in ObstaclesTransform)
        {
            Simulator.Instance.addObstacle(Geometry.ConvertToListOfLine(obs));
        }
        Simulator.Instance.processObstacles();
    }

    private void FixedUpdate()
    {
        Simulator.Instance.doStep();
    }

    public void FindPathGroup( List<GameAgent> AgentGroup, Vector2 finalTarget)
    {
        if (!Grouping) throw new Exception("Cannot call is function when ECMMap.Grouping is off");
        var gh = new GroupHandler(ECMGraph, AgentGroup);
        gh.FindPath(finalTarget);
    }

#if UNITY_EDITOR
    #region Draw
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
    [HideInInspector]
    public List<Vector2> shortestPath = null;
    //List<Vector2> portalsLeft;
    //List<Vector2> portalsRight;
    List<Edge> selectedEdge;
    List<Segment> segments = new List<Segment>();
    List<List<Vector2>> curveEdges = new List<List<Vector2>>();
    void OnDrawGizmos()
    {
        //Draw input point
        if (ECMGraph == null || !drawGraph) return;

        //Draw input segment
        Gizmos.color = Color.yellow;
        foreach (var inputSegment in ECMGraph.InputSegments.Values)
        {
            var startPoint = new Vector2(inputSegment.Start.x, inputSegment.Start.y);
            var endPoint = new Vector2(inputSegment.End.x, inputSegment.End.y);

            Gizmos.DrawSphere(startPoint.To3D(), inputPointRadius);
            Gizmos.DrawSphere(endPoint.To3D(), inputPointRadius);
            Gizmos.DrawLine(startPoint.To3D(), endPoint.To3D());
        }

        //Draw ouput edge and vertex
        foreach (var vertex in ECMGraph.Vertices.Values)
        {
            DrawUtils.DrawVertex(vertex);
            foreach (var edge in vertex.Edges)
            {
                DrawUtils.DrawEdge(edge);
            }
        }
        Gizmos.color = Color.blue;
        foreach (var l in curveEdges)
        {
            DrawUtils.DrawPolyLine(l);
        }
        //Draw Nearest Obstacle Point
        if (drawNearestObstaclePoints)
        {
            foreach (var edge in ECMGraph.Edges.Values)
            {
                DrawUtils.DrawObstaclePoint(edge);
            }
        }
        if (shortestPath != null && drawShortestPath)
        {
            Gizmos.color = Color.red;
            DrawUtils.DrawPolyLine(shortestPath);
        }
    }




    

    public void ComputeCurveEdge()
    {
        curveEdges.Clear();
        foreach (var vertex in ECMGraph.Vertices.Values)
        {
            foreach (var e in vertex.Edges)
            {
                if (!e.IsLinear && !e.IsTwin)
                {
                    List<Vector2> discretizedEdge = SampleCurvedEdge(ECMGraph, e, 10);
                    curveEdges.Add(discretizedEdge);
                }
            }
        }
    }
    /// <summary>
    /// Generate a polyline representing a curved edge.
    /// </summary>
    /// <param name="edge">The curvy edge.</param>
    /// <param name="max_distance">The maximum distance between two vertex on the output polyline.</param>
    /// <returns></returns>
    public List<Vector2> SampleCurvedEdge(ECM ECMGraph, Edge edge, float max_distance)
    {
        //test
        //return new List<Vector2>() { edge.Start.Position, edge.End.Position };

        Edge pointCell = null;
        Edge lineCell = null;

        //Max distance to be refined
        if (max_distance <= 0)
            throw new Exception("Max distance must be greater than 0");

        Vector2Int pointSite;
        Segment segmentSite;

        Edge twin = edge.Twin;

        if (edge.ContainsSegment == true && twin.ContainsSegment == true)
            return new List<Vector2>() { edge.Start.Position, edge.End.Position };

        if (edge.ContainsPoint)
        {
            pointCell = edge;
            lineCell = twin;
        }
        else
        {
            lineCell = edge;
            pointCell = twin;
        }

        pointSite = ECMGraph.RetrieveInputPoint(pointCell);
        segmentSite = ECMGraph.RetrieveInputSegment(lineCell);

        List<Vector2> discretization = new List<Vector2>(){
                edge.Start.Position,
                edge.End.Position
            };

        if (edge.IsLinear)
            return discretization;


        return ParabolaComputation.Densify(
            new Vector2(pointSite.x, pointSite.y),
            new Vector2(segmentSite.Start.x, segmentSite.Start.y),
            new Vector2(segmentSite.End.x, segmentSite.End.y),
            discretization[0],
            discretization[1],
            max_distance,
            0
        );
    }
    #endregion

#endif
}
