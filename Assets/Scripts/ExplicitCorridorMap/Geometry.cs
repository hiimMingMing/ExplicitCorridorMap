using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Advanced.Algorithms.DataStructures;

namespace ExplicitCorridorMap
{
    public class Geometry
    {
        public static bool PolygonContainsPoint(List<Vector2> polyPoints, Vector2 p)
        {
            var j = polyPoints.Count - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                if (((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) &&
                   (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x))
                    inside = !inside;
            }
            return inside;
        }
        public static bool PolygonContainsPoint(Vector2 polyPoint1, Vector2 polyPoint2, Vector2 polyPoint3, Vector2 p)
        {
            return PolygonContainsPoint(new List<Vector2> { polyPoint1, polyPoint2, polyPoint3 }, p);
        }
        public static RectInt ConvertToRect(Transform cube)
        {
            int w = (int)cube.localScale.x;
            int h = (int)cube.localScale.y;
            int x = (int)cube.position.x;
            int y = (int)cube.position.y;
            return new RectInt(x - w / 2, y - h / 2, w, h);
        }
        
        public static MBRectangle ComputeMBRectangle(List<Vector2> points)
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            foreach (var v in points)
            {
                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }
            return new MBRectangle(new Point( minX, maxY), new Point( maxX, minY));
        }
        public static Rectangle ExtendRectangle(Rectangle e, float d)
        {
            return new Rectangle(new Point(e.LeftTop.X -d, e.LeftTop.Y + d), new Point(e.RightBottom.X + d, e.RightBottom.Y - d));
        }
    }
}
