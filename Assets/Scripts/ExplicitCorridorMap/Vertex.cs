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
        public int ID;
        public Vector2 Position;
        public float X { get => Position.x; set => Position.x = value; }
        public float Y { get => Position.y; set => Position.y = value; }
        public List<Edge> Edges { get; set; }

        public bool IsInside { get; set; }
        //public List<Point> NearestObstaclePoints { get; } = new List<Point>();
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="t">A tuple where the first value represents the X-axis and the second value the Y-axis</param>
        public Vertex(int id,float x, float y)
        {
            ID = id;
            Position = new Vector2(x,y);
            Edges = new List<Edge>();
            IsInside = false;
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

        public float[] GetKDKey()
        {
            return new float[] { X, Y };
        }
    }
}
