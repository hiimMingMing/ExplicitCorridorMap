using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class Astar
    {
        public static List<Vector2> FindPath(ECM ecm,Vector2 startPosition, Vector2 endPosition)
        {
            var startNearestVertex = ecm.GetNearestVertex(startPosition);
            var goalNearestVertex = ecm.GetNearestVertex(endPosition);
            var edgeList = Astar.FindEdgePathFromVertexToVertex(ecm, startNearestVertex, goalNearestVertex);
            ComputePortals(edgeList, startPosition, endPosition, out List<Vector2> portalsLeft, out List<Vector2> portalsRight);
            return GetShortestPath(portalsLeft, portalsRight);
        }
        private static List<Edge> FindEdgePathFromVertexToVertex(ECM graph, Vertex start, Vertex goal)
        {
            var path = Astar.FindPathFromVertexToVertex(graph, start, goal);
            var edgeList = new List<Edge>();
            
            for (int i = 0; i < path.Count - 1; i++)
            {
                foreach( var edge in path[i].Edges)
                {
                    var end = edge.End;
                    if (end.Equals(path[i + 1])) edgeList.Add(edge);
                }
            }
            return edgeList;

        }
        private static List<Vertex> FindPathFromVertexToVertex(ECM graph, Vertex start, Vertex goal)
        {
            return FindPathVertexReversed(graph, goal, start);
        }
        private static List<Vertex> FindPathVertexReversed(ECM graph, Vertex start, Vertex goal)
        {
            var openSet = new HashSet<Vertex>();
            openSet.Add(start);
            var closeSet = new HashSet<Vertex>();
            var cameFrom = new Dictionary<Vertex, Vertex>();
            var gScore = new Dictionary<Vertex, double>();
            gScore[start] = 0;
            var fScore = new Dictionary<Vertex, double>();
            fScore[start] = HeuristicCost(start, goal);
            List<Vertex> result = new List<Vertex>();
            while (openSet.Count != 0)
            {
                var current = LowestFScore(openSet, fScore);
                if (current.Equals(goal)) return RecontructPath(cameFrom, current);
                openSet.Remove(current);
                closeSet.Add(current);

                foreach(var edge in current.Edges)
                {
                    var neigborVertex = edge.End;
                    if (closeSet.Contains(neigborVertex)) continue;
                    var tentativeGScore = gScore[current] + HeuristicCost(current, neigborVertex);
                    if (!openSet.Contains(neigborVertex)) openSet.Add(neigborVertex);
                    else if (tentativeGScore >= gScore[neigborVertex]) continue;
                    cameFrom[neigborVertex] = current;
                    gScore[neigborVertex] = tentativeGScore;
                    fScore[neigborVertex] = tentativeGScore + HeuristicCost(neigborVertex, goal);
                }
            }
            return result;

        }
        private static List<Vertex> RecontructPath(Dictionary<Vertex, Vertex> cameFrom, Vertex current)
        {
            var totalPath = new List<Vertex>();
            totalPath.Add(current);
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            return totalPath;
        }
        private static double HeuristicCost(Vertex start, Vertex goal)
        {
            var h = Math.Pow(start.X - goal.X, 2) + Math.Pow(start.Y - goal.Y, 2);
            return h;
        }
        private static Vertex LowestFScore(HashSet<Vertex> hashSet, Dictionary<Vertex, double> fScore)
        {
            double min = Double.MaxValue;
            Vertex result = null;
            foreach (var v in hashSet)
            {
                var f = fScore[v];
                if (f < min)
                {
                    min = f;
                    result = v;
                }
            }
            return result;
        }
        private static List<Vector2> GetShortestPath(List<Vector2> portalsLeft, List<Vector2> portalsRight)
        {
            List<Vector2> path = new List<Vector2>();
            if (portalsLeft.Count == 0) return path;
            Vector2 portalApex, portalLeft, portalRight;
            int apexIndex = 0, leftIndex = 0, rightIndex = 0;
            portalApex = portalsLeft[0];
            portalLeft = portalsLeft[0];
            portalRight = portalsRight[0];
            path.Add(portalApex);
            for (int i = 1; i < portalsLeft.Count; i++)
            {
                var left = portalsLeft[i];
                var right = portalsRight[i];
                // Update right vertex.
                if (CrossProduct(portalApex, portalRight, right) <= 0.0f)
                {
                    if (portalApex.Equals(portalRight) || CrossProduct(portalApex, portalLeft, right) > 0.0f)
                    {
                        // Tighten the funnel.
                        portalRight = right;
                        rightIndex = i;
                    }
                    else
                    {
                        path.Add(portalLeft);
                        // Make current left the new apex.
                        portalApex = portalLeft;
                        apexIndex = leftIndex;
                        // Reset portal
                        portalLeft = portalApex;
                        portalRight = portalApex;
                        leftIndex = apexIndex;
                        rightIndex = apexIndex;
                        // Restart scan
                        i = apexIndex;
                        continue;
                    }
                }
                // Update left vertex.
                if (CrossProduct(portalApex, portalLeft, left) >= 0.0f)
                {
                    if (portalApex.Equals(portalLeft) || CrossProduct(portalApex, portalRight, left) < 0.0f)
                    {
                        // Tighten the funnel.
                        portalLeft = left;
                        leftIndex = i;
                    }
                    else
                    {
                        // Left over right, insert right to path and restart scan from portal right point.
                        path.Add(portalRight);
                        // Make current right the new apex.
                        portalApex = portalRight;
                        apexIndex = rightIndex;
                        // Reset portal
                        portalLeft = portalApex;
                        portalRight = portalApex;
                        leftIndex = apexIndex;
                        rightIndex = apexIndex;
                        // Restart scan
                        i = apexIndex;
                        continue;
                    }
                }//if
            }//for
            path.Add(portalsLeft[portalsLeft.Count - 1]);
            return path;
        }//funtion
        private static void ComputePortals(List<Edge> edgeList, Vector2 startPosition, Vector2 endPosition, out List<Vector2> portalsLeft, out List<Vector2> portalsRight)
        {
            portalsLeft = new List<Vector2>();
            portalsRight = new List<Vector2>();
            for (int i = 0; i < edgeList.Count; i++)
            {
                var edge = edgeList[i];
                if (i != 0)
                {
                    var left1 = edge.LeftObstacleStart;
                    var right1 = edge.RightObstacleStart;
                    portalsLeft.Add(left1);
                    portalsRight.Add(right1);
                }
                else
                {
                    portalsLeft.Add(startPosition);
                    portalsRight.Add(startPosition);
                }
                if (i == edgeList.Count - 1)
                {
                    portalsLeft.Add(endPosition);
                    portalsRight.Add(endPosition);
                    break;
                }
                var edgeNext = edgeList[i + 1];
                if (edge.LeftObstacleEnd.Equals(edgeNext.LeftObstacleStart) ||
                    edge.RightObstacleEnd.Equals(edgeNext.RightObstacleStart))
                {
                    continue;
                }
                var left2 = edge.LeftObstacleEnd;
                var right2 = edge.RightObstacleEnd;
                portalsLeft.Add(left2);
                portalsRight.Add(right2);
            }
        }
        private static float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = b.x - a.x;
            float ay = b.y - a.y;
            float bx = c.x - a.x;
            float by = c.y - a.y;
            return bx * ay - ax * by;
        }
    }
}
