using ExplicitCorridorMap.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RBush;

namespace ExplicitCorridorMap
{
    public class Edge : ISpatialData
    {
        public int ID;
        public List<Vector2> Cell { get; set; }
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public Edge Twin { get; set; }
        public bool IsLinear { get; set; }
     
        public Vector2 LeftObstacleOfStart { get; set; }
        public Vector2 RightObstacleOfStart { get; set; }
        public Vector2 LeftObstacleOfEnd { get; set; }
        public Vector2 RightObstacleOfEnd { get; set; }

        public int SiteID { get; set; }
        public bool ContainsPoint { get; set; }
        public bool ContainsSegment { get; set; }
        public SourceCategory SourceCategory { get; set; }

        private Envelope _envelope;
        public ref readonly Envelope Envelope => ref _envelope;
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
                    Start.Position,
                    RightObstacleOfStart,
                    RightObstacleOfEnd,
                    End.Position,
                    LeftObstacleOfEnd,
                    LeftObstacleOfStart
                };
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            foreach(var v in Cell)
            {
                if (v.x < minX) minX = v.x;
                else if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                else if (v.y > maxY) maxY = v.y;
            }
            _envelope = new Envelope(minX, minY, maxX, maxY);
        }
    }
}
