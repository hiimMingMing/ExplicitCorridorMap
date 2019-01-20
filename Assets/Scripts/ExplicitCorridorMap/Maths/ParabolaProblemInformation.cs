using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap.Maths
{
    public class ParabolaProblemInformation
    {
        public Vector2 FocusPoint { get; set; }
        public Vector2 DirectixSegmentStart { get; set; }
        public Vector2 DirectixSegmentEnd { get; set; }
        public Vector2 ParabolaStart { get; set; }
        public Vector2 ParabolaEnd { get; set; }

        public ParabolaProblemInformation(Vector2 focusPoint, Vector2 directixSegmentStart, Vector2 directixSegmentEnd, Vector2 parabolaStart, Vector2 parabolaEnd)
        {
            FocusPoint = focusPoint;
            DirectixSegmentStart = directixSegmentStart;
            DirectixSegmentEnd = directixSegmentEnd;
            ParabolaStart = parabolaStart;
            ParabolaEnd = parabolaEnd;
        }

        public List<Tuple<Vector2, string>> GetAsVertexList()
        {
            return new List<Tuple<Vector2, string>>(){
                Tuple.Create<Vector2, string>(FocusPoint, "Focus"),
                Tuple.Create<Vector2, string>(DirectixSegmentStart, "DirectixStart"),
                Tuple.Create<Vector2, string>(DirectixSegmentEnd, "DirectixEnd"),
                Tuple.Create<Vector2, string>(ParabolaStart, "ParabolaStart"),
                Tuple.Create<Vector2, string>(ParabolaEnd, "ParabolaEnd")
            };
        }
    }
}
