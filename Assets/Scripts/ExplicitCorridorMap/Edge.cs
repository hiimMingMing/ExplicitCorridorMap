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
        public bool IsTwin { get; set; } //use to remove twin while traverse edge list

        public Vector2 LeftObstacleOfStart { get; set; }
        public Vector2 RightObstacleOfStart { get; set; }
        public Vector2 LeftObstacleOfEnd { get; set; }
        public Vector2 RightObstacleOfEnd { get; set; }
        public float ClearanceOfStart { get; set; }
        public float ClearanceOfEnd { get; set; }
        public float WidthClearanceOfStart { get; set; }
        public float WidthClearanceOfEnd { get; set; }
        public List<EdgeProperty> EdgeProperties { get; set; }//use for agent radius
        public float Length { get; set; }

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
            IsTwin = false;
            EdgeProperties = new List<EdgeProperty>();
            Length = (start.Position -  end.Position).magnitude;
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
            ClearanceOfStart = (Start.Position - RightObstacleOfStart).magnitude;
            ClearanceOfEnd = (End.Position - RightObstacleOfEnd).magnitude;
            WidthClearanceOfStart = (LeftObstacleOfStart - RightObstacleOfStart).magnitude / 2.0f;
            WidthClearanceOfEnd = (LeftObstacleOfEnd - RightObstacleOfEnd).magnitude / 2.0f;
        }
        public void ComputeEnvelope()
        {
            _envelope = Geometry.FindBoundingBox(Cell);
        }
        public void SetEnvelope(Envelope e)
        {
            _envelope = e;
        }
        public bool HasEnoughClearance(float radius)
        {
            return radius >= WidthClearanceOfStart && radius >= WidthClearanceOfEnd;
        }
        public void AddProperty(float radius)
        {
            var p = new EdgeProperty();
            if(radius >= ClearanceOfStart)
            {
                p.LeftObstacleOfStart = p.RightObstacleOfStart = Start.Position;
            }
            else
            {
                p.LeftObstacleOfStart = LeftObstacleOfStart + radius * (Start.Position - LeftObstacleOfStart).normalized;
                p.RightObstacleOfStart = RightObstacleOfStart + radius * (Start.Position - RightObstacleOfStart).normalized;
            }
            p.ClearanceOfStart = ClearanceOfStart - radius;

            if (radius >= ClearanceOfEnd)
            {
                p.LeftObstacleOfEnd = p.RightObstacleOfEnd = End.Position;
            }
            else
            {
                p.LeftObstacleOfEnd = LeftObstacleOfEnd + radius * (End.Position - LeftObstacleOfEnd).normalized;
                p.RightObstacleOfEnd = RightObstacleOfEnd + radius * (End.Position - RightObstacleOfEnd).normalized;
            }
            p.ClearanceOfEnd = ClearanceOfEnd - radius;

            EdgeProperties.Add(p);
        }
    }
    public class EdgeProperty
    {
        public Vector2 LeftObstacleOfStart { get; set; }
        public Vector2 RightObstacleOfStart { get; set; }
        public Vector2 LeftObstacleOfEnd { get; set; }
        public Vector2 RightObstacleOfEnd { get; set; }
        public float ClearanceOfStart { get; set; }
        public float ClearanceOfEnd { get; set; }
        
    }
}
