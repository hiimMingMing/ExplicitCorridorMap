using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class PathFinding
    {
        public static List<Vector2> FindPath(ECM ecm,Vector2 startPosition, Vector2 goalPosition)
        {
            var startNearestEdge = ecm.GetNearestEdge(startPosition);
            var goalNearestEdge = ecm.GetNearestEdge(goalPosition);
            var startVertex = FindBestVertexOnEdge(startNearestEdge, startPosition, goalPosition);
            var endVertex = FindBestVertexOnEdge(goalNearestEdge, goalPosition, startPosition);

            var edgeList = FindEdgePathFromVertexToVertex(ecm, startVertex, endVertex);
            ComputePortals(edgeList, startPosition, goalPosition, out List<Vector2> portalsLeft, out List<Vector2> portalsRight);
            return GetShortestPath(portalsLeft, portalsRight);
        }
        private static Vertex FindBestVertexOnEdge(Edge edge,Vector2 start, Vector2 goal)
        {
            var fStartVertex = HeuristicCost(start, edge.Start.Position) + HeuristicCost(edge.Start.Position, goal);
            var fEndVertex = HeuristicCost(start, edge.End.Position) + HeuristicCost(edge.End.Position, goal);
            if (fStartVertex < fEndVertex) return edge.Start;
            else return edge.End;
        }
        private static List<Edge> FindEdgePathFromVertexToVertex(ECM graph, Vertex start, Vertex goal)
        {
            var path = PathFinding.FindPathFromVertexToVertex(graph, start, goal);
            var edgeList = new List<Edge>();
            
            for (int i = path.Count - 1; i > 0; i--)
            {
                foreach( var edge in path[i].Edges)
                {
                    var end = edge.End;
                    if (end.Equals(path[i - 1])) edgeList.Add(edge);
                }
            }
            return edgeList;

        }
        //Simple Astar Algorithm
        private static List<Vertex> FindPathFromVertexToVertex(ECM graph, Vertex start, Vertex goal)
        {
            var openSet = new HashSet<Vertex>();
            openSet.Add(start);
            var closeSet = new HashSet<Vertex>();
            var cameFrom = new Dictionary<Vertex, Vertex>();
            var gScore = new Dictionary<Vertex, float>();
            gScore[start] = 0;
            var fScore = new Dictionary<Vertex, float>();
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
        private static float HeuristicCost(Vertex start, Vertex goal)
        {
            return HeuristicCost(start.Position, goal.Position);
        }
        private static float HeuristicCost(Vector2 start, Vector2 goal)
        {
            var dx = start.x - goal.x;
            var dy = start.y - goal.y;
            var h = dx * dx + dy * dy;
            return h;
        }
        private static Vertex LowestFScore(HashSet<Vertex> hashSet, Dictionary<Vertex, float> fScore)
        {
            float min = float.MaxValue;
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
        private static float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = b.x - a.x;
            float ay = b.y - a.y;
            float bx = c.x - a.x;
            float by = c.y - a.y;
            return bx * ay - ax * by;
        }

        //Simple Stupid Funnel Algorithm
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

            var left1 = portalsLeft[1];
            var right1 = portalsRight[1];
            //heuristic
            if (HeuristicCost(portalApex, left1) > HeuristicCost(portalApex, right1))
            {
                for (int i = 1; i < portalsLeft.Count; i++)
                {
                    var left = portalsLeft[i];
                    var right = portalsRight[i];
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
                    
                }//for
            }
            else
            {
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
            }
            path.Add(portalsLeft[portalsLeft.Count - 1]);
            return path;
        }//funtion
        private static void ComputePortals(List<Edge> edgeList, Vector2 startPosition, Vector2 endPosition, out List<Vector2> portalsLeft, out List<Vector2> portalsRight)
        {
            portalsLeft = new List<Vector2>();
            portalsRight = new List<Vector2>();
            portalsLeft.Add(startPosition);
            portalsRight.Add(startPosition);
            for (int i = 0; i < edgeList.Count; i++)
            {
                var edge = edgeList[i];
                if (i != 0)
                {
                    var left1 = edge.LeftObstacleOfStart;
                    var right1 = edge.RightObstacleOfStart;
                    //heuristic
                    var containStart = Geometry.PolygonContainsPoint(edge.Start.Position, left1, right1, startPosition);
                    var containEnd = Geometry.PolygonContainsPoint(edge.Start.Position, left1, right1, endPosition);
                    if (!containStart&&!containEnd)
                    {
                        portalsLeft.Add(left1);
                        portalsRight.Add(right1);
                    }
                }
                if (i == edgeList.Count - 1)
                {
                    
                    break;
                }
                var edgeNext = edgeList[i + 1];
                if (edge.LeftObstacleOfEnd.Equals(edgeNext.LeftObstacleOfStart) ||
                    edge.RightObstacleOfEnd.Equals(edgeNext.RightObstacleOfStart))
                {
                    continue;
                }
                var left2 = edge.LeftObstacleOfEnd;
                var right2 = edge.RightObstacleOfEnd;
                //heuristic
                var containStart2 = Geometry.PolygonContainsPoint(edge.End.Position, left2, right2, startPosition);
                var containEnd2 = Geometry.PolygonContainsPoint(edge.End.Position, left2, right2, endPosition);
                if (!containStart2 && !containEnd2)
                {
                    portalsLeft.Add(left2);
                    portalsRight.Add(right2);
                }
            }
            portalsLeft.Add(endPosition);
            portalsRight.Add(endPosition);
        }
        
    }
}
