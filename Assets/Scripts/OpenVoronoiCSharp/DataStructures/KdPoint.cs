using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenVoronoiCSharp.Internals;
namespace OpenVoronoiCSharp.DataStructures
{
    class KdPoint
    {
        public Point p;
        public Face face;

        public KdPoint(Point p, Face face)
        {
            this.p = p;
            this.face = face;
        }
    }

}
