using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
namespace ExplicitCorridorMap
{
    public class PathFinding
    {
        public static List<Portal> FindPath(ECM ecm, int radiusIndex, Edge startEdge, Vector2 startPosition1, Vector2 startPosition2,ref Vector2 endPosition, out Vertex choosenVertex)
        {
            //find end edge
            choosenVertex = null;
            var pathPortals = new List<Portal>();
            var radius = ecm.AgentRadius[radiusIndex];
            var endEdge = ecm.GetNearestEdge(ref endPosition, radius);
            if (startEdge == null || endEdge == null) return pathPortals;
            //check goal clearance
            var endEdgeProperty = endEdge.EdgeProperties[radiusIndex];
            if (endEdgeProperty.ClearanceOfStart < 0 && endEdgeProperty.ClearanceOfEnd < 0)
            {
                return pathPortals;
            }
            //run astar
            var vertexList = FindPathFromVertexToVertex(ecm, radiusIndex, startEdge.Start, startEdge.End, endEdge.Start, endEdge.End, startPosition1, startPosition2, endPosition);
            if (vertexList.Count == 0) return pathPortals;

            //choose best start position for group
            choosenVertex = vertexList[vertexList.Count-1];
            Vector2 choosenStartPosition;
            if (choosenVertex == startEdge.Start) choosenStartPosition = startPosition1;
            else choosenStartPosition = startPosition2;
            if (vertexList.Count == 1)
            {
                pathPortals.Add(new Portal(choosenStartPosition));
                pathPortals.Add( new Portal(endPosition) );
                return pathPortals;
            }
            //convert to path
            var edgeList = ConvertToEdgeList(vertexList);
            pathPortals = ComputePortals(radiusIndex, edgeList, choosenStartPosition, endPosition);
            return pathPortals;
        }
        public static List<Vector2> FindPath(ECM ecm,int radiusIndex,Vector2 startPosition, Vector2 endPosition)
        {
            var radius = ecm.AgentRadius[radiusIndex];
            var startEdge = ecm.GetNearestEdge(ref startPosition, radius);
            var pathPortals =  FindPath(ecm, radiusIndex, startEdge, startPosition, startPosition,ref endPosition, out Vertex v);
            return GetShortestPath(pathPortals).ConvertAll(x => x.Point);
        }
        
        private static List<Edge> ConvertToEdgeList(List<Vertex> path)
        {
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
        private static List<Vertex> FindPathFromVertexToVertex(ECM graph, int radiusIndex, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition1,Vector2 startPosition2, Vector2 endPosition)
        {
            List<Vertex> result = new List<Vertex>();
            var radius = graph.AgentRadius[radiusIndex];

            var openSet = new HashSet<Vertex>();
            openSet.Add(start1);
            openSet.Add(start2);
            var closeSet = new HashSet<Vertex>();
            var cameFrom = new Dictionary<Vertex, Vertex>();
            var gScore = new Dictionary<Vertex, float>();
            var fScore = new Dictionary<Vertex, float>();
            gScore[start1] = HeuristicCost(startPosition1, start1.Position);
            fScore[start1] = HeuristicCost(start1.Position, endPosition);
            gScore[start2] = HeuristicCost(startPosition2, start2.Position);
            fScore[start2] = HeuristicCost(start2.Position, endPosition);

            while (openSet.Count != 0)
            {
                var current = LowestFScore(openSet, fScore);
                if (current.Equals(end1) || current.Equals(end2)) return RecontructPath(cameFrom, current);
                openSet.Remove(current);
                closeSet.Add(current);

                foreach (var edge in current.Edges)
                {
                    var neigborVertex = edge.End;
                    if (closeSet.Contains(neigborVertex) || edge.HasEnoughClearance(radius)) continue;
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
        private static List<Vertex> FindPathFromVertexToVertex(ECM graph, int radiusIndex, Vertex start1, Vertex start2, Vertex end1, Vertex end2, Vector2 startPosition, Vector2 endPosition)
        {
            return FindPathFromVertexToVertex(graph, radiusIndex, start1, start2, end1, end2, startPosition, startPosition, endPosition);
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
        public static List<Portal> GetShortestPath(List<Portal> portals)
        {
            var path = new List<Portal>();
            if (portals.Count == 0) return path;
            int apexIndex = 0, leftIndex = 0, rightIndex = 0;
            Vector2 portalApex = portals[0].Left;
            Vector2 portalLeft = portals[0].Left;
            Vector2 portalRight = portals[0].Right;
            portals[0].Point = portalApex;
            AddToPath(path,portals[0]);

            for (int i = 1; i < portals.Count; i++)
            {
                var left = portals[i].Left;
                var right = portals[i].Right;
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
                        var p = portals[rightIndex];
                        p.IsLeft = false;
                        p.Point = portalRight;
                        AddToPath(path, p);
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
                        var p = portals[leftIndex];
                        //Default p.IsLeft = true;
                        p.Point = portalLeft;
                        AddToPath(path, p);
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
            var lastPortal = portals[portals.Count - 1];
            lastPortal.Point = lastPortal.Left;
            AddToPath(path,lastPortal);
            return path;
        }//funtion

        public static List<Portal> ComputePortals(int radiusIndex, List<Edge> edgeList, Vector2 startPosition, Vector2 endPosition)
        {
            var portals = new List<Portal>();
            portals.Add(new Portal(startPosition));
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
                if (portals.Count != 0 &&
                    edge.EdgeProperties[radiusIndex].LeftObstacleOfStart == portals[portals.Count - 1].Left &&
                    edge.EdgeProperties[radiusIndex].RightObstacleOfStart == portals[portals.Count - 1].Right) continue;
                var p = new Portal();
                p.Left = edge.EdgeProperties[radiusIndex].LeftObstacleOfStart;
                p.Right = edge.EdgeProperties[radiusIndex].RightObstacleOfStart;
                p.Length = edge.EdgeProperties[radiusIndex].WidthClearanceOfStart;
                portals.Add(p);
            }
            //add last end portal
            if (end >= start)
            {
                var endEdge = edgeList[end];
                containsEnd = Geometry.PolygonContainsPoint(endEdge.End.Position, endEdge.EdgeProperties[radiusIndex].LeftObstacleOfEnd, endEdge.EdgeProperties[radiusIndex].RightObstacleOfEnd, endPosition);
                if (!containsEnd)
                {
                    var p = new Portal();
                    p.Left = endEdge.EdgeProperties[radiusIndex].LeftObstacleOfEnd;
                    p.Right = endEdge.EdgeProperties[radiusIndex].RightObstacleOfEnd;
                    p.Length = endEdge.EdgeProperties[radiusIndex].WidthClearanceOfEnd;
                    portals.Add(p);
                }
            }
            portals.Add(new Portal(endPosition));
            return portals;
        }
        private static void AddToPath(List<Portal> path, Portal point)
        {
            if(path.Count != 0 && point.Point == path[path.Count - 1].Point)
            {
                return;
            }
            else
            {
                path.Add(point);
            }
        }
    }

    public class Portal
    {
        public Vector2 Point;
        public bool IsLeft = true;
        public Vector2 Left;
        public Vector2 Right;
        public float Length;
        public Vector2 RightToLeft;
        public Vector2 LeftToRight;
        public Portal(Vector2 l, Vector2 r)
        {
            Left = l;
            Right = r;
        }
        public Portal(Vector2 l)
        {
            Left = l;
            Right = l;
            Point = l;
            Length = 0.0f;
        }
        public Portal() { }
        public void ComputeVector()
        {
            RightToLeft = (Left - Right).normalized;
            LeftToRight = (Right - Left).normalized;
        }
    }
}
