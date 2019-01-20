using ExplicitCorridorMap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap.Maths
{
    public class ParabolaComputation
    {
        /// <summary>
        /// Return the angle between 2 points as radians
        /// </summary>
        /// <param name="start">The first point</param>
        /// <param name="end">The second point</param>
        /// <returns>The angle in radians</returns>
        static float GetLineAngleAsRadiant(Vector2 start, Vector2 end)
        {
            return Mathf.Atan2(end.y - start.y, end.x - start.x);
        }


        /// <summary>
        /// Find the position of y on the parabolar given focus and directix y. The equation to find a point on the parabola
        /// given those two value is: (x−a)2+b2−c2=2(b−c)y so y=((x−a)2+b2−c2)/2(b−c)
        /// </summary>
        /// <param name="x">The x value for which a y value is wanted</param>
        /// <param name="focus">The focus of the parabola</param>
        /// <param name="directrix_y">The y value of the directx line</param>
        /// <returns>The y value associated with x</returns>
        static float ParabolaY(float x, Vector2 focus, float directrix_y)
        {
            return (Mathf.Pow(x - focus.x, 2) + Mathf.Pow(focus.y, 2) - Mathf.Pow(directrix_y, 2)) / (2 * (focus.y - directrix_y));
        }


        /// <summary>
        /// Interpolate parabola points between two points. The equation is computed by turning the input segment
        /// into the directix of the parabola, and the input point into the focus of the parabola. The directix used
        /// to solve the parabola is horizontal (parallel to x-axis).
        /// </summary>
        /// <param name="focus">The input point that will be used as a focus point.</param>
        /// <param name="dir">The input segment that will be used a the directix</param>
        /// <param name="par_start">The point on the parabola used as a starting point.</param>
        /// <param name="par_end">The point on the parabola used as a ending point.</param>
        /// <param name="max_distance">The maximum distance between 2 points on the parabola</param>
        /// <param name="tolerance">The maximum distance between 2 points on the parabola</param>
        /// <returns></returns>
        public static List<Vector2> Densify(Vector2 focus, Vector2 dir_start, Vector2 dir_end, Vector2 par_start, Vector2 par_end, float max_distance, float tolerance)
        {

            if (max_distance <= 0)
                throw new ArgumentOutOfRangeException(String.Format("The maximum distance must be greater than 0. Value passed: {0}", max_distance));

            if (tolerance < 0)
                throw new ArgumentOutOfRangeException(String.Format("The tolerance must be greater than or equal to 0. Value passed: {0}", tolerance));

            #region Rotate Input Points

            //Compute the information required to perform rotation
            float shift_X = Mathf.Min(dir_start.x, dir_end.x);
            float shift_Y = Mathf.Min(dir_start.y, dir_end.y);
            float angle = GetLineAngleAsRadiant(dir_start, dir_end);

            Vector2 focus_rotated = Rotation.Rotate(
                focus, 
                angle, 
                shift_X, 
                shift_Y
            );

            Vector2 dir_startPoint_rotated = Rotation.Rotate(
                dir_start,
                angle,
                shift_X,
                shift_Y
            );

            Vector2 dir_endPoint_rotated = Rotation.Rotate(
                dir_end,
                angle,
                shift_X,
                shift_Y);

            Vector2 par_startPoint_rotated = Rotation.Rotate(
                par_start, 
                angle, 
                shift_X, 
                shift_Y
            );
            
            Vector2 par_endPoint_rotated = Rotation.Rotate(
                par_end, 
                angle, 
                shift_X, 
                shift_Y
            );

            #endregion

            #region Validate the equation on first and last points given by Boost
            //Set parabola parameters
            float directrix = dir_endPoint_rotated.y;
            float snapTolerance = 5;


            List<Vector2> densified_rotated = new List<Vector2>();
            Stack<Vector2> next = new Stack<Vector2>();

            ParabolaProblemInformation nonRotatedInformation = new ParabolaProblemInformation(
                    focus,
                    dir_start,
                    dir_end,
                    par_start,
                    par_end
            );


            float distanceFocusToDirectix = 0;
            Distance.GetClosestPointOnLine(focus, dir_start, dir_end, out distanceFocusToDirectix);
            if (distanceFocusToDirectix == 0)
                throw new FocusOnDirectixException(nonRotatedInformation);

            ParabolaProblemInformation rotatedInformation = new ParabolaProblemInformation(
                focus_rotated,
                dir_startPoint_rotated,
                dir_endPoint_rotated,
                par_startPoint_rotated,
                par_endPoint_rotated
            );

            List<Tuple<Vector2, Vector2>> points = new List <Tuple<Vector2, Vector2>>();
            points.Add(
                Tuple.Create<Vector2, Vector2>(
                    par_startPoint_rotated, 
                    new Vector2(par_startPoint_rotated.x, ParabolaY(par_startPoint_rotated.x, focus_rotated, directrix))
                 )
            );

            points.Add(
                Tuple.Create<Vector2, Vector2>(
                    par_endPoint_rotated,
                    new Vector2(par_endPoint_rotated.x, ParabolaY(par_endPoint_rotated.x, focus_rotated, directrix))
                 )
            );

            foreach (var point in points)
            {
                float delta = point.Item1.y > point.Item2.y ?
                        point.Item1.y - point.Item2.y : point.Item2.y - point.Item1.y;

                if (delta > snapTolerance)
                {
                    GenerateParabolaIssueInformation(rotatedInformation, nonRotatedInformation, point.Item1, point.Item2, 0.001f);
                    throw new Exception(
                        String.Format(
                            "The computed y on the parabola for the starting / ending point is different from the rotated point returned by Boost. Difference: {0}",
                            delta)
                         );
                }
            }
            #endregion

            #region Compute Intermediate Points (Rotated)
            Vector2 previous = points[0].Item2;
            densified_rotated.Add(previous);
            next.Push(points[1].Item2);

            while (next.Count > 0)
            {
                Vector2 current = next.Peek();
                float mid_cord_x = (previous.x + current.x) / 2;
                Vector2 mid_curve = new Vector2(mid_cord_x, ParabolaY(mid_cord_x, focus_rotated, directrix));
                float distance = Distance.ComputeDistanceBetweenPoints(current, previous);
                if (distance > max_distance)
                {
                    next.Push(mid_curve);
                }
                else
                {
                    next.Pop();
                    densified_rotated.Add(current);
                    previous = current;
                }
            }
            #endregion

            #region Unrotate and validate
            List<Vector2> densified = densified_rotated.Select(w => Rotation.Unrotate(w, angle, shift_X, shift_Y)).ToList();
            
            //reset the first and last points so they match exactly.
            if (Mathf.Abs(densified[0].x - par_start.x) > snapTolerance ||
                Mathf.Abs(densified[0].y - par_start.y) > snapTolerance)
                throw new Exception(String.Format("Segmented curve start point is not correct. Tolerance exeeded in x ({0}) or y ({1})",
                    Mathf.Abs(densified[0].x - par_start.x), Mathf.Abs(densified[0].y - par_start.y)));
            densified[0] = par_start;

            if (Mathf.Abs(densified[densified.Count - 1].x - par_end.x) > snapTolerance ||
                Mathf.Abs(densified[densified.Count - 1].y - par_end.y) > snapTolerance)
                throw new Exception(String.Format("Segmented curve end point is not correct. Tolerance exeeded in x ({0}) or y ({1})",
                    Mathf.Abs(densified[densified.Count - 1].x - par_end.x), Mathf.Abs(densified[densified.Count - 1].y - par_end.y)));
            densified[densified.Count - 1] = par_end;
            #endregion
            
            return densified;
        }


        /// <summary>
        /// Generate an exception when the point computed by the parabola equation is different from the point computed by boost.
        /// </summary>
        /// <param name="rotatedInformation">The information used to solve parabola. This is the is the information before the rotation.</param>
        /// <param name="nonRotatedInformation">The information used to solve parabola. This is the is the information after the rotation.</param>
        /// <param name="boostPoint">The point on the parabola returned by Boost.</param>
        /// <param name="parabolaPoint">The point on the parabola computed.</param>
        /// <param name="tolerance">The tolerance used to decide if an exception need to be raise.</param>
        private static void GenerateParabolaIssueInformation(ParabolaProblemInformation rotatedInformation, ParabolaProblemInformation nonRotatedInformation, Vector2 boostPoint, Vector2 parabolaPoint, float tolerance)
        {
            if (tolerance < 0)
                throw new ArgumentOutOfRangeException(String.Format("Tolenrance must be greater than 0"));

            float minX = Mathf.Min(Mathf.Min(Mathf.Min(rotatedInformation.DirectixSegmentStart.x, rotatedInformation.DirectixSegmentStart.x), boostPoint.x), parabolaPoint.x);
            float maxX = Mathf.Max(Mathf.Max(Mathf.Max(rotatedInformation.DirectixSegmentStart.x, rotatedInformation.DirectixSegmentStart.x), boostPoint.x), parabolaPoint.x);

            //Compute the distance between the input parabola point
            float distanceBoostPointToFocus = Distance.ComputeDistanceBetweenPoints(boostPoint, rotatedInformation.FocusPoint);
            float distanceBoostPointToDirectix = 0;
            Distance.GetClosestPointOnLine(
                    new Vector2(minX, rotatedInformation.DirectixSegmentEnd.y),
                    new Vector2(maxX, rotatedInformation.DirectixSegmentEnd.y),
                    boostPoint, 
                    out distanceBoostPointToDirectix
            );


            float distanceComputedPointToFocus = Distance.ComputeDistanceBetweenPoints(parabolaPoint, rotatedInformation.FocusPoint);
            float distanceComputedPointToDirectix = 0;
            Distance.GetClosestPointOnLine(
                    new Vector2(minX, rotatedInformation.DirectixSegmentEnd.y),
                    new Vector2(maxX, rotatedInformation.DirectixSegmentEnd.y),
                    parabolaPoint,
                    out distanceComputedPointToDirectix
            );

            float distanceDiff = distanceComputedPointToFocus > distanceComputedPointToDirectix ?
                distanceComputedPointToFocus - distanceComputedPointToDirectix : distanceComputedPointToDirectix - distanceComputedPointToFocus;

            if (distanceDiff < tolerance || float.IsNaN(distanceDiff) || float.IsInfinity(distanceDiff))
                throw new UnsolvableVertexException(nonRotatedInformation, rotatedInformation, boostPoint, parabolaPoint,
                    distanceBoostPointToFocus, distanceComputedPointToFocus, distanceBoostPointToDirectix, distanceComputedPointToDirectix);

        }
    }
}
