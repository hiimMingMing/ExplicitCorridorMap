using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class DynamicReplanning
    {
        //Handle dynamic events (Add/Delete obstacle)
        public static void HandleDynamicEvent(Player player)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //Add a dynamic obstacle
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z -= Camera.main.transform.position.z;

                player.obstacle = DRMath.ConvertToRect(player.addObj.transform.localScale.x, player.addObj.transform.localScale.y, position);
                Object.Instantiate(player.addObj, position, player.transform.rotation);

                //Handle the path
                var listAffectPath  = ListAffectedPath(player.wayPointList, player.obstacle, player.currentWayPoint);
                var newPath = DynamicFindPathByECM(player.ecmMap, player.RadiusIndex, player.transform.position, player.finalTarget, listAffectPath, player.obstacle);
                if (newPath.Count > 0)
                {
                    player.wayPointList = newPath;           
                }
                player.DrawPath(Color.green);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                //Delete an obstacle
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    BoxCollider bc = (BoxCollider)hit.collider;
                    if (bc != null)
                    {
                        var xScale = bc.gameObject.transform.localScale.x;
                        var yScale = bc.gameObject.transform.localScale.y;
                        var position = bc.gameObject.transform.position;
                        player.obstacle = DRMath.ConvertToRect(xScale, yScale, position);

                        Object.Destroy(bc.gameObject);
                    }
                }

                //Handle the path
                player.wayPointList = DynamicFindPathByECM2(player.ecmMap, player.RadiusIndex, player.transform.position, player.finalTarget, player.obstacle);
                player.DrawPath(Color.green);
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
            ecmmap.ecm.AddPolygonDynamic(new Obstacle(obstacle));
            ecmmap.ComputeCurveEdge();
            if (listAffectPath.Count > 0)
                return PathFinding.FindPath(ecmmap.ecm, radiusIndex, startPosition, endPosition);

            return newPath;
        }

        //Dynamic find the new path by rebuild ECMMap - Delete obstacle case
        public static List<Vector2> DynamicFindPathByECM2(ECMMap ecmmap, int radiusIndex, Vector2 startPosition, Vector2 endPosition, RectInt obstacle)
        {
            int ID = FindObstacleID(ecmmap.ecm, obstacle);
            ecmmap.ecm.DeletePolygonDynamic(ID);
            ecmmap.ComputeCurveEdge();
            var shortestPath = PathFinding.FindPath(ecmmap.ecm, 0, startPosition, endPosition);

            return shortestPath;
        }

        //Dynamic find the new path using ODPA* - Add obstacle case
        //public static List<Vector2> DynamicFindPath(ECM ecm, int radiusIndex, Vector2 startPosition, Vector2 endPosition, List<Vector2> listAffectPath, RectInt obstacle)
        //{
        //    return null;
        //}

        //Dynamic find the new path using ODPA* - Delete obstacle case
        public static List<Vector2> DynamicFindPath2(ECM ecm, int radiusIndex, Vector2 startPosition, Vector2 endPosition, RectInt obstacle)
        {
            List<Vector2> intersectPoint = new List<Vector2>();
            List<Vector2> shortertPath = new List<Vector2>();
            List<Vector2> portalsLeft, portalsRight;
            List<Vector2> portalsLeft2, portalsRight2;

            Vector2 bottomLeft = obstacle.min;
            Vector2 bottomRight = new Vector2(obstacle.xMax, obstacle.yMin);
            Vector2 topRight = obstacle.max;
            Vector2 topLeft = new Vector2(obstacle.xMin, obstacle.yMax);

            float oldFScore = 0, newFScore = 0;
            List<Edge> oldEdgeList, newEdgeList, newEdgeList2;

            var startEdge = ecm.GetNearestEdge(ref startPosition, radiusIndex);
            var endEdge = ecm.GetNearestEdge(ref endPosition, radiusIndex);
       
            oldEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, endEdge.Start, endEdge.End, startPosition, endPosition, out oldFScore);

            //Find intersectPoint
            if (DRMath.IsIntersecting(bottomLeft, bottomRight, startPosition, endPosition))
                intersectPoint.Add(DRMath.LineLineIntersection(bottomLeft, bottomRight, startPosition, endPosition));

            if (DRMath.IsIntersecting(bottomRight, topRight, startPosition, endPosition))
                intersectPoint.Add(DRMath.LineLineIntersection(bottomRight, topRight, startPosition, endPosition));

            if (DRMath.IsIntersecting(topRight, topLeft, startPosition, endPosition))
                intersectPoint.Add(DRMath.LineLineIntersection(topRight, topLeft, startPosition, endPosition));

            if (DRMath.IsIntersecting(topLeft, bottomLeft, startPosition, endPosition))
                intersectPoint.Add(DRMath.LineLineIntersection(topLeft, bottomLeft, startPosition, endPosition));
        
            ////
            if (intersectPoint.Count == 0) //The new path is still the old path
            {               
                Debug.Log("TH0: No change!");
                PathFinding.ComputePortals(radiusIndex, oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
                shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
            }
            else if (intersectPoint.Count == 1)
            {               
                Debug.Log("TH1: " + intersectPoint[0]);
                var tempPoint = intersectPoint[0];
                var midEdge = ecm.GetNearestEdge(ref tempPoint, radiusIndex);

                newEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, midEdge.Start, midEdge.End, startPosition, intersectPoint[0], out newFScore);
                float tempFScore = newFScore;
                newEdgeList2 = FindEdgePathFromVertexToVertex(ecm, midEdge.Start, midEdge.End, endEdge.Start, endEdge.End, intersectPoint[0], endPosition, out newFScore);
                newFScore += tempFScore;

                Debug.Log(oldFScore + " - " + newFScore);
                if (oldFScore < newFScore) //The new path is still the old path
                {
                    Debug.Log("No change!!!");
                    PathFinding.ComputePortals(radiusIndex, oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                }
                else //Change the path
                {
                    Debug.Log("Change!!!");
                    PathFinding.ComputePortals(radiusIndex, newEdgeList, startPosition, intersectPoint[0], out portalsLeft, out portalsRight);
                    PathFinding.ComputePortals(radiusIndex, newEdgeList2, intersectPoint[0], endPosition, out portalsLeft2, out portalsRight2);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                    shortertPath.AddRange(PathFinding.GetShortestPath(portalsLeft2, portalsRight2));
                }
            }
            else if (intersectPoint.Count == 2)
            {
                Debug.Log("TH2: " + intersectPoint[0] + " - " + intersectPoint[1]);
                var tempPoint = intersectPoint[0];
                var tempPoint2 = intersectPoint[1];
                var midEdge1 = ecm.GetNearestEdge(ref tempPoint, radiusIndex);
                var midEdge2 = ecm.GetNearestEdge(ref tempPoint2, radiusIndex);

                if (PathFinding.HeuristicCost(startPosition, intersectPoint[0]) > PathFinding.HeuristicCost(startPosition, intersectPoint[1]))
                    intersectPoint.Reverse();

                newEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, midEdge1.Start, midEdge1.End, startPosition, intersectPoint[0], out newFScore);
                float tempFScore = newFScore;
                newEdgeList2 = FindEdgePathFromVertexToVertex(ecm, midEdge2.Start, midEdge2.End, endEdge.Start, endEdge.End, intersectPoint[1], endPosition, out newFScore);
                newFScore += (PathFinding.HeuristicCost(intersectPoint[0], intersectPoint[1]) + tempFScore);

                Debug.Log(oldFScore + " - " + newFScore);
                if (oldFScore < newFScore) //The new path is still the old path
                {
                    Debug.Log("No change!!!");
                    PathFinding.ComputePortals(radiusIndex, oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                }
                else //Change the path
                {
                    Debug.Log("Change!!!");
                    PathFinding.ComputePortals(radiusIndex, newEdgeList, startPosition, intersectPoint[0], out portalsLeft, out portalsRight);
                    PathFinding.ComputePortals(radiusIndex, newEdgeList2, intersectPoint[1], endPosition, out portalsLeft2, out portalsRight2);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                    shortertPath.AddRange(PathFinding.GetShortestPath(portalsLeft2, portalsRight2));
                }
            }
            return shortertPath;
        }

        private static List<Edge> FindEdgePathFromVertexToVertex(ECM graph, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition, out float finalFScore)
        {
            var path = FindPathFromVertexToVertex(graph, start1, start2, end1, end2, startPosition, endPosition, out finalFScore);
            var edgeList = new List<Edge>();

            for (int i = path.Count - 1; i > 0; i--)
            {
                foreach (var edge in path[i].Edges)
                {
                    var end = edge.End;
                    if (end.Equals(path[i - 1])) edgeList.Add(edge);
                }
            }
            return edgeList;
        }

        public static List<Vertex> FindPathFromVertexToVertex(ECM graph, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition, out float finalFScore)
        {
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

            List<Vertex> result = new List<Vertex>();
            while (openSet.Count != 0)
            {
                var current = PathFinding.LowestFScore(openSet, fScore);
                if (current.Equals(end1) || current.Equals(end2))
                {
                    if (current.Equals(end1)) finalFScore = fScore[end1];
                    else finalFScore = fScore[end2];
                    return PathFinding.RecontructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closeSet.Add(current);

                foreach (var edge in current.Edges)
                {
                    var neigborVertex = edge.End;
                    if (closeSet.Contains(neigborVertex)) continue;
                    var tentativeGScore = gScore[current] + PathFinding.HeuristicCost(current, neigborVertex);
                    if (!openSet.Contains(neigborVertex)) openSet.Add(neigborVertex);
                    else if (tentativeGScore >= gScore[neigborVertex]) continue;
                    cameFrom[neigborVertex] = current;
                    gScore[neigborVertex] = tentativeGScore;
                    fScore[neigborVertex] = tentativeGScore + PathFinding.HeuristicCost(neigborVertex.Position, endPosition);
                }
            }
            finalFScore = 0;
            return result;
        }   
        
        public static int FindObstacleID(ECM ecm, RectInt obstacle)
        {
            foreach (var o in ecm.Obstacles)
            {
                if (o.Value.Points.Contains(obstacle.min) && o.Value.Points.Contains(obstacle.max))
                    return o.Value.ID;
            }
            return -1;
        }  
    }
}
