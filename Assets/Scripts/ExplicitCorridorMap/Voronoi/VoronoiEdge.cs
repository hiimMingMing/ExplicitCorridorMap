using System;
using UnityEngine;
namespace ExplicitCorridorMap.Voronoi
{
    public class VoronoiEdge
    {
        public int ID;
        /// <summary>
        /// The index of the start vertex of this segment.
        /// </summary>
        public int Start { get; set; }

        public Vector2 LeftObstacleStart { get; set; }
        public Vector2 RightObstacleStart { get; set; }
        public Vector2 LeftObstacleEnd { get; set; }
        public Vector2 RightObstacleEnd { get; set; }
        /// <summary>
        /// The index of the end vertex of this segment.
        /// </summary>
        public int End { get; set; }


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
        public int Cell { get; set; }

        /// <summary>
        /// The index of the twin cell associated with this segment
        /// </summary>
        public int Twin { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="t">A tuple returned by the CLR wrapper.</param>


        public VoronoiEdge(int id,int start,int end,bool isPrimary,bool isLinear,bool isFinite,int twin,int cell)
        {
            ID = id;
            Start = start;
            End = end;
            IsPrimary = isPrimary;
            IsLinear = isLinear;
            IsFinite = isFinite;
            Twin = twin;
            Cell = cell;
        }
    }
}
