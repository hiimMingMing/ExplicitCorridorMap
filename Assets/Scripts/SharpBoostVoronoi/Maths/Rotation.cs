using SharpBoostVoronoi.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SharpBoostVoronoi.Maths
{
    public class Rotation
    {
        /// <summary>
        /// Rotate a point by an angle around another point (origin). 
        /// The rotation happen clockwise. 
        /// https://www.siggraph.org/education/materials/HyperGraph/modeling/mod_tran/2drota.htm
        /// The orgin point is computed as the point moved by a shift on x and y axis.
        /// </summary>
        /// <param name="p">The point to be rotated</param>
        /// <param name="theta">The angle for the rotation (in radians).</param>
        /// <param name="shift_x">The translation along the x-axis used during the rotation.</param>
        /// <param name="shift_y">The translation along the y-axis used during the rotation.</param>
        /// <returns></returns>
        public static Vector2 Rotate(Vector2 p, float theta, float shift_x, float shift_y)
        {
            return Rotate(new Vector2(p.x - shift_x, p.y - shift_y), theta);
        }

        /// <summary>
        /// Rotate a point around another point (anchor)
        /// </summary>
        /// <param name="point">The anchor point used for the rotation</param>
        /// <param name="theta">The angle for the rotation</param>
        /// <returns>The rotated point</returns>
        public static Vector2 Rotate(Vector2 point, float theta)
        {
            float t = -1 * theta;
            float cos = Mathf.Cos(t);
            float sin = Mathf.Sin(t);
            return new Vector2(
                (point.x * cos) - (point.y * sin),
                (point.x * sin) + (point.y * cos)
                );
        }

        /// <summary>
        /// Undo the rotation done for a point.
        /// </summary>
        /// <param name="p">The point to rotate.</param>
        /// <param name="theta">The angle in radians</param>
        /// <param name="shift_x">The translation along the x-axis used during the rotation.</param>
        /// <param name="shift_y">The translation along the y-axis used during the rotation.</param>
        /// <returns></returns>
        public static Vector2 Unrotate(Vector2 p, float theta, float shift_x, float shift_y)
        {
            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);

            return new Vector2(
                (p.x * cos) - (p.y * sin) + shift_x,
                (p.x * sin) + (p.y * cos) + shift_y
                );
        }
    }
}
