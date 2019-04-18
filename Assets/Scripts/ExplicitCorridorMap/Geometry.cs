using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Advanced.Algorithms.DataStructures;

namespace ExplicitCorridorMap
{
    public static class Geometry
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
            int w, h, x, y;
            w = (int)cube.localScale.x;
            h = (int)cube.localScale.z;
            x = (int)cube.position.x;
            y = (int)cube.position.z;
            return new RectInt(x - w / 2, y - h / 2, w, h);
        }
        public static RectInt ConvertToRect2D(Transform cube)
        {
            int w, h, x, y;
            w = (int)cube.localScale.x;
            h = (int)cube.localScale.y;
            x = (int)cube.position.x;
            y = (int)cube.position.y;
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
        public static Obstacle ConvertToObstacle(Transform go)
        {
            return new Obstacle(ConvertToRectInt(go));
        }
        public static RectInt ConvertToRectInt(Transform go)
        {
            var bound = go.GetComponent<Renderer>().bounds;
            var min = Vector2Int.FloorToInt(bound.min.To2D());
            var size = Vector2Int.FloorToInt(bound.size.To2D());
            return new RectInt(min, size);
        }

        public static float ComputeAreaOfPolygon(List<Vector2> p)
        {
            if (p.Count <= 2) return 0.0f;
            float a = 0;
            for (int i = 0; i < p.Count - 2; i++)
            {
                a += p[i].x * p[i + 1].y;
                a -= p[i + 1].x * p[i].y;
            }
            a += p[p.Count - 1].x * p[0].y;
            a -= p[0].x * p[p.Count - 1].y;
            a = a / 2.0f;
            return a;
        }
    }
}
