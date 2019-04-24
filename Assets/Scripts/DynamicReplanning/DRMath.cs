using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class DRMath
    {
        public static RectInt ConvertTransToRectInt(float xScale, float yScale, Vector3 input)
        {
            int w = (int)xScale;
            int h = (int)yScale;
            int x = (int)input.x;
            int y = (int)input.y;
            return new RectInt(x - w / 2, y - h / 2, w, h);
        }

        // Check if 2 segments are intersecting or not
        //https://stackoverflow.com/questions/14176776/find-out-if-2-lines-intersect
        public static bool IsIntersecting(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            return (((q1.x - p1.x) * (p2.y - p1.y) - (q1.y - p1.y) * (p2.x - p1.x))
                    * ((q2.x - p1.x) * (p2.y - p1.y) - (q2.y - p1.y) * (p2.x - p1.x)) < 0)
                    &&
                   (((p1.x - q1.x) * (q2.y - q1.y) - (p1.y - q1.y) * (q2.x - q1.x))
                    * ((p2.x - q1.x) * (q2.y - q1.y) - (p2.y - q1.y) * (q2.x - q1.x)) < 0);
        }

        // Remove duplicate in List<Vector2>
        public static List<Vector2> RemoveDuplicate(List<Vector2> list)
        {
            List<Vector2> newList = new List<Vector2>();
            foreach (var l in list)
            {
                if (!newList.Contains(l))
                    newList.Add(l);
            }
            return newList;
        }

        // Remove duplicate in List<Vertex>
        public static List<Vertex> RemoveDuplicate(List<Vertex> list)
        {
            List<Vertex> newList = new List<Vertex>();
            foreach (var l in list)
            {
                if (!newList.Contains(l))
                    newList.Add(l);
            }
            return newList;
        }

        //Find nearest point in R_area to startPosition
        public static Vector2 FindNearestPoint(Vector2 startPosition, RectInt R_area)
        {
            Dictionary<Vector2, float> distance = new Dictionary<Vector2, float>();
            float minValue = float.MinValue;
            Vector2 minDistancePoint = new Vector2();

            Vector2 bottomLeft = R_area.min;
            Vector2 bottomRight = new Vector2(R_area.xMax, R_area.yMin);
            Vector2 topRight = R_area.max;
            Vector2 topLeft = new Vector2(R_area.xMin, R_area.yMax);

            distance[bottomLeft] = PathFinding.HeuristicCost(bottomLeft, startPosition);
            distance[bottomRight] = PathFinding.HeuristicCost(bottomRight, startPosition);
            distance[topRight] = PathFinding.HeuristicCost(topRight, startPosition);
            distance[topLeft] = PathFinding.HeuristicCost(topLeft, startPosition);

            foreach (var d in distance)
            {
                if (d.Value < minValue)
                {
                    minValue = d.Value;
                    minDistancePoint = d.Key;
                }
            }

            return minDistancePoint;
        }

        // Find a position intersection of 2 lines
        //https://www.geeksforgeeks.org/program-for-point-of-intersection-of-two-lines/
        public static Vector2 LineLineIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            // Line AB represented as a1x + b1y = c1  
            float a1 = B.y - A.y;
            float b1 = A.x - B.x;
            float c1 = a1 * (A.x) + b1 * (A.y);

            // Line CD represented as a2x + b2y = c2  
            float a2 = D.y - C.y;
            float b2 = C.x - D.x;
            float c2 = a2 * (C.x) + b2 * (C.y);

            float determinant = a1 * b2 - a2 * b1;

            if (determinant == 0) // 2 lines are parallel -> return a pair of MaxValue
            {
                return new Vector2(float.MaxValue, float.MaxValue);
            }
            else
            {
                float x = (b2 * c1 - b1 * c2) / determinant;
                float y = (a1 * c2 - a2 * c1) / determinant;
                return new Vector2(x, y);
            }
        }

        public static void SwapValue(ref Vector2 num1, ref Vector2 num2)
        {
            Vector2 temp;
            temp = num1;
            num1 = num2;
            num2 = temp;
        }
    }
}
