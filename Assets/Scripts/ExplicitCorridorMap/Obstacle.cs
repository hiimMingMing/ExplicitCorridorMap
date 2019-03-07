using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RBush;
namespace ExplicitCorridorMap
{
    public class Obstacle : ISpatialData
    {
        public int ID { get; set; }
        public List<Vector2> Points = new List<Vector2>();
        public List<Segment> Segments = new List<Segment>();
        private Envelope _envelope;
        public ref readonly Envelope Envelope => ref _envelope;
        public bool IsBorder = false;
        public Obstacle(List<Vector2> points)
        {
            Points = points;
            Init();
        }
        public Obstacle(RectInt rect)
        {
            Points.Add(new Vector2(rect.xMin, rect.yMin));
            Points.Add(new Vector2(rect.xMin, rect.yMax));
            Points.Add(new Vector2(rect.xMax, rect.yMax));
            Points.Add(new Vector2(rect.xMax, rect.yMin));
            Init();
        }
        public bool ContainsPoint(Vector2 p)
        {
            return Geometry.PolygonContainsPoint(Points, p);
        }
        public void Init()
        {
            _envelope = Geometry.FindBoundingBox(Points);
            for (int i = 1; i < Points.Count; i++)
            {
                var s = new Segment(Vector2Int.CeilToInt(Points[i - 1]),Vector2Int.CeilToInt(Points[i]));
                s.Parent = this;
                Segments.Add(s);
            }
            var sLast = new Segment(Vector2Int.CeilToInt(Points[Points.Count - 1]), Vector2Int.CeilToInt(Points[0]));
            sLast.Parent = this;
            Segments.Add(sLast);
        }
    }
}
