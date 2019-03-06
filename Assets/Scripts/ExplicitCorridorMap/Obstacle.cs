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
        public List<Vector2> Points = new List<Vector2>();
        private Envelope _envelope;
        public ref readonly Envelope Envelope => ref _envelope;
        public Obstacle(List<Vector2> points)
        {
            Points = points;
            _envelope = Geometry.FindBoundingBox(Points);
        }
        public Obstacle(RectInt rect)
        {
            Points.Add(new Vector2(rect.xMin, rect.yMin));
            Points.Add(new Vector2(rect.xMin, rect.yMax));
            Points.Add(new Vector2(rect.xMax, rect.yMax));
            Points.Add(new Vector2(rect.xMax, rect.yMin));
            _envelope = Geometry.FindBoundingBox(Points);
        }
        public bool ContainsPoint(Vector2 p)
        {
            return Geometry.PolygonContainsPoint(Points, p);
        }

    }
}
