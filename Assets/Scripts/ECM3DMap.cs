//using ExplicitCorridorMap;
//using ExplicitCorridorMap.Maths;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ECM3DMap : ECMMap
//{
  

//    void Awake()
//    {
//        var obsList = new List<Obstacle>();
//        foreach (Transform obs in Obstacles)
//        {
//            obsList.Add(new Obstacle(Geometry.ConvertToRect(obs)));
//        }

//        float width = Ground.transform.GetComponent<MeshRenderer>().bounds.size.x;
//        float height = Ground.transform.GetComponent<MeshRenderer>().bounds.size.z;
//        ecm = new ECM(obsList, new Obstacle(new RectInt((int)(Ground.transform.position.z-(width/2)), (int)(Ground.transform.position.x - (height / 2)), (int)width, (int)height)));
//        ecm.Construct();
//        ecm.AddAgentRadius(AgentRadiusList);
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    #region DrawGizmos

//    public GameObject Ground;
   
//    //List<Vector2> portalsLeft;
//    //List<Vector2> portalsRight;
//    private List<Edge> selectedEdge;
//    private List<Segment> segments = new List<Segment>();
//    private List<List<Vector2>> curveEdges = new List<List<Vector2>>();
//    public new void Bake()
//    {
//        //populate segment
//        var obstacles = new List<Obstacle>();
//        foreach (Transform obs in Obstacles)
//        {
//            obstacles.Add(new Obstacle(Geometry.ConvertToRect(obs)));
//        }


//        float width = Ground.transform.GetComponent<MeshRenderer>().bounds.size.x;
//        float height = Ground.transform.GetComponent<MeshRenderer>().bounds.size.z;
//        ecm = new ECM(obstacles, new Obstacle(new RectInt((int)(Ground.transform.position.z - (width / 2)), (int)(Ground.transform.position.x - (height / 2)), (int)width, (int)height)));
//        ecm.Construct();
//        ComputeCurveEdge();
//        foreach (var r in AgentRadiusList)
//        {
//            ecm.AddAgentRadius(r);

//        }
//        shortestPath = PathFinding.FindPath(ecm, 0, StartPoint.position.to2D(), EndPoint.position.to2D());
//    }
//    public new void AddObstacle()
//    {
//        if (ecm == null)
//        {
//            Debug.Log("Ecm must be baked");
//        }
//        else
//        {
//            ecm.AddPolygonDynamic(new Obstacle(Geometry.ConvertToRect(DynamicObstacle)));
//            ComputeCurveEdge();
//            shortestPath = PathFinding.FindPath(ecm, 0, StartPoint.position.to2D(), EndPoint.position.to2D());
//        }
//    }
//    public new void DeleteObstacle()
//    {
//        if (ecm == null)
//        {
//            Debug.Log("Ecm must be baked");
//        }
//        else
//        {
//            ecm.DeletePolygonDynamic(obstacleToDelete);
//            ComputeCurveEdge();
//            shortestPath = PathFinding.FindPath(ecm, 0, StartPoint.position.to2D(), EndPoint.position.to2D());
//        }
//    }
//    void OnDrawGizmos()
//    {

//        //Draw input point
//        if (ecm == null || !drawGraph) return;

//        //Draw input segment
//        Gizmos.color = Color.yellow;
//        foreach (var inputSegment in ecm.InputSegments.Values)
//        {
//            var startPoint = new Vector3(inputSegment.Start.x, inputSegment.Start.y).to3D();
//            var endPoint = new Vector3(inputSegment.End.x, inputSegment.End.y).to3D();

//            Gizmos.DrawSphere(startPoint, inputPointRadius);
//            Gizmos.DrawSphere(endPoint, inputPointRadius);
//            Gizmos.DrawLine(startPoint, endPoint);
//        }

//        //Draw ouput edge and vertex
//        foreach (var vertex in ecm.Vertices.Values)
//        {
//            DrawVertex(vertex);
//            foreach (var edge in vertex.Edges)
//            {
//                DrawEdge(edge);
//            }
//        }
//        Gizmos.color = Color.blue;
//        foreach (var l in curveEdges)
//        {
//            DrawPolyLine(l);
//        }
//        //Draw Nearest Obstacle Point
//        if (drawNearestObstaclePoints)
//        {
//            foreach (var edge in ecm.Edges.Values)
//            {
//                DrawObstaclePoint(edge);
//            }
//        }
//        if (shortestPath != null && drawShortestPath)
//        {
//            Gizmos.color = Color.red;
//            DrawPolyLine(shortestPath);
//        }
//        //if (portalsLeft != null && drawShortestPath)
//        //{
//        //    for (int i = 0; i < portalsLeft.Count; i++)
//        //    {
//        //        DrawPortal(portalsLeft[i], portalsRight[i]);
//        //    }
//        //}
//    }




//    void DrawPortal(Vector2 begin, Vector2 end)
//    {
//        Gizmos.color = Color.magenta;
//        Gizmos.DrawLine(begin.to3D(), end.to3D());
//    }
//    void DrawObstaclePoint(Edge edge)
//    {
//        var startVertex = edge.Start.Position;
//        var endVertex = edge.End.Position;
//        var begin = startVertex;
//        var obsLeft = edge.LeftObstacleOfStart;
//        var obsRight = edge.RightObstacleOfStart;

//        Gizmos.color = Color.green;
//        Gizmos.DrawLine(begin.to3D(), obsLeft.to3D());
//        Gizmos.DrawLine(begin.to3D(), obsRight.to3D());

//        begin = endVertex;
//        obsLeft = edge.LeftObstacleOfEnd;
//        obsRight = edge.RightObstacleOfEnd;
//        Gizmos.DrawLine(begin.to3D(), obsLeft.to3D());
//        Gizmos.DrawLine(begin.to3D(), obsRight.to3D());

//    }
//    void DrawObstaclePointProperty(Edge edge, int index)
//    {
//        var startVertex = edge.Start.Position;
//        var endVertex = edge.End.Position;
//        var begin = startVertex;
//        var obsLeft = edge.EdgeProperties[index].LeftObstacleOfStart;
//        var obsRight = edge.EdgeProperties[index].RightObstacleOfStart;

//        Gizmos.color = Color.green;
//        Gizmos.DrawLine(begin.to3D(), obsLeft.to3D());
//        Gizmos.DrawLine(begin.to3D(), obsRight.to3D());

//        begin = endVertex;
//        obsLeft = edge.EdgeProperties[index].LeftObstacleOfEnd;
//        obsRight = edge.EdgeProperties[index].RightObstacleOfEnd;
//        Gizmos.DrawLine(begin.to3D(), obsLeft.to3D());
//        Gizmos.DrawLine(begin.to3D(), obsRight.to3D());
//    }
//    void DrawVertex(Vertex vertex)
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawSphere(vertex.Position.to3D(), outputPointRadius);
//    }
//    void DrawEdge(Edge edge)
//    {
//        if (edge.IsLinear && !edge.IsTwin)
//        {
//            Gizmos.color = Color.blue;
//            Gizmos.DrawLine(edge.Start.Position.to3D(), edge.End.Position.to3D());
//        }
//    }

//    public new void ComputeCurveEdge()
//    {
//        curveEdges.Clear();
//        foreach (var vertex in ecm.Vertices.Values)
//        {
//            foreach (var e in vertex.Edges)
//            {
//                if (!e.IsLinear && !e.IsTwin)
//                {
//                    List<Vector2> discretizedEdge = SampleCurvedEdge(ecm, e, 10);
//                    curveEdges.Add(discretizedEdge);
//                }
//            }
//        }
//    }
//    void DrawPolyLine(List<Vector2> l)
//    {
//        for (int i = 0; i < l.Count - 1; i++)
//        {
//            Gizmos.DrawLine(l[i].to3D(), l[i + 1].to3D());
//        }
//    }
//    /// <summary>
//    /// Generate a polyline representing a curved edge.
//    /// </summary>
//    /// <param name="edge">The curvy edge.</param>
//    /// <param name="max_distance">The maximum distance between two vertex on the output polyline.</param>
//    /// <returns></returns>
//    public new List<Vector2> SampleCurvedEdge(ECM ecm, Edge edge, float max_distance)
//    {
//        //test
//        //return new List<Vector2>() { edge.Start.Position, edge.End.Position };

//        Edge pointCell = null;
//        Edge lineCell = null;

//        //Max distance to be refined
//        if (max_distance <= 0)
//            throw new Exception("Max distance must be greater than 0");

//        Vector2Int pointSite;
//        Segment segmentSite;

//        Edge twin = edge.Twin;

//        if (edge.ContainsSegment == true && twin.ContainsSegment == true)
//            return new List<Vector2>() { edge.Start.Position, edge.End.Position };

//        if (edge.ContainsPoint)
//        {
//            pointCell = edge;
//            lineCell = twin;
//        }
//        else
//        {
//            lineCell = edge;
//            pointCell = twin;
//        }

//        pointSite = ecm.RetrieveInputPoint(pointCell);
//        segmentSite = ecm.RetrieveInputSegment(lineCell);

//        List<Vector2> discretization = new List<Vector2>(){
//                edge.Start.Position,
//                edge.End.Position
//            };

//        if (edge.IsLinear)
//            return discretization;


//        return ParabolaComputation.Densify(
//            new Vector2(pointSite.x, pointSite.y),
//            new Vector2(segmentSite.Start.x, segmentSite.Start.y),
//            new Vector2(segmentSite.End.x, segmentSite.End.y),
//            discretization[0],
//            discretization[1],
//            max_distance,
//            0
//        );
//    }
//    #endregion
//}
