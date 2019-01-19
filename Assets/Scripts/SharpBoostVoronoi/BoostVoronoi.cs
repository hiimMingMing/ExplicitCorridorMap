﻿using SharpBoostVoronoi.Parabolas;
using SharpBoostVoronoi.Exceptions;
using SharpBoostVoronoi.Input;
using SharpBoostVoronoi.Output;
using SharpBoostVoronoi.Maths;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SharpBoostVoronoi
{
    public class BoostVoronoi : IDisposable
    {
        #region DLL_IMPORT
        [DllImport("BoostVoronoi")]
        private static extern IntPtr CreateVoronoiWraper();
        [DllImport("BoostVoronoi")]
        private static extern void DeleteVoronoiWrapper(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern void AddPoint(IntPtr v, int x, int y);
        [DllImport("BoostVoronoi")]
        private static extern void AddSegment(IntPtr v, int x1, int y1, int x2, int y2);
        [DllImport("BoostVoronoi")]
        private static extern void Construct(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern void Clear(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern long GetCountVertices(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern long GetCountEdges(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern long GetCountCells(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern void CreateVertexMap(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern void CreateEdgeMap(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern void CreateCellMap(IntPtr v);
        [DllImport("BoostVoronoi")]
        private static extern void GetVertex(IntPtr v, long index, 
            out double a1,
            out double a2,
            out long a3);
        [DllImport("BoostVoronoi")]
        private static extern void GetEdge(IntPtr v, long index, 
            out long a1,
            out long a2,
            out bool a3,
            out bool a4,
            out bool a5,
            out long a6,
            out long a7,
            out long a8,
            out long a9,
            out long a10,
            out long a11);
        [DllImport("BoostVoronoi")]
        private static extern void GetCell(IntPtr v, long index, 
            out long a1,
            out short a2,
            out bool a3,
            out bool a4,
            out bool a5,
            out long a6);
        #endregion
        public bool disposed = false;


        private const int BUFFER_SIZE = 15;
        /// <summary>
        /// The reference to the CLR wrapper class
        /// </summary>
        private IntPtr VoronoiWrapper { get; set; }

        /// <summary>
        /// The input points used to construct the voronoi diagram
        /// </summary>
        public Dictionary<long, Vector2Int> InputPoints { get; private set; }

        /// <summary>
        /// The input segments used to construct the voronoi diagram
        /// </summary>
        public Dictionary<long, Segment> InputSegments { get; private set; }

        /// <summary>
        /// A property used to define tolerance to parabola interpolation.
        /// </summary>
        public float Tolerance { get; set; }


        public long CountVertices { get; private set; }
        public long CountEdges { get; private set; }
        public long CountCells { get; private set; }
        public Dictionary<long, Vertex> Vertices { get; } = new Dictionary<long, Vertex>();
        public Dictionary<long, Edge> Edges { get; } = new Dictionary<long, Edge>();
        public Dictionary<long, Cell> Cells { get; } = new Dictionary<long, Cell>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoostVoronoi()
        {
            InputPoints = new Dictionary<long, Vector2Int>();
            InputSegments = new Dictionary<long, Segment>();
            VoronoiWrapper = CreateVoronoiWraper();
            CountVertices = -1;
            CountEdges = -1;
            CountCells = -1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            //if (disposing)
            //{
            //    //Free managed object here
            //
            //}

            // Free any unmanaged objects here.
            DeleteVoronoiWrapper(VoronoiWrapper);

            disposed = true;
        }




        /// <summary>
        /// Calls the voronoi API in order to build the voronoi cells.
        /// </summary>
        public void Construct()
        {
            //Construct
            Construct(VoronoiWrapper);

            //Build Maps
            CreateVertexMap(VoronoiWrapper);
            CreateEdgeMap(VoronoiWrapper);
            CreateCellMap(VoronoiWrapper);

            //long maxEdgeSize = VoronoiWrapper.GetEdgeMapMaxSize();
            //long maxEdgeIndexSize = VoronoiWrapper.GetEdgeIndexMapMaxSize();

            this.CountVertices = GetCountVertices(VoronoiWrapper);
            this.CountEdges = GetCountEdges(VoronoiWrapper);
            this.CountCells = GetCountCells(VoronoiWrapper);

            for (long i=0;i< CountVertices; i++)
            {
                var vertex = GetVertex(i);
                Vertices.Add(i, vertex);
            }
            for (long i = 0; i < CountEdges; i++)
            {
                var edge = GetEdge(i);
                Edges.Add(i, edge);
            }
            for (long i = 0; i < CountCells; i++)
            {
                var cell = GetCell(i);
                Cells.Add(i, cell);
            }
            foreach (var edge in Edges.Values)
            {
                if (edge.ID % 2 == 1|| !edge.IsFinite || !edge.IsPrimary) continue;
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
        private void ComputeObstaclePoint(Cell cell,Edge edge, out Vector2 start, out Vector2 end)
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
        /// <summary>
        /// Clears the list of the inserted geometries.
        /// </summary>
        public void Clear()
        {
            Clear(VoronoiWrapper);
        }

        private Vertex GetVertex(long index)
        {
            if (index < -0 || index > this.CountVertices - 1)
                throw new IndexOutOfRangeException();
            GetVertex(VoronoiWrapper, index, out double a1, out double a2, out long a3);
            var x = Tuple.Create((float)a1,(float)a2,a3);
            return new Vertex(index,x);
        }


        private Edge GetEdge(long index)
        {
            if (index < -0 || index > this.CountEdges - 1)
                throw new IndexOutOfRangeException();
            GetEdge(VoronoiWrapper, index, out long a1, out long a2, out bool a3, out bool a4, out bool a5, out long a6, out long a7, out long a8, out long a9, out long a10, out long a11);
            var relation = Tuple.Create(a6,a7,a8, a9, a10, a11);
            var x = Tuple.Create( a1, a2, a3, a4,a5, relation);
            return new Edge(index,x);
        }

        private Cell GetCell(long index)
        {
            if (index < -0 || index > this.CountCells - 1)
                throw new IndexOutOfRangeException();
            long[] array1 = new long[BUFFER_SIZE];
            long[] array2 = new long[BUFFER_SIZE];
            GetCell(VoronoiWrapper, index, out long a1, out short a2, out bool a3, out bool a4, out bool a5, out long a6);
            var x = Tuple.Create(a1, a2,a3, a4, a5, a6);
            return new Cell(index,x);
        }





        /// <summary>
        /// Add a point to the list of input points
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        //public void AddPoint(int x, int y)
        //{
        //    InputPoints.Add(new Point(x * ScaleFactor, y * ScaleFactor));
        //}

        /// <summary>
        /// Add a point to the list of input points. The input points will be applied a scale factor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPoint(int x, int y)
        {
            Vector2Int p = new Vector2Int(x,y);
            InputPoints.Add(InputPoints.Count, p);
            AddPoint(VoronoiWrapper,p.x, p.y);
        }

        /// <summary>
        /// Add a segment to the list of input segments
        /// </summary>
        /// <param name="x1">x coordinate of the start point</param>
        /// <param name="y1">y coordinate of the start point</param>
        /// <param name="x2">x coordinate of the end point</param>
        /// <param name="y2">y coordinate of the end point</param>
        //public void AddSegment(int x1, int y1, int x2, int y2)
        //{
        //    InputSegments.Add(new Segment(x1 * ScaleFactor,y1 * ScaleFactor,x2 * ScaleFactor,y2 * ScaleFactor));
        //}


        /// <summary>
        /// Add a segment to the list of input segments
        /// </summary>
        /// <param name="x1">x coordinate of the start point</param>
        /// <param name="y1">y coordinate of the start point</param>
        /// <param name="x2">x coordinate of the end point</param>
        /// <param name="y2">y coordinate of the end point</param>
        public void AddSegment(int x1, int y1, int x2, int y2)
        {
            Segment s = new Segment(x1,y1,x2,y2);

            InputSegments.Add(InputSegments.Count, s);
            AddSegment(
                VoronoiWrapper,
                s.Start.x,
                s.Start.y,
                s.End.x,
                s.End.y
            );
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
                new Vector2(segmentSite.Start.x , segmentSite.Start.y),
                new Vector2(segmentSite.End.x ,segmentSite.End.y),
                discretization[0],
                discretization[1],
                max_distance,
                Tolerance
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
