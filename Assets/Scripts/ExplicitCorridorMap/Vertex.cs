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
        public float X { get; set; }
        public float Y { get; set; }
        public long IncidentEdge { get; set; }
        //public List<Point> NearestObstaclePoints { get; } = new List<Point>();
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="t">A tuple where the first value represents the X-axis and the second value the Y-axis</param>
        public Vertex(long id,Tuple<float, float, long> t)
        {
            ID = id;
            X = t.Item1;
            Y = t.Item2;
            IncidentEdge = t.Item3;
        }
        

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var p = obj as Vertex;
            if (p == null) return false;
            return X == p.X && Y == p.Y;
        }
        public override int GetHashCode()
        {
            return (int)X^(int)Y;
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
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }
}
