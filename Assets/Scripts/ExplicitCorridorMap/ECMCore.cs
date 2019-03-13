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
    public class ECMCore
    {
        public Dictionary<int, Segment> InputSegments { get; private set; }
        public Dictionary<int, Vertex> Vertices { get; }
        public Dictionary<int, Edge> Edges { get; }

        public Dictionary<int,Obstacle> Obstacles { get; }
        public RBush<Edge> RTree { get; }
        public RBush<Obstacle> RTreeObstacle { get; }
        public Obstacle Border { get; set; }
        protected int CountVertices;
        protected int CountEdges;
        protected int CountSegments;
        protected int CountObstacles;
        public ECMCore(List<Obstacle> obstacles, Obstacle border)
        {
            CountSegments = 0;
            CountObstacles = 0;
            InputSegments = new Dictionary<int, Segment>();
            Vertices = new Dictionary<int, Vertex>();
            Edges = new Dictionary<int, Edge>();
            RTree = new RBush<Edge>(3);

            Obstacles = new Dictionary<int, Obstacle>();
            RTreeObstacle = new RBush<Obstacle>();
            foreach (var obs in obstacles)
            {
                AddObstacle(obs);
            }
            AddBorder(border);
        }
        public void Construct()
        {
            using (BoostVoronoi bv = new BoostVoronoi())
            {

                foreach (var segment in InputSegments.Values)
                    bv.AddSegment(segment.Start.x, segment.Start.y, segment.End.x, segment.End.y);
                bv.Construct();
                CountVertices = bv.CountVertices;
                CountEdges = bv.CountEdges;

                for (int i = 0; i < CountVertices; i++)
                {
                    var vertex = bv.GetVertex(i);
                    if (InObstacleSpace(vertex.Position))
                    {
                        vertex.IsInside = true;
                    }
                    Vertices.Add(i, vertex);
                }
                //add edges to vertex
                for (int i = 0; i < CountEdges; i += 2)
                {
                    var edge = bv.GetEdge(i);
                    if (!edge.IsFinite || !edge.IsPrimary) continue;
                    var start = Vertices[edge.Start];
                    var end = Vertices[edge.End];
                    if (start.IsInside || end.IsInside) continue;

                    var twinEdge = bv.GetEdge(i + 1);
                    var cell = bv.GetCell(edge.Cell);
                    var twinCell = bv.GetCell(twinEdge.Cell);
                    //init 2 twin ecm edge
                    var ecmEdge = new Edge(start, end, edge, cell);
                    var ecmEdgeTwin = new Edge(end, start, twinEdge, twinCell);

                    ecmEdge.Twin = ecmEdgeTwin;
                    ecmEdgeTwin.Twin = ecmEdge;
                    ecmEdgeTwin.IsTwin = true;
                    start.Edges.Add(ecmEdge);
                    end.Edges.Add(ecmEdgeTwin);
                    Edges.Add(i, ecmEdge);
                    Edges.Add(i + 1, ecmEdgeTwin);

                }
            }
            ConstructTree();
        }
        protected virtual void ConstructTree()
        {
            //remove used vertex
            foreach (var vertex in Vertices.Values.ToList())
            {
                if (vertex.Edges.Count == 0) Vertices.Remove(vertex.ID);
            }
            //Compute nearest obstacle point and contruct RTree
            foreach (var edge in Edges.Values)
            {
                if (!edge.IsTwin)
                {
                    ComputeObstaclePoint(edge);
                    RTree.Insert(edge);
                }
            }

        }

        protected void ComputeObstaclePoint(Edge edge)
        {
            var twinEdge = edge.Twin;

            ComputeObstaclePoint(edge, edge, out Vector2 obsLeftStart, out Vector2 obsLeftEnd);
            edge.LeftObstacleOfStart = obsLeftStart;
            edge.LeftObstacleOfEnd = obsLeftEnd;

            ComputeObstaclePoint(twinEdge, edge, out Vector2 obsRightStart, out Vector2 obsRightEnd);
            edge.RightObstacleOfStart = obsRightStart;
            edge.RightObstacleOfEnd = obsRightEnd;

            twinEdge.LeftObstacleOfStart = edge.RightObstacleOfEnd;
            twinEdge.LeftObstacleOfEnd = edge.RightObstacleOfStart;
            twinEdge.RightObstacleOfStart = edge.LeftObstacleOfEnd;
            twinEdge.RightObstacleOfEnd = edge.LeftObstacleOfStart;

            edge.ComputeCell();
            twinEdge.ComputeCell();
            edge.ComputeEnvelope();
            twinEdge.SetEnvelope(edge.Envelope);
        }
        protected void ComputeObstaclePoint(Edge cell, Edge edge, out Vector2 start, out Vector2 end)
        {
            var startVertex = edge.Start;
            var endVertex = edge.End;
            if (cell.ContainsPoint)
            {
                var pointSite = RetrieveInputPoint(cell);
                start = pointSite;
                end = pointSite;
            }
            else
            {
                var lineSite = RetrieveInputSegment(cell);
                var startLineSite = new Vector2(lineSite.Start.x, lineSite.Start.y);
                var endLineSite = new Vector2(lineSite.End.x, lineSite.End.y);
                var nearestPointOfStartVertex = Distance.GetClosestPointOnLine(startLineSite, endLineSite, startVertex.Position);
                var nearestPointOfEndVertex = Distance.GetClosestPointOnLine(startLineSite, endLineSite, endVertex.Position);

                start = nearestPointOfStartVertex;
                end = nearestPointOfEndVertex;
            }
        }
        protected virtual void AddSegment(Segment s)
        {
            InputSegments[CountSegments++] = s;
        }
        protected virtual void AddObstacle(Obstacle obs)
        {
            AddSegment(obs.Segments);
            Obstacles[CountObstacles++] = obs;
            RTreeObstacle.Insert(obs);
        }
        protected void AddSegment(List<Segment> segs)
        {
            foreach (var s in segs)
            {
                AddSegment(s);
            }
        }
        protected void AddBorder(Obstacle border)
        {
            border.IsBorder = true;
            Border = border;
            AddSegment(border.Segments);
        }
       
        protected bool InObstacleSpace(Vector2 point)
        {
            var result = RTreeObstacle.Search(new Envelope(point.x, point.y, point.x, point.y));
            foreach (var o in result)
            {
                if (o.ContainsPoint(point)) return true;
            }
            return false;
        }


        /// <summary>
        ///  Retrieve the input point site asssociated with a cell. The point returned is the one
        ///  sent to boost. If a scale factor was used, then the output coordinates should be divided by the
        ///  scale factor. An exception will be returned if this method is called on a cell that does
        ///  not contain a point site.
        /// </summary>
        /// <param name="cell">The cell that contains the point site.</param>
        /// <returns>The input point site of the cell.</returns>
        public Vector2Int RetrieveInputPoint(Edge cell)
        {
            Vector2Int pointNoScaled;
            if (cell.SourceCategory == SourceCategory.SegmentStartPoint)
                pointNoScaled = InputSegments[RetrieveInputSegmentIndex(cell)].Start;
            else if (cell.SourceCategory == SourceCategory.SegmentEndPoint)
                pointNoScaled = InputSegments[RetrieveInputSegmentIndex(cell)].End;
            else
                throw new Exception("This cells does not have a point as input site");

            return pointNoScaled;
        }


        /// <summary>
        ///  Retrieve the input segment site asssociated with a cell. The segment returned is the one
        ///  sent to boost. If a scale factor was used, then the output coordinates should be divided by the
        ///  scale factor. An exception will be returned if this method is called on a cell that does
        ///  not contain a segment site.
        /// </summary>
        /// <param name="cell">The cell that contains the segment site.</param>
        /// <returns>The input segment site of the cell.</returns>
        public Segment RetrieveInputSegment(Edge cell)
        {
            return InputSegments[RetrieveInputSegmentIndex(cell)];
        }

        public int RetrieveInputSegmentIndex(Edge cell)
        {
            if (cell.SourceCategory == SourceCategory.SinglePoint)
                throw new Exception("Attempting to retrive an input segment on a cell that was built around a point");
            return cell.SiteID;
        }

    }
}
