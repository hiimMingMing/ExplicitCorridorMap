using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class Geometry
    {
        public static bool ContainsPoint(List<Vector2> polyPoints  , Vector2 p )  { 
           var j = polyPoints.Count - 1;
           var inside = false; 
           for (int i = 0; i<polyPoints.Count; j = i++) { 
              if (((polyPoints[i].y <= p.y && p.y<polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y<polyPoints[i].y)) && 
                 (p.x<(polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x)) 
                 inside = !inside; 
           } 
           return inside; 
        }

    }
}
