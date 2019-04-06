using ExplicitCorridorMap.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Advanced.Algorithms.DataStructures;

namespace ExplicitCorridorMap
{
    public class ECM : ECMCore
    {
        public Dictionary<Vector2, Vertex> MapVertices { get; }
        public List<float> AgentRadius { get; }
        public ECM(List<Obstacle> obstacles, Obstacle border) : base(obstacles, border)
        {
            MapVertices = new Dictionary<Vector2, Vertex>();
            AgentRadius = new List<float>();
        }
        protected override void ConstructTree()
        {
            base.ConstructTree();
            //contruct kdtree
            foreach (var v in Vertices.Values)
            {
                MapVertices.Add(v.Position, v);
            }
        }
        public void AddAgentRadius(List<float> radiusList)
        {
            foreach (var r in radiusList)
            {
                AgentRadius.Add(r);
                foreach (var e in Edges.Values)
                {
                    e.AddProperty(r);
                }
            }
        }
        public void AddAgentRadius(float radius)
        {
            AgentRadius.Add(radius);
            foreach (var e in Edges.Values)
            {
                e.AddProperty(radius);
            }
        }
        public Edge GetNearestEdge(Vector2 point)
        {
            var edges = RTreeEdge.RangeSearch(new Rectangle(point));
            foreach (var edge in edges)
            {
                if (Geometry.PolygonContainsPoint(edge.Cell, point)) return edge;
            }
            return null;
        }
        public Edge GetNearestEdge(ref Vector2 center, float radius)
        {
            //check if it intersect with obstacle
            var obsList = RTreeObstacle.RangeSearch(new Rectangle(center,radius));
            if (obsList.Count != 0)
            {
                float minDistance = float.MaxValue;
                Vector2 closestPointOnObstacle = Vector2.zero;
                Obstacle closestObstacle = null;
                foreach (var obs in obsList)
                {
                    foreach (var s in obs.Segments)
                    {
                        var closestPoint = Distance.GetClosestPointOnLine(s.Start, s.End, center, out float d);
                        if (d < minDistance)
                        {
                            minDistance = d;
                            closestPointOnObstacle = closestPoint;
                            closestObstacle = obs;
                        }
                    }
                }
                //move away from obstacle
                if (Geometry.PolygonContainsPoint(closestObstacle.Points, center))
                {
                    center = closestPointOnObstacle + radius * (closestPointOnObstacle - center).normalized;
                }
                else if (minDistance < radius)
                {
                    center = closestPointOnObstacle + radius * (center - closestPointOnObstacle).normalized;
                }
            }
            var edges = RTreeEdge.RangeSearch(new Rectangle(center));
            foreach (var edge in edges)
            {
                if (Geometry.PolygonContainsPoint(edge.Cell, center)) return edge;
            }
            return null;
        }

        protected override void AddSegment(Segment s)
        {
            s.ID = CountSegments++;
            InputSegments[s.ID] = s;
        }
        protected override void AddObstacle(Obstacle obs)
        {
            AddSegment(obs.Segments);
            obs.ID = CountObstacles++;
            Obstacles[obs.ID] = obs;
            RTreeObstacle.Insert(obs);
        }
        private void DeleteObstacle(Obstacle obs)
        {
            foreach (var seg in obs.Segments)
            {
                InputSegments.Remove(seg.ID);
            }
            Obstacles.Remove(obs.ID);
            RTreeObstacle.Delete(obs);
        }
        private void DeleteEdge(Edge e)
        {
            e.Start.Edges.Remove(e);
            if (e.Start.Edges.Count == 0) DeleteVertex(e.Start);
            if (!e.IsTwin) RTreeEdge.Delete(e);
            Edges.Remove(e.ID);
        }
        private void DeleteVertex(Vertex v)
        {
            Vertices.Remove(v.ID);
            MapVertices.Remove(v.Position);
        }
        private void AddEdge(Edge e)
        {
            e.ID = CountEdges++;
            Edges[e.ID] = e;
            if (!e.IsTwin) RTreeEdge.Insert(e);
        }
        private void AddVertex(Vertex v)
        {
            v.ID = CountVertices++;
            Vertices[v.ID] = v;
            MapVertices.Add(v.Position, v);
        }

        private void CheckOldVertex(Vertex v)
        {
            if (MapVertices.ContainsKey(v.Position))
            {
                var oldVertex = MapVertices[v.Position];
                oldVertex.IsLinked = true;
                v.IsLinked = true;
                v.OldVertex = oldVertex;
            }
        }

        private void GetRelatedObstacles(Obstacle obstacle, out List<Obstacle> obstacles, out IReadOnlyList<Edge> selectedEdges)
        {
            //selecte all edges intersect with obstacle
            selectedEdges = RTreeEdge.RangeSearch(obstacle.mBRectangle);
            //enlarge obstacle
            float maxClearance = 0;
            foreach (var e in selectedEdges)
            {
                var d1 = e.ClearanceOfStart;
                var d2 = e.ClearanceOfEnd;
                if (d1 > maxClearance) maxClearance = d1;
                if (d2 > maxClearance) maxClearance = d2;
            }

            //search again with extended envelope, find all obstacle and segment involves
            var extendedRectangle = Geometry.ExtendRectangle(obstacle.mBRectangle, maxClearance);
            var edges = RTreeEdge.RangeSearch(extendedRectangle);

            var obstacleSet = new HashSet<Obstacle>();
            foreach (var e in edges)
            {
                foreach (var ei in e.Start.Edges)
                {
                    var obs = RetrieveInputSegment(ei).Parent;
                    if (!obs.IsBorder) obstacleSet.Add(obs);
                    obs = RetrieveInputSegment(ei.Twin).Parent;
                    if (!obs.IsBorder) obstacleSet.Add(obs);
                }
                foreach (var ei in e.End.Edges)
                {
                    var obs = RetrieveInputSegment(ei).Parent;
                    if (!obs.IsBorder) obstacleSet.Add(obs);
                    obs = RetrieveInputSegment(ei.Twin).Parent;
                    if (!obs.IsBorder) obstacleSet.Add(obs);
                }
            }
            obstacles = obstacleSet.ToList();

        }
        private void GetEdgesToReplace(Vertex startVertex, HashSet<Vertex> vertices, HashSet<Edge> edges)
        {
            if (!startVertex.IsLinked)
            {
                if (!vertices.Contains(startVertex)) vertices.Add(startVertex);
                foreach (var e in startVertex.Edges)
                {
                    if (!edges.Contains(e))
                    {
                        edges.Add(e);
                        GetEdgesToReplace(e.End, vertices, edges);
                    }
                }
            }
            else
            {
                foreach (var e in startVertex.Edges)
                {
                    if (!edges.Contains(e) && vertices.Contains(e.End))
                    {
                        edges.Add(e);
                        GetEdgesToReplace(e.End, vertices, edges);
                    }
                }
            }
        }
        private void GetEdgesToReplace(ECMCore ecm, IReadOnlyList<Edge> selectedEdges, out HashSet<Vertex> vertices, out HashSet<Edge> edges)
        {

            Vertex startVertex = null;
            foreach (var e in selectedEdges)
            {
                if (!e.Start.IsLinked)
                {
                    startVertex = e.Start;
                    break;
                }
                if (!e.End.IsLinked)
                {
                    startVertex = e.End;
                    break;
                }
            }

            vertices = new HashSet<Vertex>();
            edges = new HashSet<Edge>();
            if (startVertex == null)
            {
                foreach (var e in selectedEdges)
                {
                    edges.Add(e);
                    edges.Add(e.Twin);
                }
            }
            else GetEdgesToReplace(startVertex, vertices, edges);
        }
        private void ComputeNewECMAndMerge(List<Obstacle> obstacleList, Rectangle mBRectangle, IReadOnlyList<Edge> selectedEdges)
        {
            foreach (var v in Vertices.Values) { v.IsLinked = false; }
            //contruct new ECM
            var newECM = new ECMCore(obstacleList, this.Border);
            newECM.Construct();
            //update edge infos
            foreach (var v in newECM.Vertices.Values)
            {
                CheckOldVertex(v);
            }
            //Find all edges of new ECM to add
            var newSelectedEdges = newECM.RTreeEdge.RangeSearch(mBRectangle);
            //Debug.Log("New");
            GetEdgesToReplace(newECM, newSelectedEdges, out HashSet<Vertex> newVertices, out HashSet<Edge> newEdges);
            //Debug.Log("Old");
            //Find all edges of old ECM to remove
            GetEdgesToReplace(this, selectedEdges, out HashSet<Vertex> oldVertices, out HashSet<Edge> oldEdges);

            foreach (var e in oldEdges)
            {
                //Debug.Log("D "+e);
                DeleteEdge(e);
            }
            //add new edge and vertex to old ecm
            foreach (var v in newVertices)
            {
                AddVertex(v);
            }
            foreach (var e in newEdges)
            {
                if (e.Start.IsLinked)
                {
                    e.Start = e.Start.OldVertex;
                    e.Start.Edges.Add(e);
                }
                if (e.End.IsLinked) e.End = e.End.OldVertex;
                e.SiteID = newECM.RetrieveInputSegment(e).ID;
                foreach (var r in AgentRadius)
                {
                    e.AddProperty(r);
                }
                AddEdge(e);
            }
        }
        public void AddPolygonDynamic(Obstacle obstacle)
        {
            GetRelatedObstacles(obstacle, out List<Obstacle> obstacles, out IReadOnlyList<Edge> selectedEdges);

            obstacles.Add(obstacle);
            this.AddObstacle(obstacle);

            ComputeNewECMAndMerge(obstacles, obstacle.mBRectangle, selectedEdges);

            //foreach(var v in Vertices.Values)
            //{
            //    string es = "";
            //    foreach(var e in v.Edges)
            //    {
            //        es += e.End;
            //        es += "O" + RetrieveInputSegment(e).Parent.ID;
            //        es += ", ";
            //    }
            //    Debug.Log(v + ":   "+es);
            //}
        }
        public void DeletePolygonDynamic(int obstacleID)
        {
            if (!Obstacles.ContainsKey(obstacleID)) throw new KeyNotFoundException("Obstacle ID not exists");
            var obstacle = Obstacles[obstacleID];

            GetRelatedObstacles(obstacle, out List<Obstacle> obstacles, out IReadOnlyList<Edge> selectedEdges);

            obstacles.Remove(obstacle);
            this.DeleteObstacle(obstacle);
            ComputeNewECMAndMerge(obstacles, obstacle.mBRectangle, selectedEdges);
        }

    }
}
