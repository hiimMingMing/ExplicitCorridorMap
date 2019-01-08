using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class ArcSite : Site
    {
        Point _start;  ///< start Point of arc
        Point _end;    ///< end Point of arc
        Point _center; ///< center Point of arc
        bool _dir;     ///< CW or CCW direction flag
        double _radius;///< radius of arc
        double _k; ///< offset-direction. +1 for enlarging, -1 for shrinking circle
        Edge e; ///< edge_descriptor to ::ARCSITE pseudo-edge

        /// create arc-site
        public ArcSite(Point startpt, Point endpt, Point centr, bool dir)
        {
            this._start = startpt;
            this._end = endpt;
            this._center = centr;
            this._dir = dir;
            this._radius = _center.sub(_start).norm();
            eq.q = true;
            eq.a = -2 * _center.x;
            eq.b = -2 * _center.y;
            _k = 1;
            eq.k = -2 * _k * _radius;
            eq.c = _center.x * _center.x + _center.y * _center.y - _radius * _radius;
        }

        public override Ofs offset(Point p1, Point p2)
        {
            return new ArcOfs(p1, p2, _center, -1);
        } //FIXME: radius

        public override bool in_region(Point p)
        {
            if (p == _center)
                return true;

            double t = in_region_t(p);
            return ((t >= 0) && (t <= 1));
        }

        /// \todo fix arc-site in_region_t test!!
        public override double in_region_t(Point pt)
        {
            double t = in_region_t_raw(pt); //(diangle_pt - diangle_min) / (diangle_max-diangle_min);
            double eps = 1e-7;
            if (Math.Abs(t) < eps)  // rounding... UGLY
                t = 0.0;
            else if (Math.Abs(t - 1.0) < eps)
                t = 1.0;
            return t;
        }

        public override double in_region_t_raw(Point pt)
        {
            // projection onto circle
            Point cen_start = _start.sub(_center);
            Point cen_end = _end.sub(_center);
            Point cen_pt = pt.sub(_center);
            Point proj = _center.add(cen_pt.mult(_radius / cen_pt.norm()));

            double diangle_min;
            double diangle_max;
            if (!_dir)
            {
                diangle_min = Numeric.diangle(cen_start.x, cen_start.y);
                diangle_max = Numeric.diangle(cen_end.x, cen_end.y);
            }
            else
            {
                diangle_max = Numeric.diangle(cen_start.x, cen_start.y);
                diangle_min = Numeric.diangle(cen_end.x, cen_end.y);
            }
            double diangle_pt = Numeric.diangle(cen_pt.x, cen_pt.y);

            double t = (diangle_pt - diangle_min) / (diangle_max - diangle_min);
            return t;
        }

        public override Point apex_point(Point p)
        {
            if (in_region(p))
                return projection_point(p);
            else
                return closer_endpoint(p);
        }

        public override double x() { return _center.x; }
        public override double y() { return _center.y; }
        public override double r() { return _radius; }
        public override double k() { return _k; } // ?

        /// return start Point of ArcSite
        public override Point start() { return _start; }
        /// return end Point of ArcSite
        public override Point end() { return _end; }
        /// return center Point of ArcSite
        public Point center() { return _center; }
        /// return radius of ArcSite
        public double radius() { return _radius; }
        /// return true for CW ArcSite and false for CCW
        public override bool cw() { return _dir; }
        public override bool isArc() { return true; }

        /// projection of given Point onto the ArcSite
        private Point projection_point(Point p)
        {
            if (p == _center)
            {
                return _start;
            }
            else
            {
                Point dir = p.sub(_center);
                dir.normalize();
                return _center.add(dir.mult(_radius)); // this point should lie on the arc
            }
        }
        /// return the end Point (either _start or _end) that is closest to the given Point
        private Point closer_endpoint(Point p)
        {
            double d_start = _start.sub(p).norm();
            double d_end = _end.sub(p).norm();
            if (d_start < d_end)
                return _start;
            else
                return _end;
        }
    };

}
