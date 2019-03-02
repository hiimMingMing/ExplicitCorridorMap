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


namespace ExplicitCorridorMap
{
    public class ECM
    {
        public Dictionary<int, Vector2Int> InputPoints { get; private set; }
        public Dictionary<int, Segment> InputSegments { get; private set; }
        public Dictionary<int, Vertex> Vertices { get; }
        public Dictionary<int, Edge> Edges { get; }
        
        public List<RectInt> Obstacles { get; }
        private KdTree<float, Vertex> KdTree { get; }
        public ECM(List<RectInt> obstacles)
        {
            InputPoints = new Dictionary<int, Vector2Int>();
            InputSegments = new Dictionary<int, Segment>();
            Vertices = new Dictionary<int, Vertex>();
            Edges = new Dictionary<int, Edge>();
            Obstacles = obstacles;
            KdTree = new KdTree<float, Vertex>(2, new FloatMath());

            foreach (var rect in obstacles)
            {
                AddRect(rect);
            }
        }
        public ECM()
        {
            InputPoints = new Dictionary<int, Vector2Int>();
            InputSegments = new Dictionary<int, Segment>();
            Vertices = new Dictionary<int, Vertex>();
            Edges = new Dictionary<int, Edge>();
            Obstacles = new List<RectInt>();
        }
        public void Construct()
        {
            using (BoostVoronoi bv = new BoostVoronoi())
            {
                foreach (var point in InputPoints.Values)
                    bv.AddPoint(point.x, point.y);
                foreach (var segment in InputSegments.Values)
                    bv.AddSegment(segment.Start.x, segment.Start.y, segment.End.x, segment.End.y);
                bv.Construct();
                var CountVertices = bv.CountVertices;
                var CountEdges = bv.CountEdges;
                var CountCells = bv.CountCells;

                for (int i = 0; i < CountVertices; i++)
                {
                    var vertex = bv.GetVertex(i);
                    foreach(var obs in Obstacles)
                    {
                        if (obs.Contains(Vector2Int.CeilToInt(vertex.Position))){
                            vertex.IsInside = true;
                            break;
                        }
                    }
                    Vertices.Add(i, vertex);
                }
                //add edges to vertex
                for(int i=0 ; i< CountEdges;i += 2)
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
                    var ecmEdge = new Edge(start, end, edge,cell);
                    var ecmEdgeTwin = new Edge(end, start, twinEdge,twinCell);

                    ecmEdge.Twin = ecmEdgeTwin;
                    ecmEdgeTwin.Twin = ecmEdge;
                    start.Edges.Add(ecmEdge);
                    end.Edges.Add(ecmEdgeTwin);
                    Edges.Add(i, ecmEdge);
                    Edges.Add(i+1, ecmEdgeTwin);

                }
                foreach (var vertex in Vertices.Values.ToList())
                {
                    if (vertex.Edges.Count == 0) Vertices.Remove(vertex.ID);
                }

                foreach (var edge in Edges.Values)
                {
                    if (edge.ID % 2 == 0 ) ComputeObstaclePoint(edge);
                    
                }
                //contruct kdtree
                foreach(var v in Vertices.Values)
                {
                    KdTree.Add(v.GetKDKey(), v);
                }
            }
            
        }
        public Edge GetNearestEdge(Vector2 point)
        {
            foreach(var edge in Edges.Values)
            {
                if (edge.ID % 2 == 0)
                {
                    var isInside = Geometry.PolygonContainsPoint(edge.Cell, point);
                    if (isInside) return edge;
                }
            }
            return GetNearestVertex(point).Edges[0];
        }
        public Vertex GetNearestVertex(Vector2 point)
        {
            var pointArray = new float[] { point.x, point.y };
            var nodes = KdTree.GetNearestNeighbours(pointArray, 1);
            return nodes[0].Value;
        }
        public void ComputeObstaclePoint(Edge edge)
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
        }
        private void ComputeObstaclePoint(Edge cell, Edge edge, out Vector2 start, out Vector2 end)
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
        public void AddPoint(int x, int y)
        {
            Vector2Int p = new Vector2Int(x, y);
            InputPoints[InputPoints.Count] =  p;
        }
        public void AddSegment(Segment s)
        {
            InputSegments[InputSegments.Count] = s;
        }
        public void AddRect(RectInt rect)
        {
            AddSegment(new Segment(rect.x, rect.y, rect.x, rect.yMax));
            AddSegment(new Segment(rect.x, rect.yMax, rect.xMax, rect.yMax));
            AddSegment(new Segment(rect.xMax, rect.yMax, rect.xMax, rect.y));
            AddSegment(new Segment(rect.xMax, rect.y, rect.x, rect.y));
        }
        #region Code to discretize curves
        //The code below is a simple port to C# of the C++ code in the links below
        //http://www.boost.org/doc/libs/1_54_0/libs/polygon/example/voronoi_visualizer.cpp
        //http://www.boost.org/doc/libs/1_54_0/libs/polygon/example/voronoi_visual_utils.hpp

        /// <summary>
        /// Generate a polyline representing a curved edge.
        /// </summary>
        /// <param name="edge">The curvy edge.</param>
        /// <param name="max_distance">The maximum distance between two vertex on the output polyline.</param>
        /// <returns></returns>
        public List<Vector2> SampleCurvedEdge(Edge edge, float max_distance)
        {
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
            if (cell.SourceCategory == SourceCategory.SinglePoint)
                pointNoScaled = InputPoints[cell.SiteID];
            else if (cell.SourceCategory == SourceCategory.SegmentStartPoint)
                pointNoScaled = InputSegments[RetriveInputSegmentIndex(cell)].Start;
            else if (cell.SourceCategory == SourceCategory.SegmentEndPoint)
                pointNoScaled = InputSegments[RetriveInputSegmentIndex(cell)].End;
            else
                throw new Exception("This cells does not have a point as input site");

            return new Vector2Int(pointNoScaled.x, pointNoScaled.y);
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
            Segment segmentNotScaled = InputSegments[RetriveInputSegmentIndex(cell)];
            return new Segment(new Vector2Int(segmentNotScaled.Start.x, segmentNotScaled.Start.y),
                new Vector2Int(segmentNotScaled.End.x, segmentNotScaled.End.y));
        }

        private int RetriveInputSegmentIndex(Edge cell)
        {
            if (cell.SourceCategory == SourceCategory.SinglePoint)
                throw new Exception("Attempting to retrive an input segment on a cell that was built around a point");
            return cell.SiteID - InputPoints.Count;
        }

        #endregion
    }
}
