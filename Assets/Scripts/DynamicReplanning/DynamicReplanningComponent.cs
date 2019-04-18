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

                    if (listAffectPath.Count > 0)
                        if (!usingODPA)
                        {
                            newPath = PathFinding.FindPath(ecmMap.ECMGraph, player.RadiusIndex, player.transform.position, player.WayPointList[player.WayPointList.Count - 1]);
                        }
                        else
                        {
                            newPath = DynamicFindPathByODPAVer2(ecmMap.ECMGraph, player.RadiusIndex, player.transform.position, player.WayPointList[player.WayPointList.Count - 1], player.WayPointList, listAffectPath, obsRect);
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
                float rayDistance;

                Vector3 _position = Vector3.zero;
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane basePlane = new Plane(Vector3.up, Vector3.zero);
                if (basePlane.Raycast(mouseRay, out rayDistance))
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
                float rayDistance;

                Vector3 _position = Vector3.zero;
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane basePlane = new Plane(Vector3.up, Vector3.zero);
                if (basePlane.Raycast(mouseRay, out rayDistance))
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

        //Case 1: Unaffected path (When delete an obstacle)
        //R_area is area changed by add/delete obstacle
        public static List<Vector2> DynamicFindPathByODPAVer1(ECM ecm, int radiusIndex, Vector2 startPosition, Vector2 endPosition, RectInt R_area)
        {
            List<Vector2> newPath = new List<Vector2>();

            return newPath;
        }

        //Case 2: Affected path (When add new obstacle)
        //listAffectPath is a segment path changed
        //R_area is area changed by add/delete obstacle
        public static List<Vector2> DynamicFindPathByODPAVer2(ECM ecm, int radiusIndex, Vector2 startPosition, Vector2 endPosition, List<Vector2> wayPointList, List<Vector2> listAffectPath, RectInt R_area)
        {
            List<Vector2> newPath = new List<Vector2>(); Vertex v;

            //
            //Step 1: Go to last point of listAffectPath (lastAffectPoint) using A*
            float radius = ecm.AgentRadius[radiusIndex];
            Edge startEdge = ecm.GetNearestEdge(ref startPosition, radius);
            Vector2 lastAffectPoint = listAffectPath[listAffectPath.Count - 1];

            var pathPortals = PathFinding.FindPath(ecm, radiusIndex, startEdge, startPosition, startPosition, ref lastAffectPoint, out v);
            newPath = PathFinding.GetShortestPath(pathPortals).ConvertAll(x => x.Point);

            //
            //Step 2: Go to endPosition using ODPA*
            var oldPath = FindOldVertexPath(ecm, wayPointList, lastAffectPoint); //oldPath BG-

            var midEdge = ecm.GetNearestEdge(ref lastAffectPoint, radius);
            var endEdge = ecm.GetNearestEdge(ref endPosition, radius);
            if (midEdge == null || endEdge == null) return newPath;

            //Check goal clearance
            var endEdgeProperty = endEdge.EdgeProperties[radiusIndex];
            if (endEdgeProperty.ClearanceOfStart < 0 && endEdgeProperty.ClearanceOfEnd < 0) return newPath;

            //Run ODPA*
            var vertexList = DynamicFindPathVertexVer2(ecm, radiusIndex, midEdge.Start, midEdge.End, endEdge.Start, endEdge.End, lastAffectPoint, endPosition, oldPath, R_area);
            if (vertexList.Count == 0) return newPath;

            //convert to path
            var edgeList = PathFinding.ConvertToEdgeList(vertexList);
            pathPortals = PathFinding.ComputePortals(radiusIndex, edgeList, lastAffectPoint, endPosition);

            newPath.AddRange(PathFinding.GetShortestPath(pathPortals).ConvertAll(x => x.Point));
            return newPath;
        }

        //ODPA* algorithm case 2: Affected path
        public static List<Vertex> DynamicFindPathVertexVer2(ECM ecm, int radiusIndex, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition, List<Vertex> oldPath, RectInt R_area)
        {
            List<Vertex> result = new List<Vertex>();
            var radius = ecm.AgentRadius[radiusIndex];

            var openSet = new HashSet<Vertex>();
            openSet.Add(start1);
            openSet.Add(start2);
            var closeSet = new HashSet<Vertex>();
            var cameFrom = new Dictionary<Vertex, Vertex>();
            var gScore = new Dictionary<Vertex, float>();
            var fScore = new Dictionary<Vertex, float>();
            gScore[start1] = PathFinding.HeuristicCost(startPosition, start1.Position);
            fScore[start1] = PathFinding.HeuristicCost(start1.Position, endPosition);
            gScore[start2] = PathFinding.HeuristicCost(startPosition, start2.Position);
            fScore[start2] = PathFinding.HeuristicCost(start2.Position, endPosition);

            //ODPA
            bool checkAll;
            var rWorse = new Dictionary<Vertex, bool>();
            rWorse[start1] = false;
            rWorse[start2] = false;
            Dictionary<Vertex, float> preComputeCost = PreComputeCost(oldPath); //oldPath: BG-
            //

            while (openSet.Count != 0)
            {
                var current = PathFinding.LowestFScore(openSet, fScore);
                if (current.Equals(end1) || current.Equals(end2)) return PathFinding.RecontructPath(cameFrom, current);
                openSet.Remove(current);
                closeSet.Add(current);

                //ODPA
                //if (cameFrom.ContainsKey(current)) rWorse[cameFrom[current]] = rWorse[current];
                if (!oldPath.Contains(current))
                    checkAll = true;
                else
                {
                    var R_point = DRMath.FindNearestPoint(startPosition, R_area); //Find nearest point in R_area to startPosition
                    if (PathFinding.HeuristicCost(current.Position, R_point) + PathFinding.HeuristicCost(R_point, endPosition)
                        > preComputeCost[current]) //h'(V,R) + h'(G,R) > cost([VG]-)
                    {
                        rWorse[current] = true;
                        checkAll = false;
                    }
                    else
                    {
                        rWorse[current] = false;
                        checkAll = true;
                    }
                } //

                foreach (var edge in current.Edges)
                {
                    var neigborVertex = edge.End;
                    if (closeSet.Contains(neigborVertex) || edge.HasEnoughClearance(radius)) continue;

                    //ODPA
                    if (!checkAll && !oldPath.Contains(neigborVertex)) continue; //

                    var tentativeGScore = gScore[current] + edge.Length;
                    if (!openSet.Contains(neigborVertex)) openSet.Add(neigborVertex);
                    else if (tentativeGScore >= gScore[neigborVertex]) continue;
                    cameFrom[neigborVertex] = current;
                    gScore[neigborVertex] = tentativeGScore;
                    fScore[neigborVertex] = tentativeGScore + PathFinding.HeuristicCost(neigborVertex.Position, endPosition);
                }
            }
            return result;
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

            //if (list.Count > 0) Debug.Log(list.Count + " - Affected!!");
            //else Debug.Log(list.Count + " - Unaffected!!");

            return list;
        }

        //Find old list of vertex 
        public static List<Vertex> FindOldVertexPath(ECM ecm, List<Vector2> wayPointList, Vector2 startPosition)
        {
            List<Vertex> listVertex = new List<Vertex>();

            for (int i = wayPointList.Count - 1; i > 0; i--)
            {
                var edge = ecm.GetNearestEdge(wayPointList[i]);
                if (edge != null)
                {
                    listVertex.Add(edge.End);
                    listVertex.Add(edge.Start);
                }
                if (wayPointList[i].Equals(startPosition)) break;
            }
            var newListVertex = DRMath.RemoveDuplicate(listVertex);
            newListVertex.Reverse();

            return newListVertex;
        }

        //Compute the cost of old path
        public static Dictionary<Vertex, float> PreComputeCost(List<Vertex> oldPath)
        {
            var preComputeCost = new Dictionary<Vertex, float>();

            float tempValue = 0;

            if (oldPath.Count == 0) return preComputeCost;
            else if (oldPath.Count == 1)
            {
                preComputeCost[oldPath[0]] = 0;
                return preComputeCost;
            }
            preComputeCost[oldPath[oldPath.Count - 1]] = 0;

            for (int i = oldPath.Count - 2; i >= 0; i--)
            {
                preComputeCost[oldPath[i]] = PathFinding.HeuristicCost(oldPath[i], oldPath[i + 1]) + tempValue;
                tempValue = preComputeCost[oldPath[i]];
            }

            return preComputeCost;
        }

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
