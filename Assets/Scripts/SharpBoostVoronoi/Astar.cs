using System;
using System.Collections.Generic;
using SharpBoostVoronoi.Output;

namespace SharpBoostVoronoi
{
    public class Astar
    {
        public static List<Edge> FindPath(ExplicitCorridorMap graph, Vertex start, Vertex goal)
        {
            var path = Astar.FindPathVertex(graph, start, goal);
            var edgeList = new List<Edge>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var originalEdge = graph.Edges[path[i].IncidentEdge];
                var neighborEdge = originalEdge;
                do
                {
                    if (!neighborEdge.IsFinite || !neighborEdge.IsPrimary) continue;
                    var end = graph.Vertices[neighborEdge.End];
                    if (end.Equals(path[i + 1])) edgeList.Add(neighborEdge);
                }
                //proceed to next edge
                while ((neighborEdge = graph.Edges[neighborEdge.RotNext]) != originalEdge);
            }
            return edgeList;

        }
        private static List<Vertex> FindPathVertex(ExplicitCorridorMap graph, Vertex start, Vertex goal)
        {
            return FindPathVertexReversed(graph, goal, start);
        }
        private static List<Vertex> FindPathVertexReversed(ExplicitCorridorMap graph, Vertex start, Vertex goal)
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

                var originalEdge = graph.Edges[current.IncidentEdge];
                var neighborEdge = originalEdge;
                do
                {
                    if (!neighborEdge.IsFinite || !neighborEdge.IsPrimary) continue;
                    var neigborVertex = graph.Vertices[neighborEdge.End];
                    if (closeSet.Contains(neigborVertex)) continue;
                    var tentativeGScore = gScore[current] + HeuristicCost(current, neigborVertex);
                    if (!openSet.Contains(neigborVertex)) openSet.Add(neigborVertex);
                    else if (tentativeGScore >= gScore[neigborVertex]) continue;
                    cameFrom[neigborVertex] = current;
                    gScore[neigborVertex] = tentativeGScore;
                    fScore[neigborVertex] = tentativeGScore + HeuristicCost(neigborVertex, goal);                    
                }
                //proceed to next edge
                while ((neighborEdge = graph.Edges[neighborEdge.RotNext]) != originalEdge);
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
    }
}
