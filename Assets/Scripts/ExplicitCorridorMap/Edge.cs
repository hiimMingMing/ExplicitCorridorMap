using ExplicitCorridorMap.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class Edge
    {
        public int ID;
        public List<Vector2> Cell { get; set; }
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public Edge Twin { get; set; }
        public bool IsLinear { get; set; }

        public Vector2 LeftObstacleStart { get; set; }
        public Vector2 RightObstacleStart { get; set; }
        public Vector2 LeftObstacleEnd { get; set; }
        public Vector2 RightObstacleEnd { get; set; }

        public int SiteID { get; set; }
        public bool ContainsPoint { get; set; }
        public bool ContainsSegment { get; set; }
        public SourceCategory SourceCategory { get; set; }
        public Edge(Vertex start, Vertex end, VoronoiEdge e, VoronoiCell c)
        {
            Start = start;
            End = end;
            IsLinear = e.IsLinear;
            ID = e.ID;

            SiteID = c.Site;
            ContainsPoint = c.ContainsPoint;
            ContainsSegment = c.ContainsSegment;
            SourceCategory = c.SourceCategory;
        }
        public override string ToString()
        {
            return string.Format("{0}-{1}", Start, End);
        }
        public void ComputeCell()
        {
            Cell = new List<Vector2>
                {
                    LeftObstacleStart,
                    Start.Position,
                    RightObstacleStart,
                    RightObstacleEnd,
                    End.Position,
                    LeftObstacleEnd
                };
        }
    }
}
