using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    abstract class Site
    {
        /// the HEFace of this Site
        public Face face;
        /// equation parameters
        public Eq eq = new Eq();

        /// return closest point on site to given point p
        public abstract Point apex_point(Point p);

        /// return offset of site
        public abstract Ofs offset(Point p1, Point p2);

        /// position of site for PointSite
        public virtual Point position()
        {
            throw new NotSupportedException();
        }

        /// start point of site (for LineSite and ArcSite)
        public virtual Point start()
        {
            throw new NotSupportedException();
        }

        /// end point of site (for LineSite and ArcSite)
        public virtual Point end()
        {
            throw new NotSupportedException();
        }

        /// return equation parameters
        public virtual Eq eqp()
        {
            return eq;
        }

        /// return equation parameters
        public virtual Eq eqp(double kk)
        {
            Eq eq2 = new Eq(eq);
            eq2.k *= kk;
            return eq2;
        }

        /// true for LineSite
        public virtual bool is_linear()
        {
            return isLine();
        }

        /// true for PointSite and ArcSite
        public virtual bool is_quadratic()
        {
            return isPoint();
        }

        /// x position
        public virtual double x()
        {
            throw new NotSupportedException();
        }

        /// y position
        public virtual double y()
        {
            throw new NotSupportedException();
        }

        /// radius (zero for PointSite)
        public virtual double r()
        {
            throw new NotSupportedException();
        }

        /// offset direction
        public virtual double k()
        {
            throw new NotSupportedException();
        }

        /// LineSite a parameter
        public virtual double a()
        {
            throw new NotSupportedException();
        }

        /// LineSite b parameter
        public virtual double b()
        {
            throw new NotSupportedException();
        }

        /// LineSite c parameter
        public virtual double c()
        {
            throw new NotSupportedException();
        }

        public virtual void set_c(Point p)
        {
            throw new NotSupportedException();
        }

        /// true for PointSite
        public virtual bool isPoint() { return false; }
        /// true for LineSite
        public virtual bool isLine() { return false; }
        /// true for ArcSite
        public virtual bool isArc() { return false; }
        /// true for CW oriented ArcSite
        public virtual bool cw() { return false; }
        /// is given Point in_region ?
        public abstract bool in_region(Point p);
        /// is given Point in region?
        public virtual double in_region_t(Point p)
        {
            throw new NotSupportedException();
        }

        /// in-region t-valye
        public virtual double in_region_t_raw(Point p)
        {
            throw new NotSupportedException();
        }

        /// return edge (if this is a LineSite or ArcSite
        public virtual Edge edge()
        {
            throw new NotSupportedException();
        }

        /// return vertex, if this is a PointSite
        public virtual Vertex vertex()
        {
            throw new NotSupportedException();
        }
    }
}
