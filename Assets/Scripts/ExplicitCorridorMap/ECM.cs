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
            RTree.Delete(e);
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
            RTree.Insert(e);
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
                //update this vertex
                v.OldVertex = node[0].Value;
            }
        }
        private void UpdateEdge(Edge e, HashSet<Vertex> newVertices)
        {
            if (e.Start.OldVertex != null)
            {
                e.Start = e.Start.OldVertex;
                e.Start.Edges.Add(e);
            }
            else newVertices.Add(e.Start);
            if (e.End.OldVertex != null)
            {
                e.End = e.End.OldVertex;
            }
            else newVertices.Add(e.End);
        }
        //Deleted all affected edges and return Obstacle Set with extended envelope for later use
        private HashSet<Obstacle> DeleteAffectedEdge(Obstacle obstacle, out Envelope extendedEnvelope)
        {
            var selectedEdges = RTree.Search(obstacle.Envelope);
            //enlarge obstacle
            float maxSquareClearance = 0;
            foreach (var e in selectedEdges)
            {
                var d1 = (e.Start.Position - e.LeftObstacleOfStart).sqrMagnitude;
                var d2 = (e.End.Position - e.LeftObstacleOfEnd).sqrMagnitude;
                if (d1 > maxSquareClearance) maxSquareClearance = d1;
                if (d2 > maxSquareClearance) maxSquareClearance = d2;
            }
            var maxClearance = Mathf.Sqrt(maxSquareClearance);
            extendedEnvelope = Geometry.ExtendEnvelope(obstacle.Envelope, maxClearance);

            //search again with extended envelope, find all obstacle and segment involves
            selectedEdges = RTree.Search(extendedEnvelope);
            var obstacleSet = new HashSet<Obstacle>();
            foreach (var e in selectedEdges)
            {
                var obs = RetrieveInputSegment(e).Parent;
                if (obs != null && !obs.IsBorder) obstacleSet.Add(obs);
                obs = RetrieveInputSegment(e.Twin).Parent;
                if (obs != null && !obs.IsBorder) obstacleSet.Add(obs);
                DeleteEdge(e);
                DeleteEdge(e.Twin);
            }
            return obstacleSet;
        }
        private void ComputeNewECMAndMerge(List<Obstacle> obstacleList, Envelope extendedEnvelope)
        {
            //contruct new ECM
            var newECM = new ECMCore(obstacleList, this.Border);
            newECM.Construct();
            var newEdges = newECM.RTree.Search(extendedEnvelope);
            //update edge infos
            foreach (var v in newECM.Vertices.Values)
            {
                CheckOldVertex(v);
            }
            var newVertices = new HashSet<Vertex>();
            foreach (var e in newEdges)
            {
                UpdateEdge(e, newVertices);
                UpdateEdge(e.Twin, newVertices);
                e.SiteID = newECM.RetrieveInputSegment(e).ID;
                e.Twin.SiteID = newECM.RetrieveInputSegment(e.Twin).ID;
            }
            //add new edge and vertex to old ecm
            foreach (var v in newVertices)
            {
                AddVertex(v);
            }
            foreach (var e in newEdges)
            {
                AddEdge(e);
                AddEdge(e.Twin);
            }
        }
        public void AddPolygonDynamic(Obstacle newObstacle)
        {
            Envelope extendedEnvelope;
            var obstacleList = DeleteAffectedEdge(newObstacle, out extendedEnvelope).ToList();

            obstacleList.Add(newObstacle);
            this.AddObstacle(newObstacle);

            ComputeNewECMAndMerge(obstacleList, extendedEnvelope);
        }
        public void DeletePolygonDynamic(int obstacleID)
        {
            if (!Obstacles.ContainsKey(obstacleID)) throw new KeyNotFoundException("Obstacle ID not exists");
            var obstacle = Obstacles[obstacleID];

            Envelope extendedEnvelope;
            var obstacleSet = DeleteAffectedEdge(obstacle, out extendedEnvelope);

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
            //return new List<Vector2>() { edge.Start.Position, edge.End.Position };

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
