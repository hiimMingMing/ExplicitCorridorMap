using SharpBoostVoronoi.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace SharpBoostVoronoi.Maths
{
    public class Distance
    {
        /// <summary>
        /// Fetch the distance between two points
        /// </summary>
        /// <param name="p1">A point</param>
        /// <param name="p2">A point</param>
        /// <returns></returns>
        public static float ComputeDistanceBetweenPoints(Vector2 p1, Vector2 p2)
        {
            if (p1.x == p2.x && p1.y == p2.y)
            {
                return 0;
            }
            return (p1 - p2).magnitude;
        }

        /// <summary>
        /// Fetch the square distance between 2 points
        /// </summary>
        /// <param name="p1">A point</param>
        /// <param name="p2">A point</param>
        /// <returns></returns>
        public static float ComputeSquareDistanceBetweenPoints(Vector2 p1, Vector2 p2)
        {
            return (p1-p2).sqrMagnitude;
        }

        /*****************************************
        Inspiration for finding the closest point : 
        http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment/1501725#1501725
        ******************************************/
        /// <summary>
        /// Find the closest point on a segment from another point. The segment is expected to be a straight line between two points
        /// </summary>
        /// <param name="lineStart">The first point of the segment</param>
        /// <param name="lineEnd">The last point of the segment</param>
        /// <param name="point">The point projected on the line</param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector2 GetClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point, out float distance)
        {
            //Test if the line has a length <> 0
            float dist2 = ComputeSquareDistanceBetweenPoints(lineStart, lineEnd);
            if (dist2 == 0)
            {
                distance = ComputeDistanceBetweenPoints(point, lineStart);
                return lineStart;
            }

            //Compute the projection of on the line
            float t = ((point.x - lineStart.x) * (lineEnd.x - lineStart.x) + (point.y - lineStart.y) * (lineEnd.y - lineStart.y)) / dist2;


            if (t < 0)//point projection falls beyond the first node of the segment
            {
                distance = ComputeDistanceBetweenPoints(point, lineStart);
                return lineStart;
            }

            else if (t > 1)//point projection falls beyond the first node of the segment
            {
                distance = ComputeDistanceBetweenPoints(point, lineEnd);
                return lineEnd;
            }

            Vector2 projectionPoint = new Vector2(lineStart.x + t * (lineEnd.x - lineStart.x), lineStart.y + t * (lineEnd.y - lineStart.y));
            distance = ComputeDistanceBetweenPoints(point, projectionPoint);
            return projectionPoint;
        }


        /// <summary>
        /// Find the closest point on a segment from another point. The segment is expected to be a straight line between two points
        /// </summary>
        /// <param name="lineStart">The first point of the segment</param>
        /// <param name="lineEnd">The last point of the segment</param>
        /// <param name="point">The point projected on the line</param>
        /// <returns></returns>
        public static Vector2 GetClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            float distance = 0;
            return GetClosestPointOnLine(lineStart, lineEnd, point, out distance);
        }



        /// <summary>
        /// Compute the vector between two points
        /// </summary>
        /// <param name="lineStart">The first point of the segment</param>
        /// <param name="lineEnd">The last point of the segment</param>
        /// <returns></returns>
        public static Vector2 ComputeVector(Vector2 lineStart, Vector2 lineEnd)
        {
            return new Vector2(lineEnd.x - lineStart.x, lineEnd.y - lineStart.y);
        }

        /// <summary>
        /// Compute a point on the line at a specific distance.
        /// </summary>
        /// <param name="p1">The first point of the line.</param>
        /// <param name="p2">The last point of the line.</param>
        /// <param name="distanceOnLine">The distance on the line where the point will be fetched.</param>
        /// <returns></returns>
        public static Vector2 GetPointAtDistance(Vector2 p1, Vector2 p2, float distanceOnLine)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            float l = Mathf.Sqrt(Mathf.Pow(dx,2) + Mathf.Pow(dy,2));
            if(distanceOnLine > l)
                throw new Exception ("Length is greater than the length of the segment");

            return new Vector2(p1.x + dx/l * distanceOnLine, p1.y + dy/l * distanceOnLine);
        }

    }
}
