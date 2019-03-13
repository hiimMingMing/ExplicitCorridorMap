using ExplicitCorridorMap.Maths;
using ExplicitCorridorMap.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KdTree;
using KdTree.Math;
using RBush;

namespace ExplicitCorridorMap
{
    public class ECM : ECMCore
    {
        private KdTree<float, Vertex> KdTree { get; }

        public ECM(List<Obstacle> obstacles, Obstacle border) : base(obstacles, border)
        {
            KdTree = new KdTree<float, Vertex>(2, new FloatMath());
        }
        protected override void ConstructTree()
        {
            base.ConstructTree();
            //contruct kdtree
            foreach (var v in Vertices.Values)
            {
                KdTree.Add(v.KDKey, v);
            }
        }
        public Edge GetNearestEdge(Vector2 point)
        {
            var edges = RTree.Search(new Envelope(point.x, point.y, point.x, point.y));
            foreach (var edge in edges)
            {
                if (Geometry.PolygonContainsPoint(edge.Cell, point)) return edge;
            }
            return GetNearestVertex(point).Edges[0];
        }
        public Vertex GetNearestVertex(Vector2 point)
        {
            var pointArray = new float[] { point.x, point.y };
            var nodes = KdTree.GetNearestNeighbours(pointArray, 1);
            return nodes[0].Value;
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
            if (!e.IsTwin) RTree.Delete(e);
            Edges.Remove(e.ID);
        }
        private void DeleteVertex(Vertex v)
        {
            Vertices.Remove(v.ID);
            KdTree.RemoveAt(v.KDKey);
        }
        private void AddEdge(Edge e)
        {
            e.ID = CountEdges++;
            Edges[e.ID] = e;
            if (!e.IsTwin) RTree.Insert(e);
        }
        private void AddVertex(Vertex v)
        {
            v.ID = CountVertices++;
            Vertices[v.ID] = v;
            KdTree.Add(v.KDKey, v);
        }

        private void CheckOldVertex(Vertex v)
        {
            var node = KdTree.GetNearestNeighbours(v.KDKey, 1);
            if (node[0].Value.Position == v.Position)
            {
                node[0].Value.IsNew = true;
                v.OldVertex = node[0].Value;
            }
        }
        
        private void GetRelatedObstacles(Obstacle obstacle, out HashSet<Obstacle> obstacleSet, out Envelope extendedEnvelope)
        {
            var initialSelectedEdges = RTree.Search(obstacle.Envelope);
            //enlarge obstacle
            float maxSquareClearance = 0;
            foreach (var e in initialSelectedEdges)
            {
                var d1 = (e.Start.Position - e.LeftObstacleOfStart).sqrMagnitude;
                var d2 = (e.End.Position - e.LeftObstacleOfEnd).sqrMagnitude;
                if (d1 > maxSquareClearance) maxSquareClearance = d1;
                if (d2 > maxSquareClearance) maxSquareClearance = d2;
            }
            var maxClearance = Mathf.Sqrt(maxSquareClearance);

            //search again with extended envelope, find all obstacle and segment involves
            extendedEnvelope = Geometry.ExtendEnvelope(obstacle.Envelope, maxClearance);
            var oldEdges = RTree.Search(extendedEnvelope).ToList();
            obstacleSet = new HashSet<Obstacle>();
            foreach (var e in oldEdges)
            {
                var obs = RetrieveInputSegment(e).Parent;
                if (obs != null && !obs.IsBorder) obstacleSet.Add(obs);
                obs = RetrieveInputSegment(e.Twin).Parent;
                if (obs != null && !obs.IsBorder) obstacleSet.Add(obs);
                //DeleteEdge(e);
                //DeleteEdge(e.Twin);
            }
        }
        private void GetEdgesToReplace( Vertex startVertex,bool isOld,  HashSet<Vertex> vertices, HashSet<Edge> edges)
        {
            if (!startVertex.IsOldOrNew(isOld))
            {
                if(!vertices.Contains(startVertex)) vertices.Add(startVertex);
                foreach(var e in startVertex.Edges)
                {
                    if (!edges.Contains(e))
                    {
                        edges.Add(e);
                        GetEdgesToReplace( e.End,isOld,  vertices, edges);
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
                        GetEdgesToReplace( e.End,isOld,  vertices, edges);
                    }
                }
            }
        }
        private void GetEdgesToReplace(ECMCore ecm, Envelope envelope, bool isOld,out HashSet<Vertex> vertices,out HashSet<Edge> edges)
        {
            vertices = new HashSet<Vertex>();
            edges = new HashSet<Edge>();

            var nearestEdge = ecm.RTree.Search(envelope);
            Vertex startVertex = null;
            foreach(var e in nearestEdge)
            {
                if (!e.Start.IsOldOrNew(isOld)) {
                    startVertex = e.Start;
                    break;
                }
                if (!e.End.IsOldOrNew(isOld))
                {
                    startVertex = e.End;
                    break;
                }
            }
            if (startVertex == null) throw new Exception("Cannot find Start Vertex");
            GetEdgesToReplace( startVertex,isOld,  vertices, edges);
        }
        private void ComputeNewECMAndMerge(List<Obstacle> obstacleList, Envelope envelope)
        {
            foreach(var v in Vertices.Values) { v.IsNew = false; }
            //contruct new ECM
            var newECM = new ECMCore(obstacleList, this.Border);
            newECM.Construct();
            //update edge infos
            foreach (var v in newECM.Vertices.Values)
            {
                CheckOldVertex(v);
            }

            GetEdgesToReplace(newECM, envelope, true, out HashSet<Vertex> newVertices, out HashSet<Edge> newEdges);
            GetEdgesToReplace(this, envelope, false, out HashSet<Vertex> oldVertices, out HashSet<Edge> oldEdges);

            foreach (var e in oldEdges)
            {
                //Debug.Log(e);
                DeleteEdge(e);
            }
            ////add new edge and vertex to old ecm
            foreach (var v in newVertices)
            {
                //Debug.Log("B" + v);
                AddVertex(v);
                //Debug.Log("A" + v);
            }
            foreach (var e in newEdges)
            {
                //Debug.Log(e);
                if (e.Start.IsOld)
                {
                    e.Start = e.Start.OldVertex;
                    e.Start.Edges.Add(e);
                }
                if (e.End.IsOld) e.End = e.End.OldVertex;
                e.SiteID = newECM.RetrieveInputSegment(e).ID;
                AddEdge(e);
            }
            //Debug.Log("Start=====================================");
            //foreach (var v in Vertices.Values)
            //{
            //    Debug.Log(v + "   " + string.Join(",", v.Edges));
            //}
        }
        public void AddPolygonDynamic(Obstacle newObstacle)
        {
            GetRelatedObstacles(newObstacle, out HashSet<Obstacle> obstacleSet, out Envelope extendedEnvelope);

            obstacleSet.Add(newObstacle);
            this.AddObstacle(newObstacle);

            ComputeNewECMAndMerge(obstacleSet.ToList(), newObstacle.Envelope);


        }
        public void DeletePolygonDynamic(int obstacleID)
        {
            if (!Obstacles.ContainsKey(obstacleID)) throw new KeyNotFoundException("Obstacle ID not exists");
            var obstacle = Obstacles[obstacleID];

            GetRelatedObstacles(obstacle, out HashSet<Obstacle> obstacleSet, out Envelope extendedEnvelope);

            obstacleSet.Remove(obstacle);
            this.DeleteObstacle(obstacle);

            ComputeNewECMAndMerge(obstacleSet.ToList(), extendedEnvelope);
        }


        /// <summary>
        /// Generate a polyline representing a curved edge.
        /// </summary>
        /// <param name="edge">The curvy edge.</param>
        /// <param name="max_distance">The maximum distance between two vertex on the output polyline.</param>
        /// <returns></returns>
        public List<Vector2> SampleCurvedEdge(Edge edge, float max_distance)
        {
            //test
            return new List<Vector2>() { edge.Start.Position, edge.End.Position };

            Edge pointCell = null;
            Edge lineCell = null;

            //Max distance to be refined
            if (max_distance <= 0)
                throw new Exception("Max distance must be greater than 0");

            Vector2Int pointSite;
            Segment segmentSite;

            Edge twin = edge.Twin;

            if (edge.ContainsSegment == true && twin.ContainsSegment == true)
                return new List<Vector2>() { edge.Start.Position, edge.End.Position };

            if (edge.ContainsPoint)
            {
                pointCell = edge;
                lineCell = twin;
            }
            else
            {
                lineCell = edge;
                pointCell = twin;
            }

            pointSite = RetrieveInputPoint(pointCell);
            segmentSite = RetrieveInputSegment(lineCell);

            List<Vector2> discretization = new List<Vector2>(){
                edge.Start.Position,
                edge.End.Position
            };

            if (edge.IsLinear)
                return discretization;


            return ParabolaComputation.Densify(
                new Vector2(pointSite.x, pointSite.y),
                new Vector2(segmentSite.Start.x, segmentSite.Start.y),
                new Vector2(segmentSite.End.x, segmentSite.End.y),
                discretization[0],
                discretization[1],
                max_distance,
                0
            );
        }
    }
}
