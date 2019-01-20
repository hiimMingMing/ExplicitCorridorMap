using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap
{
    public class Vertex
    {
        public long ID;
        public Vector2 Position;
        public long IncidentEdge { get; set; }
        public float X { get => Position.x; set => Position.x = value; }
        public float Y { get => Position.y; set => Position.y = value; }

        //public List<Point> NearestObstaclePoints { get; } = new List<Point>();
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="t">A tuple where the first value represents the X-axis and the second value the Y-axis</param>
        public Vertex(long id,Tuple<float, float, long> t)
        {
            ID = id;
            Position = new Vector2(t.Item1,t.Item2);
            IncidentEdge = t.Item3;
        }
        

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var p = obj as Vertex;
            if (p == null) return false;
            return Position.Equals(p.Position);
        }
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
        /// <summary>
        /// Returns a concatenation of the coordinates, separated by a comma
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}, {1}", X, Y);
        }

        public double[] GetKDKey()
        {
            return new double[] { X, Y };
        }
    }
}
