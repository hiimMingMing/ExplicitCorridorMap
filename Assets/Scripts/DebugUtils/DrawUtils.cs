using ExplicitCorridorMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DebugUtils
{
    class DrawUtils
    {
        public static void DrawPolyLine(List<Vector2> l)
        {
            for (int i = 0; i < l.Count - 1; i++)
            {
                Gizmos.DrawLine(l[i].To3D(), l[i + 1].To3D());
            }
        }

        public static void DrawPortal(Vector2 begin, Vector2 end)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(begin.To3D(), end.To3D());
        }
        public static void DrawObstaclePoint(Edge edge)
        {
            var startVertex = edge.Start.Position;
            var endVertex = edge.End.Position;
            var begin = startVertex;
            var obsLeft = edge.LeftObstacleOfStart;
            var obsRight = edge.RightObstacleOfStart;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(begin.To3D(), obsLeft.To3D());
            Gizmos.DrawLine(begin.To3D(), obsRight.To3D());

            begin = endVertex;
            obsLeft = edge.LeftObstacleOfEnd;
            obsRight = edge.RightObstacleOfEnd;
            Gizmos.DrawLine(begin.To3D(), obsLeft.To3D());
            Gizmos.DrawLine(begin.To3D(), obsRight.To3D());

        }

        public static void DrawVertex(Vertex vertex)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vertex.Position.To3D(), 3);
        }
        public static void DrawEdge(Edge edge)
        {
            if (edge.IsLinear && !edge.IsTwin)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(edge.Start.Position.To3D(), edge.End.Position.To3D());
            }
        }
    }
}
