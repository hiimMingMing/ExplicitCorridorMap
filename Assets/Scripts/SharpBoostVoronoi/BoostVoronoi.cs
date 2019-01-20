using SharpBoostVoronoi.Parabolas;
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
    internal class BoostVoronoi : IDisposable
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
        /// A property used to define tolerance to parabola interpolation.
        /// </summary>
        public float Tolerance { get; set; }


        public long CountVertices { get; private set; }
        public long CountEdges { get; private set; }
        public long CountCells { get; private set; }
        

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoostVoronoi()
        {
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

            
        }
        
        /// <summary>
        /// Clears the list of the inserted geometries.
        /// </summary>
        public void Clear()
        {
            Clear(VoronoiWrapper);
        }

        public Vertex GetVertex(long index)
        {
            if (index < -0 || index > this.CountVertices - 1)
                throw new IndexOutOfRangeException();
            GetVertex(VoronoiWrapper, index, out double a1, out double a2, out long a3);
            var x = Tuple.Create((float)a1,(float)a2,a3);
            return new Vertex(index,x);
        }


        public Edge GetEdge(long index)
        {
            if (index < -0 || index > this.CountEdges - 1)
                throw new IndexOutOfRangeException();
            GetEdge(VoronoiWrapper, index, out long a1, out long a2, out bool a3, out bool a4, out bool a5, out long a6, out long a7, out long a8, out long a9, out long a10, out long a11);
            var relation = Tuple.Create(a6,a7,a8, a9, a10, a11);
            var x = Tuple.Create( a1, a2, a3, a4,a5, relation);
            return new Edge(index,x);
        }

        public Cell GetCell(long index)
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
            AddSegment(
                VoronoiWrapper,
                s.Start.x,
                s.Start.y,
                s.End.x,
                s.End.y
            );
        }

        
    }
}
