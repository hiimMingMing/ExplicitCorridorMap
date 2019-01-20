using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace ExplicitCorridorMap
{
    public class Segment
    {
        public Vector2Int Start{ get; set; }
        public Vector2Int End { get; set; }

        public Segment(Vector2Int start, Vector2Int end)
        {
            Start = start;
            End = end;
        }

        public Segment(int x1, int y1, int x2, int y2)
        {
            Start = new Vector2Int(x1, y1);
            End = new Vector2Int(x2, y2);
        }

        public override string ToString()
        {
            return String.Format("Start: {0}, End: {1}", Start.ToString(), End.ToString());
        }
    }
}
