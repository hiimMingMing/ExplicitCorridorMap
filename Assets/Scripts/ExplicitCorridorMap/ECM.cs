using ExplicitCorridorMap.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class ECM
    {
        public Dictionary<long, Vector2Int> InputPoints { get; private set; }
        public Dictionary<long, Segment> InputSegments { get; private set; }
        public Dictionary<long, Vertex> Vertices { get; } = new Dictionary<long, Vertex>();
        public Dictionary<long, Edge> Edges { get; } = new Dictionary<long, Edge>();
        public Dictionary<long, Cell> Cells { get; } = new Dictionary<long, Cell>();
        public ECM()
        {
            InputPoints = new Dictionary<long, Vector2Int>();
            InputSegments = new Dictionary<long, Segment>();
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

                for (long i = 0; i < CountVertices; i++)
                {
                    var vertex = bv.GetVertex(i);
                    Vertices.Add(i, vertex);
                }
                for (long i = 0; i < CountEdges; i++)
                {
                    var edge = bv.GetEdge(i);
                    Edges.Add(i, edge);
                }
                for (long i = 0; i < CountCells; i++)
                {
                    var cell = bv.GetCell(i);
                    Cells.Add(i, cell);
                }
                foreach (var edge in Edges.Values)
                {
                    if (edge.ID % 2 == 1 || !edge.IsFinite || !edge.IsPrimary) continue;
                    var twinEdge = Edges[edge.Twin];
                    var cell = Cells[edge.Cell];
                    var twinCell = Cells[twinEdge.Cell];

                    ComputeObstaclePoint(cell, edge, out Vector2 obsLeftStart, out Vector2 obsLeftEnd);
                    edge.LeftObstacleStart = obsLeftStart;
                    edge.LeftObstacleEnd = obsLeftEnd;

                    ComputeObstaclePoint(twinCell, edge, out Vector2 obsRightStart, out Vector2 obsRightEnd);
                    edge.RightObstacleStart = obsRightStart;
                    edge.RightObstacleEnd = obsRightEnd;

                    twinEdge.LeftObstacleStart = edge.RightObstacleEnd;
                    twinEdge.LeftObstacleEnd = edge.RightObstacleStart;
                    twinEdge.RightObstacleStart = edge.LeftObstacleEnd;
                    twinEdge.RightObstacleEnd = edge.LeftObstacleStart;
                }
            }
            
        }
        private void ComputeObstaclePoint(Cell cell, Edge edge, out Vector2 start, out Vector2 end)
        {
            var startVertex = Vertices[edge.Start];
            var endVertex = Vertices[edge.End];
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
                var nearestPointOfStartVertex = Distance.GetClosestPointOnLine(startLineSite, endLineSite, startVertex.ToVector2());
                var nearestPointOfEndVertex = Distance.GetClosestPointOnLine(startLineSite, endLineSite, endVertex.ToVector2());

                start = nearestPointOfStartVertex;
                end = nearestPointOfEndVertex;
            }
        }
        public void AddPoint(int x, int y)
        {
            Vector2Int p = new Vector2Int(x, y);
            InputPoints[InputPoints.Count] =  p;
        }
        public void AddSegment(int x1, int y1, int x2, int y2)
        {
            Segment s = new Segment(x1, y1, x2, y2);
            InputSegments[InputSegments.Count] = s;
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
            long pointCell = -1;
            long lineCell = -1;

            //Max distance to be refined
            if (max_distance <= 0)
                throw new Exception("Max distance must be greater than 0");

            Vector2Int pointSite;
            Segment segmentSite;

            Edge twin = this.Edges[edge.Twin];
            Cell m_cell = this.Cells[edge.Cell];
            Cell m_reverse_cell = this.Cells[twin.Cell];

            if (m_cell.ContainsSegment == true && m_reverse_cell.ContainsSegment == true)
                return new List<Vector2>() { this.Vertices[edge.Start].ToVector2(), this.Vertices[edge.End].ToVector2() };

            if (m_cell.ContainsPoint)
            {
                pointCell = edge.Cell;
                lineCell = twin.Cell;
            }
            else
            {
                lineCell = edge.Cell;
                pointCell = twin.Cell;
            }

            pointSite = RetrieveInputPoint(this.Cells[pointCell]);
            segmentSite = RetrieveInputSegment(this.Cells[lineCell]);

            List<Vector2> discretization = new List<Vector2>(){
                this.Vertices[edge.Start].ToVector2(),
                this.Vertices[edge.End].ToVector2()
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
        public Vector2Int RetrieveInputPoint(Cell cell)
        {
            Vector2Int pointNoScaled;
            if (cell.SourceCategory == CellSourceCatory.SinglePoint)
                pointNoScaled = InputPoints[cell.Site];
            else if (cell.SourceCategory == CellSourceCatory.SegmentStartPoint)
                pointNoScaled = InputSegments[RetriveInputSegmentIndex(cell)].Start;
            else if (cell.SourceCategory == CellSourceCatory.SegmentEndPoint)
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
        public Segment RetrieveInputSegment(Cell cell)
        {
            Segment segmentNotScaled = InputSegments[RetriveInputSegmentIndex(cell)];
            return new Segment(new Vector2Int(segmentNotScaled.Start.x, segmentNotScaled.Start.y),
                new Vector2Int(segmentNotScaled.End.x, segmentNotScaled.End.y));
        }

        private long RetriveInputSegmentIndex(Cell cell)
        {
            if (cell.SourceCategory == CellSourceCatory.SinglePoint)
                throw new Exception("Attempting to retrive an input segment on a cell that was built around a point");
            return cell.Site - InputPoints.Count;
        }

        #endregion
    }
}
