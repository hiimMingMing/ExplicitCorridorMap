using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class DRMath
    {
        // Convert obstacles to list of 4 points
        //public static List<Vector2> ConvertToPoint(float xScale, float yScale, Vector3 input)
        //{
        //    List<Vector2> pointList = new List<Vector2>();
        //    float x = input.x;
        //    float y = input.y;
        //    float w = xScale;
        //    float h = yScale;
        //    pointList.Add(new Vector2(x - w / 2, y - h / 2));
        //    pointList.Add(new Vector2(x + w / 2, y - h / 2));
        //    pointList.Add(new Vector2(x + w / 2, y + h / 2));
        //    pointList.Add(new Vector2(x - w / 2, y + h / 2));
        //    return pointList;
        //}

        public static RectInt ConvertToRect(float xScale, float yScale, Vector3 input)
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
        public static void RemoveDuplicate(List<Vector2> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].Equals(list[i + 1]))
                {
                    list.RemoveAt(i + 1);
                    i--;
                }
            }
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
