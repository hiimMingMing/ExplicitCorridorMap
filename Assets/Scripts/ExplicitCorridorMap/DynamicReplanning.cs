using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class DynamicReplanning
    {
        //Convert obstacles to list of 4 points
        public static List<Vector2> convertToPoint(float xScale, float yScale, Vector3 input)
        {
            List<Vector2> pointList = new List<Vector2>();
            float x = input.x;
            float y = input.y;
            float w = xScale;
            float h = yScale;
            pointList.Add(new Vector2(x - w / 2, y - h / 2));
            pointList.Add(new Vector2(x + w / 2, y - h / 2));
            pointList.Add(new Vector2(x + w / 2, y + h / 2));
            pointList.Add(new Vector2(x - w / 2, y + h / 2));
            return pointList;
        }

        //Check if the obstacle is on the moving path or not
        public static List<Vector2> ListAffectedPath(List<Vector2> wayPointList, List<Vector2> obsPoint, int currentWayPoint)
        {
            List<Vector2> list = new List<Vector2>();

            if (obsPoint.Count > 0)
            {
                for (int i = currentWayPoint - 1; i < wayPointList.Count - 1; i++)
                {
                    var A = wayPointList[i];
                    var B = wayPointList[i + 1];

                    if (isIntersecting(A, B, obsPoint[0], obsPoint[1]) ||
                        isIntersecting(A, B, obsPoint[1], obsPoint[2]) ||
                        isIntersecting(A, B, obsPoint[2], obsPoint[3]) ||
                        isIntersecting(A, B, obsPoint[3], obsPoint[0]))
                    {
                        list.Add(A);
                        list.Add(B);
                    }
                }
                RemoveDuplicate(list);

                if (list.Count > 0) Debug.Log(list.Count + " - Affected!!");
                else Debug.Log(list.Count + " - Unaffected!!");
            }             
            return list;
        }

        public static List<Vector2> DynamicFindPath(ECM ecm, Vector2 startPosition, Vector2 endPosition, List<Vector2> listAffectPath, List<Vector2> obsPoints)
        {           
            var startEdge = ecm.GetNearestEdge(startPosition);
            var endEdge = ecm.GetNearestEdge(endPosition);
           
            return null;
        }

        //Dynamic find the new path
        public static List<Vector2> DynamicFindPath2(ECM ecm, Vector2 startPosition, Vector2 endPosition, List<Vector2> affectPoint)
        {
            List<Vector2> interscetPoint = new List<Vector2>();
            List<Vector2> shortertPath = new List<Vector2>();
            List<Vector2> portalsLeft, portalsRight;
            List<Vector2> portalsLeft2, portalsRight2;

            float oldFScore = 0, newFScore = 0;
            List<Edge> oldEdgeList, newEdgeList, newEdgeList2;

            var startEdge = ecm.GetNearestEdge(startPosition);
            var endEdge = ecm.GetNearestEdge(endPosition);
       
            oldEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, endEdge.Start, endEdge.End, startPosition, endPosition, out oldFScore);

            for (int i = 0; i < affectPoint.Count - 2; i++)
            {
                if (isIntersecting(affectPoint[i], affectPoint[i + 1], startPosition, endPosition))
                    interscetPoint.Add(LineLineIntersection(affectPoint[i], affectPoint[i + 1], startPosition, endPosition));
            }

            if (interscetPoint.Count == 0) //The new path is still the old path
            {
                Debug.Log("TH0: No change!");
                PathFinding.ComputePortals(oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
                shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
            }
            else if (interscetPoint.Count == 1)
            {
                Debug.Log("TH1");
                var midEdge = ecm.GetNearestEdge(interscetPoint[0]);

                newEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, midEdge.Start, midEdge.End, startPosition, interscetPoint[0], out newFScore);
                float tempFScore = newFScore;
                newEdgeList2 = FindEdgePathFromVertexToVertex(ecm, midEdge.Start, midEdge.End, endEdge.Start, endEdge.End, interscetPoint[0], endPosition, out newFScore);
                newFScore += tempFScore;

                Debug.Log(oldFScore + " - " + newFScore);
                if (oldFScore < newFScore) //The new path is still the old path
                {
                    Debug.Log("No change!!!");
                    PathFinding.ComputePortals(oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                }
                else //Change the path
                {
                    Debug.Log("Change!!!");
                    PathFinding.ComputePortals(newEdgeList, startPosition, interscetPoint[0], out portalsLeft, out portalsRight);
                    PathFinding.ComputePortals(newEdgeList2, interscetPoint[0], endPosition, out portalsLeft2, out portalsRight2);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                    shortertPath.AddRange(PathFinding.GetShortestPath(portalsLeft2, portalsRight2));
                }
            }
            else if (interscetPoint.Count == 2)
            {
                Debug.Log("TH2");
                var midEdge1 = ecm.GetNearestEdge(interscetPoint[0]);
                var midEdge2 = ecm.GetNearestEdge(interscetPoint[1]);

                if (PathFinding.HeuristicCost(startPosition, interscetPoint[0]) > PathFinding.HeuristicCost(startPosition, interscetPoint[1]))
                    interscetPoint.Reverse();

                newEdgeList = FindEdgePathFromVertexToVertex(ecm, startEdge.Start, startEdge.End, midEdge1.Start, midEdge1.End, startPosition, interscetPoint[0], out newFScore);
                float tempFScore = newFScore;
                newEdgeList2 = FindEdgePathFromVertexToVertex(ecm, midEdge2.Start, midEdge2.End, endEdge.Start, endEdge.End, interscetPoint[1], endPosition, out newFScore);
                newFScore += (PathFinding.HeuristicCost(interscetPoint[0], interscetPoint[1]) + tempFScore);

                Debug.Log(oldFScore + " - " + newFScore);
                if (oldFScore < newFScore) //The new path is still the old path
                {
                    Debug.Log("No change!!!");
                    PathFinding.ComputePortals(oldEdgeList, startPosition, endPosition, out portalsLeft, out portalsRight);
                    shortertPath = PathFinding.GetShortestPath(portalsLeft, portalsRight);
                }
                else //Change the path
                {
                    Debug.Log("Change!!!");
                    PathFinding.ComputePortals(newEdgeList, startPosition, interscetPoint[0], out portalsLeft, out portalsRight);
                    PathFinding.ComputePortals(newEdgeList2, interscetPoint[1], endPosition, out portalsLeft2, out portalsRight2);
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

        //Check if 2 segments are intersecting or not
        //https://stackoverflow.com/questions/14176776/find-out-if-2-lines-intersect
        private static bool isIntersecting(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            return (((q1.x - p1.x) * (p2.y - p1.y) - (q1.y - p1.y) * (p2.x - p1.x))
                    * ((q2.x - p1.x) * (p2.y - p1.y) - (q2.y - p1.y) * (p2.x - p1.x)) < 0)
                    &&
                   (((p1.x - q1.x) * (q2.y - q1.y) - (p1.y - q1.y) * (q2.x - q1.x))
                    * ((p2.x - q1.x) * (q2.y - q1.y) - (p2.y - q1.y) * (q2.x - q1.x)) < 0);
        }

        //Remove duplicate in List<Vector2>
        private static void RemoveDuplicate(List<Vector2> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].Equals(list[i + 1]))
                {
                    list.RemoveAt(i + 1);
                    i--;
                }
            }
        }

        //Find a position intersection of 2 lines
        //https://www.geeksforgeeks.org/program-for-point-of-intersection-of-two-lines/
        public static Vector2 LineLineIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            // Line AB represented as a1x + b1y = c1  
            float a1 = B.y - A.y;
            float b1 = A.x - B.x;
            float c1 = a1 * (A.x) + b1 * (A.y);

            // Line CD represented as a2x + b2y = c2  
            float a2 = D.y - C.y;
            float b2 = C.x - D.x;
            float c2 = a2 * (C.x) + b2 * (C.y);

            float determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                // The lines are parallel. This is simplified  
                // by returning a pair of FLT_MAX  
                return new Vector2(float.MaxValue, float.MaxValue);
            }
            else
            {
                float x = (b2 * c1 - b1 * c2) / determinant;
                float y = (a1 * c2 - a2 * c1) / determinant;
                return new Vector2(x, y);
            }
        }
        public static void SwapValue(ref Vector2 num1, ref Vector2 num2)
        {
            Vector2 temp;
            temp = num1;
            num1 = num2;
            num2 = temp;
        }
    }
}
