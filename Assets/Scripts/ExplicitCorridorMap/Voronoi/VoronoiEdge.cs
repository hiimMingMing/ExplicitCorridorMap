using System;
using UnityEngine;
namespace ExplicitCorridorMap.Voronoi
{
    public class VoronoiEdge
    {
        public long ID;
        /// <summary>
        /// The index of the start vertex of this segment.
        /// </summary>
        public long Start { get; set; }

        public Vector2 LeftObstacleStart { get; set; }
        public Vector2 RightObstacleStart { get; set; }
        public Vector2 LeftObstacleEnd { get; set; }
        public Vector2 RightObstacleEnd { get; set; }
        /// <summary>
        /// The index of the end vertex of this segment.
        /// </summary>
        public long End { get; set; }


        /// <summary>
        /// True is the edge is a primary edge, False otherwise.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// True is a segment is a line, false if the segment is an arc.
        /// </summary>
        public bool IsLinear { get; set; }

        /// <summary>
        /// True if the edge is delimited by two known vertices, False otherwise.
        /// </summary>
        public bool IsFinite{ get; set; }

        /// <summary>
        /// The index of the cell associated with this segment
        /// </summary>
        public long Cell { get; set; }

        /// <summary>
        /// The index of the twin cell associated with this segment
        /// </summary>
        public long Twin { get; set; }

        public long Next { get; set; }
        public long Prev { get; set; }
        public long RotNext { get; set; }
        public long RotPrev { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="t">A tuple returned by the CLR wrapper.</param>


        public VoronoiEdge(long id, Tuple<long, long, bool, bool, bool, Tuple<long, long, long, long, long, long>> t)
        {
            ID = id;
            Start = t.Item1;
            End = t.Item2;
            IsPrimary = t.Item3;
            IsLinear = t.Item4;
            IsFinite = t.Item5;
            Twin = t.Item6.Item1;
            Cell = t.Item6.Item2;
            Next = t.Item6.Item3;
            Prev = t.Item6.Item4;
            RotNext = t.Item6.Item5;
            RotPrev = t.Item6.Item6;
        }
    }
}
