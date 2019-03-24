using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
namespace ExplicitCorridorMap
{
    public class PathFinding
    {      
        public static List<Vector2> FindPath(ECM ecm,int radiusIndex,Vector2 startPosition, Vector2 endPosition)
        {
            var radius = ecm.AgentRadius[radiusIndex];
            var startEdge = ecm.GetNearestEdge(ref startPosition, radius);
            var endEdge = ecm.GetNearestEdge(ref endPosition,radius);
            if (startEdge == null || endEdge == null) return new List<Vector2>();
            //check goal clearance
            var endEdgeProperty = endEdge.EdgeProperties[radiusIndex];
            if (endEdgeProperty.ClearanceOfStart < 0 && endEdgeProperty.ClearanceOfEnd < 0)
            {
                return new List<Vector2>();
            }

            var edgeList = FindEdgePathFromVertexToVertex(ecm, radiusIndex ,startEdge.Start,startEdge.End, endEdge.Start,endEdge.End,startPosition,endPosition);
            if(edgeList.Count == 0) { return new List<Vector2>(); }
            ComputePortals(radiusIndex,edgeList, startPosition, endPosition, out List<Vector2> portalsLeft, out List<Vector2> portalsRight);
            return GetShortestPath(portalsLeft, portalsRight);
        }
        
        private static List<Edge> FindEdgePathFromVertexToVertex(ECM graph, int radiusIndex, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition)
        {
            var path = PathFinding.FindPathFromVertexToVertex(graph, radiusIndex, start1, start2, end1,end2,startPosition,endPosition);
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
        //start1,start2 are two vertex of start edge
        //end1,end2 are two vertex of end edge
        private static List<Vertex> FindPathFromVertexToVertex(ECM graph, int radiusIndex, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition)
        {
            List<Vertex> result = new List<Vertex>();
            var agentRadius = graph.AgentRadius[radiusIndex];

            var openSet = new HashSet<Vertex>();
            openSet.Add(start1);
            openSet.Add(start2);
            var closeSet = new HashSet<Vertex>();
            var cameFrom = new Dictionary<Vertex, Vertex>();
            var gScore = new Dictionary<Vertex, float>();
            var fScore = new Dictionary<Vertex, float>();
            gScore[start1] = HeuristicCost(startPosition, start1.Position);
            fScore[start1] = HeuristicCost(start1.Position, endPosition);
            gScore[start2] = HeuristicCost(startPosition, start2.Position);
            fScore[start2] = HeuristicCost(start2.Position, endPosition);

            while (openSet.Count != 0)
            {
                var current = LowestFScore(openSet, fScore);
                if (current.Equals(end1)||current.Equals(end2)) return RecontructPath(cameFrom, current);
                openSet.Remove(current);
                closeSet.Add(current);

                foreach(var edge in current.Edges)
                {
                    var neigborVertex = edge.End;
                    if (closeSet.Contains(neigborVertex) || edge.EdgeProperties[radiusIndex].ClearanceOfEnd < 0) continue;
                    var tentativeGScore = gScore[current] + edge.Length;
                    if (!openSet.Contains(neigborVertex)) openSet.Add(neigborVertex);
                    else if (tentativeGScore >= gScore[neigborVertex]) continue;
                    cameFrom[neigborVertex] = current;
                    gScore[neigborVertex] = tentativeGScore;
                    fScore[neigborVertex] = tentativeGScore + HeuristicCost(neigborVertex.Position, endPosition);
                }
            }
            return result;

        }
        public static List<Vertex> RecontructPath(Dictionary<Vertex, Vertex> cameFrom, Vertex current)
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
        public static float HeuristicCost(Vertex start, Vertex end)
        {
            return HeuristicCost(start.Position, end.Position);
        }
        public static float HeuristicCost(Vector2 start, Vector2 end)
        {
            return (start - end).magnitude;
        }
        public static Vertex LowestFScore(HashSet<Vertex> hashSet, Dictionary<Vertex, float> fScore)
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
        public static List<Vector2> GetShortestPath(List<Vector2> portalsLeft, List<Vector2> portalsRight)
        {
            List<Vector2> path = new List<Vector2>();
            if (portalsLeft.Count == 0) return path;
            Vector2 portalApex, portalLeft, portalRight;
            int apexIndex = 0, leftIndex = 0, rightIndex = 0;
            portalApex = portalsLeft[0];
            portalLeft = portalsLeft[0];
            portalRight = portalsRight[0];
            AddToPath(path,portalApex);

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
                        AddToPath(path, portalRight);
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
                        AddToPath(path, portalLeft);
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
            AddToPath(path,portalsLeft[portalsLeft.Count - 1]);
            return path;
        }//funtion

        public static void ComputePortals(List<Edge> edgeList, Vector2 startPosition, Vector2 endPosition, out List<Vector2> portalsLeft, out List<Vector2> portalsRight)
        {
            portalsLeft = new List<Vector2>();
            portalsRight = new List<Vector2>();
            portalsLeft.Add(startPosition);
            portalsRight.Add(startPosition);
            //heuristic
            bool containsStart = true;
            bool containsEnd = true;
            int start = 0;
            int end = edgeList.Count - 1;
            while (start<=end)
            {
                var edge = edgeList[start];
                containsStart = Geometry.PolygonContainsPoint(edge.Start.Position, edge.EdgeProperties[radiusIndex].LeftObstacleOfStart, edge.EdgeProperties[radiusIndex].RightObstacleOfStart, startPosition);
                if (containsStart) start++;
                else break;
            }
            while (end>=start)
            {
                var edge = edgeList[end];
                containsEnd = Geometry.PolygonContainsPoint(edge.Start.Position, edge.EdgeProperties[radiusIndex].LeftObstacleOfStart, edge.EdgeProperties[radiusIndex].RightObstacleOfStart, endPosition);
                if (containsEnd) end--;
                else break;
            }
            for (int i = start; i <= end; i++)
            {
                var edge = edgeList[i];
                if (portalsLeft.Count != 0 &&
                    edge.EdgeProperties[radiusIndex].LeftObstacleOfStart == portalsLeft[portalsLeft.Count - 1] &&
                    edge.EdgeProperties[radiusIndex].RightObstacleOfStart == portalsRight[portalsRight.Count - 1]) continue;
                portalsLeft.Add(edge.EdgeProperties[radiusIndex].LeftObstacleOfStart);
                portalsRight.Add(edge.EdgeProperties[radiusIndex].RightObstacleOfStart);
            }
            //add last end portal
            if (end >= start)
            {
                var endEdge = edgeList[end];
                containsEnd = Geometry.PolygonContainsPoint(endEdge.End.Position, endEdge.EdgeProperties[radiusIndex].LeftObstacleOfEnd, endEdge.EdgeProperties[radiusIndex].RightObstacleOfEnd, endPosition);
                if (!containsEnd)
                {
                    portalsLeft.Add(endEdge.EdgeProperties[radiusIndex].LeftObstacleOfEnd);
                    portalsRight.Add(endEdge.EdgeProperties[radiusIndex].RightObstacleOfEnd);
                }
            }
            portalsLeft.Add(endPosition);
            portalsRight.Add(endPosition);
        }
        private static void AddToPath(List<Vector2> path, Vector2 point)
        {
            if(path.Count != 0 && point == path[path.Count - 1])
            {
                return;
            }
            else
            {
                path.Add(point);
            }
        }
    }
}
