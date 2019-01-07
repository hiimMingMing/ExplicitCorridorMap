using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class PointSite : Site
    {
        private Point _p; ///< position
        public Vertex v; ///< vertex descriptor of this PointSite

        public PointSite(Point p)
        {
            this._p = p;
            face = null;
            eq.q = true;
            eq.a = -2 * p.x;
            eq.b = -2 * p.y;
            eq.k = 0;
            eq.c = p.x * p.x + p.y * p.y;
        }

        /// ctor
        public PointSite(Point p, Face f)
        {
            this._p = p;
            face = f;
            eq.q = true;
            eq.a = -2 * p.x;
            eq.b = -2 * p.y;
            eq.k = 0;
            eq.c = p.x * p.x + p.y * p.y;
        }

        /// ctor
        public PointSite(Point p, Face f, Vertex vert)
        {
            this.v = vert;
            this._p = p;
            face = f;
            eq.q = true;
            eq.a = -2 * p.x;
            eq.b = -2 * p.y;
            eq.k = 0;
            eq.c = p.x * p.x + p.y * p.y;
        }

        public override Point apex_point(Point p) { return _p; }
        public override Ofs offset(Point p1, Point p2)
        {
            double rad = p1.sub(_p).norm();
            return new ArcOfs(p1, p2, _p, rad);
        }
        public override Point position() { return _p; }
        public override double x() { return _p.x; }
        public override double y() { return _p.y; }
        public override double r() { return 0; }
        public override double k() { return 0; }
        public override bool isPoint() { return true; }
        public override bool in_region(Point p) { return true; }
        public override double in_region_t(Point p) { return -1; }
        public override Vertex vertex() { return v; }

        public override String ToString()
        {
            return String.Format("PS(%s)", _p);
        }
    };

}
