using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Advanced.Algorithms.DataStructures;
namespace ExplicitCorridorMap
{
    public class Obstacle : SpatialData
    {
        public int ID { get; set; }
        public int RvoID { get; set; }
        public List<Vector2> Points = new List<Vector2>();
        public List<Segment> Segments = new List<Segment>();
        private Rectangle Rectangle;
        public bool IsBorder = false;
        public GameObject GameObject { get; set; }
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
            ComputeMBRectangle();
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
        public override void ComputeMBRectangle()
        {
            mBRectangle = Geometry.ComputeMBRectangle(Points);
            mBRectangle.Polygon = this;
        }
        public override MBRectangle GetContainingRectangle()
        {
            return mBRectangle;
        }
        public IList<RVO.Vector2> ToRVO()
        {
            IList<RVO.Vector2> obs = new List<RVO.Vector2>();
            for(int i = Points.Count-1; i >= 0; i--)
            {
                obs.Add(new RVO.Vector2(Points[i].x, Points[i].y));
            }
            return obs;
        }
    }
}
