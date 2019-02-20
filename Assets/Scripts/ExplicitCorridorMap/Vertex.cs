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
        public override string ToString()
        {
            return ID.ToString();
        }

        public float[] GetKDKey()
        {
            return new float[] { X, Y };
        }
    }
}
