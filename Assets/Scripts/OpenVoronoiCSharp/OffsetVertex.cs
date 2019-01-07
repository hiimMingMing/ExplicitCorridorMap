using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class OffsetVertex
    {
        public Point p; ///< position (start)
        public double r; ///< arc radius (line-vertex is indicated by radius of -1)
        public Point c; ///< arc center
        public bool cw; ///< clockwise (or not)
        public Face f; ///< corresponding face in the vd-graph
        public Edge e; /// corresponding edge

        public OffsetVertex(Point p, double r, Point c, bool cw, Face f, Edge e)
        {
            this.p = p;
            this.r = r;
            this.c = c;
            this.cw = cw;
            this.f = f;
            this.e = e;
        }

        public OffsetVertex(Point p, Edge e)
        {
            this.p = p;
            r = -1;
            c = null;
            cw = false;
            f = null;
            this.e = e;
        }
    }

}
