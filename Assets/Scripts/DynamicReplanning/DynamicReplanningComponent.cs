using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExplicitCorridorMap.Maths;
using rvo = RVO;
namespace ExplicitCorridorMap
{
    public class DynamicReplanningComponent : MonoBehaviour
    {
        public GameObject obstaclePrefab;
        private ECMMap ecmMap;
        public bool usingODPA = false;
        public KeyCode defaultAddKeyCode;
        void Awake() { }
        void Start() {
            if (obstaclePrefab == null) {
                Debug.LogError("Please add default obstacle");
            }
            ecmMap = FindObjectOfType<ECMMap>();
            if (ecmMap==null) {
                Debug.LogError("Please create  EcmMap");
            }
        }

        public void addObstacle(GameObject obstaclePrefab, Vector3 position) {
          
            GameObject go = Instantiate(obstaclePrefab, new Vector3(position.x, 0, position.z), Quaternion.identity);
            var obsRect = Geometry.ConvertToRectInt(go.GetComponent<Transform>());
            var obs = new Obstacle(obsRect);
            obs.GameObject = go;
            ecmMap.ECMGraph.AddPolygonDynamic(obs);
            ecmMap.ComputeCurveEdge();
            rvo.Simulator.Instance.addObstacle(obs);
            //TODO change to effective way to add obstacle
            rvo.Simulator.Instance.processObstacles();
            //Handle the path
            foreach (var item in ecmMap.AgentMap)
            {
                GameAgent player = item.Value;
                if (player.WayPointList.Count > 0)
                {
                    var listAffectPath = ListAffectedPath(player.WayPointList, obsRect, player.CurrentWayPoint);
                    List<Vector2> newPath = new List<Vector2>();

                    if (!usingODPA)
                    {
                        newPath = DynamicFindPathByECMVer2(ecmMap, player.RadiusIndex, player.transform.position, player.WayPointList[player.WayPointList.Count - 1], listAffectPath);
                    }

                    if (newPath.Count > 0)
                    {
                        player.CurrentWayPoint = 0;
                        player.WayPointList = newPath;
                    }
                }
            }
        }
        void Update() {
            HandleDynamicEvent();
        }
        //GameAgents version
        public  void HandleDynamicEvent()
        {
           
            if (Input.GetKeyDown(defaultAddKeyCode))
            {
                //Add a dynamic obstacle
                Vector2 mousePosition;
                
                Vector3 _position = Vector3.zero;
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane basePlane = new Plane(Vector3.up, Vector3.zero);
                if (basePlane.Raycast(mouseRay, out float rayDistance))
                {
                    Vector3 temp = mouseRay.GetPoint(rayDistance);
                    _position = new Vector3(temp.x, temp.z, 0);
                }

                mousePosition = new Vector2(_position.x, _position.y);
                
                Vector3 position = new Vector3(mousePosition.x, 0, mousePosition.y);
                addObstacle(obstaclePrefab, position);
              
            
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Vector2 mousePosition;

                Vector3 _position = Vector3.zero;
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane basePlane = new Plane(Vector3.up, Vector3.zero);
                if (basePlane.Raycast(mouseRay, out float rayDistance))
                {
                    Vector3 temp = mouseRay.GetPoint(rayDistance);
                    _position = new Vector3(temp.x, temp.z, 0);
                }

                mousePosition = new Vector2(_position.x, _position.y);
                //Delete an obstacle
                
                int ID = FindObstacleID(ecmMap.ECMGraph, mousePosition);
                if (ID == -1) return;
                var deleteObs = ecmMap.ECMGraph.Obstacles[ID];
                Destroy(deleteObs.GameObject);
                var rvoID = deleteObs.RvoID;
                ecmMap.ECMGraph.DeletePolygonDynamic(ID);
                ecmMap.ComputeCurveEdge();
                rvo.Simulator.Instance.deleteObstacle(rvoID);
                rvo.Simulator.Instance.processObstacles();
                //Handle the path
                foreach (var item in ecmMap.AgentMap)
                {

                    GameAgent player = item.Value;
                    if (player.WayPointList.Count > 0)
                    {
                        List<Vector2> newPath;

                        newPath = PathFinding.FindPath(ecmMap.ECMGraph, player.RadiusIndex, player.transform.position, player.WayPointList[player.WayPointList.Count - 1]);

                        if (newPath.Count > 0)
                        {
                            player.CurrentWayPoint = 0;
                            player.WayPointList = newPath;
                        }
                    }
                }
            }
        }


        //Check if the obstacle is on the moving path or not
        public static List<Vector2> ListAffectedPath(List<Vector2> wayPointList, RectInt obstacle, int currentWayPoint)
        {
            List<Vector2> list = new List<Vector2>();

            Vector2 bottomLeft = obstacle.min;
            Vector2 bottomRight = new Vector2(obstacle.xMax, obstacle.yMin);
            Vector2 topRight = obstacle.max;
            Vector2 topLeft = new Vector2(obstacle.xMin, obstacle.yMax);

            for (int i = currentWayPoint - 1; i < wayPointList.Count - 1; i++)
            {
                var A = wayPointList[i];
                var B = wayPointList[i + 1];

                if (DRMath.IsIntersecting(A, B, bottomLeft, bottomRight) ||
                    DRMath.IsIntersecting(A, B, bottomRight, topRight) ||
                    DRMath.IsIntersecting(A, B, topRight, topLeft) ||
                    DRMath.IsIntersecting(A, B, topLeft, bottomLeft))
                {
                    list.Add(A);
                    list.Add(B);
                }
            }
            DRMath.RemoveDuplicate(list);

            if (list.Count > 0) Debug.Log(list.Count + " - Affected!!");
            else Debug.Log(list.Count + " - Unaffected!!");

            return list;
        }

        //Dynamic find the new path by rebuild ECMMap - Add obstacle case
        public static List<Vector2> DynamicFindPathByECM(ECMMap ecmmap, int radiusIndex, Vector2 startPosition, Vector2 endPosition, List<Vector2> listAffectPath, RectInt obstacle)
        {
            List<Vector2> newPath = new List<Vector2>();
            ecmmap.ECMGraph.AddPolygonDynamic(new Obstacle(obstacle));
            //ecmmap.ComputeCurveEdge();
            if (listAffectPath.Count > 0)
                return PathFinding.FindPath(ecmmap.ECMGraph, radiusIndex, startPosition, endPosition);

            return newPath;
        }

        public static List<Vector2> DynamicFindPathByECMVer2(ECMMap ecmmap, int radiusIndex, Vector2 startPosition, Vector2 endPosition, List<Vector2> listAffectPath)
        {
            List<Vector2> newPath = new List<Vector2>();

            if (listAffectPath.Count > 0)
                return PathFinding.FindPath(ecmmap.ECMGraph, radiusIndex, startPosition, endPosition);

            return newPath;
        }

        //Dynamic find the new path by rebuild ECMMap - Delete obstacle case
        public static List<Vector2> DynamicFindPathByECM2(ECMMap ecmmap, int radiusIndex, Vector2 startPosition, Vector2 endPosition, RectInt obstacle)
        {

            var shortestPath = PathFinding.FindPath(ecmmap.ECMGraph, 0, startPosition, endPosition);

            return shortestPath;
        }

        //Dynamic find the new path using ODPA* - Add obstacle case
        //public static List<Vector2> DynamicFindPath(ECM ecm, int radiusIndex, Vector2 startPosition, Vector2 endPosition, List<Vector2> listAffectPath, RectInt obstacle)
        //{
        //    return null;
        //}

        //Dynamic find the new path using ODPA* - Delete obstacle case
        //public static List<Vector2> DynamicFindPath2(ECM ecm, int radiusIndex, Vector2 startPosition, Vector2 endPosition, RectInt obstacle)
        //{
        //    List<Vector2> intersectPoint = new List<Vector2>();
        //    List<Vector2> shortertPath = new List<Vector2>();
        //    List<Vector2> portalsLeft, portalsRight;
        //    List<Vector2> portalsLeft2, portalsRight2;

        //    Vector2 bottomLeft = obstacle.min;
        //    Vector2 bottomRight = new Vector2(obstacle.xMax, obstacle.yMin);
        //    Vector2 topRight = obstacle.max;
        //    Vector2 topLeft = new Vector2(obstacle.xMin, obstacle.yMax);

        //    float oldFScore = 0, newFScore = 0;
        //    List<Edge> oldEdgeList, newEdgeList, newEdgeList2;

        //    var startEdge = ecm.GetNearestEdge(ref startPosition, radiusIndex);
        //    var endEdge = ecm.GetNearestEdge(ref endPosition, radiusIndex);

        //    oldEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, endEdge.Start, endEdge.End, startPosition, endPosition, out oldFScore);

        //    //Find intersectPoint
        //    if (DRMath.IsIntersecting(bottomLeft, bottomRight, startPosition, endPosition))
        //        intersectPoint.Add(DRMath.LineLineIntersection(bottomLeft, bottomRight, startPosition, endPosition));

        //    if (DRMath.IsIntersecting(bottomRight, topRight, startPosition, endPosition))
        //        intersectPoint.Add(DRMath.LineLineIntersection(bottomRight, topRight, startPosition, endPosition));

        //    if (DRMath.IsIntersecting(topRight, topLeft, startPosition, endPosition))
        //        intersectPoint.Add(DRMath.LineLineIntersection(topRight, topLeft, startPosition, endPosition));

        //    if (DRMath.IsIntersecting(topLeft, bottomLeft, startPosition, endPosition))
        //        intersectPoint.Add(DRMath.LineLineIntersection(topLeft, bottomLeft, startPosition, endPosition));

        //    ////
        //    if (intersectPoint.Count == 0) //The new path is still the old path
        //    {               
        //        Debug.Log("TH0: No change!");
        //        PathFinding.ComputePortals(radiusIndex, oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
        //        shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
        //    }
        //    else if (intersectPoint.Count == 1)
        //    {               
        //        Debug.Log("TH1: " + intersectPoint[0]);
        //        var tempPoint = intersectPoint[0];
        //        var midEdge = ecm.GetNearestEdge(ref tempPoint, radiusIndex);

        //        newEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, midEdge.Start, midEdge.End, startPosition, intersectPoint[0], out newFScore);
        //        float tempFScore = newFScore;
        //        newEdgeList2 = FindEdgePathFromVertexToVertex(ecm, midEdge.Start, midEdge.End, endEdge.Start, endEdge.End, intersectPoint[0], endPosition, out newFScore);
        //        newFScore += tempFScore;

        //        Debug.Log(oldFScore + " - " + newFScore);
        //        if (oldFScore < newFScore) //The new path is still the old path
        //        {
        //            Debug.Log("No change!!!");
        //            PathFinding.ComputePortals(radiusIndex, oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
        //            shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
        //        }
        //        else //Change the path
        //        {
        //            Debug.Log("Change!!!");
        //            PathFinding.ComputePortals(radiusIndex, newEdgeList, startPosition, intersectPoint[0], out portalsLeft, out portalsRight);
        //            PathFinding.ComputePortals(radiusIndex, newEdgeList2, intersectPoint[0], endPosition, out portalsLeft2, out portalsRight2);
        //            shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
        //            shortertPath.AddRange(PathFinding.GetShortestPath(portalsLeft2, portalsRight2));
        //        }
        //    }
        //    else if (intersectPoint.Count == 2)
        //    {
        //        Debug.Log("TH2: " + intersectPoint[0] + " - " + intersectPoint[1]);
        //        var tempPoint = intersectPoint[0];
        //        var tempPoint2 = intersectPoint[1];
        //        var midEdge1 = ecm.GetNearestEdge(ref tempPoint, radiusIndex);
        //        var midEdge2 = ecm.GetNearestEdge(ref tempPoint2, radiusIndex);

        //        if (PathFinding.HeuristicCost(startPosition, intersectPoint[0]) > PathFinding.HeuristicCost(startPosition, intersectPoint[1]))
        //            intersectPoint.Reverse();

        //        newEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, midEdge1.Start, midEdge1.End, startPosition, intersectPoint[0], out newFScore);
        //        float tempFScore = newFScore;
        //        newEdgeList2 = FindEdgePathFromVertexToVertex(ecm, midEdge2.Start, midEdge2.End, endEdge.Start, endEdge.End, intersectPoint[1], endPosition, out newFScore);
        //        newFScore += (PathFinding.HeuristicCost(intersectPoint[0], intersectPoint[1]) + tempFScore);

        //        Debug.Log(oldFScore + " - " + newFScore);
        //        if (oldFScore < newFScore) //The new path is still the old path
        //        {
        //            Debug.Log("No change!!!");
        //            PathFinding.ComputePortals(radiusIndex, oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
        //            shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
        //        }
        //        else //Change the path
        //        {
        //            Debug.Log("Change!!!");
        //            PathFinding.ComputePortals(radiusIndex, newEdgeList, startPosition, intersectPoint[0], out portalsLeft, out portalsRight);
        //            PathFinding.ComputePortals(radiusIndex, newEdgeList2, intersectPoint[1], endPosition, out portalsLeft2, out portalsRight2);
        //            shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
        //            shortertPath.AddRange(PathFinding.GetShortestPath(portalsLeft2, portalsRight2));
        //        }
        //    }
        //    return shortertPath;
        //}

        //private static List<Edge> FindEdgePathFromVertexToVertex(ECM graph, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition, out float finalFScore)
        //{
        //    var path = FindPathFromVertexToVertex(graph, start1, start2, end1, end2, startPosition, endPosition, out finalFScore);
        //    var edgeList = new List<Edge>();

        //    for (int i = path.Count - 1; i > 0; i--)
        //    {
        //        foreach (var edge in path[i].Edges)
        //        {
        //            var end = edge.End;
        //            if (end.Equals(path[i - 1])) edgeList.Add(edge);
        //        }
        //    }
        //    return edgeList;
        //}

        //public static List<Vertex> FindPathFromVertexToVertex(ECM graph, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition, out float finalFScore)
        //{
        //    var openSet = new HashSet<Vertex>();
        //    openSet.Add(start1);
        //    openSet.Add(start2);
        //    var closeSet = new HashSet<Vertex>();
        //    var cameFrom = new Dictionary<Vertex, Vertex>();
        //    var gScore = new Dictionary<Vertex, float>();
        //    var fScore = new Dictionary<Vertex, float>();
        //    gScore[start1] = PathFinding.HeuristicCost(startPosition, start1.Position);
        //    fScore[start1] = PathFinding.HeuristicCost(start1.Position, endPosition);
        //    gScore[start2] = PathFinding.HeuristicCost(startPosition, start2.Position);
        //    fScore[start2] = PathFinding.HeuristicCost(start2.Position, endPosition);

        //    List<Vertex> result = new List<Vertex>();
        //    while (openSet.Count != 0)
        //    {
        //        var current = PathFinding.LowestFScore(openSet, fScore);
        //        if (current.Equals(end1) || current.Equals(end2))
        //        {
        //            if (current.Equals(end1)) finalFScore = fScore[end1];
        //            else finalFScore = fScore[end2];
        //            return PathFinding.RecontructPath(cameFrom, current);
        //        }

        //        openSet.Remove(current);
        //        closeSet.Add(current);

        //        foreach (var edge in current.Edges)
        //        {
        //            var neigborVertex = edge.End;
        //            if (closeSet.Contains(neigborVertex)) continue;
        //            var tentativeGScore = gScore[current] + PathFinding.HeuristicCost(current, neigborVertex);
        //            if (!openSet.Contains(neigborVertex)) openSet.Add(neigborVertex);
        //            else if (tentativeGScore >= gScore[neigborVertex]) continue;
        //            cameFrom[neigborVertex] = current;
        //            gScore[neigborVertex] = tentativeGScore;
        //            fScore[neigborVertex] = tentativeGScore + PathFinding.HeuristicCost(neigborVertex.Position, endPosition);
        //        }
        //    }
        //    finalFScore = 0;
        //    return result;
        //}

        public static int FindObstacleID(ECM ecm, Vector2 position)
        {
            var result = ecm.RTreeObstacle.RangeSearch(new Advanced.Algorithms.DataStructures.Rectangle(position));
            foreach (var o in result)
            {
                if (o.ContainsPoint(position))
                {
                    return o.ID;
                }
            }
            return -1;
        }
    }
}
