using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class ArcOfs : Ofs
    {
        Point _start; ///< start
        Point _end;   ///< end
        Point c;      ///< center
    double r;     ///< radius

        /// \param p1 start Point
        /// \param p2 end Point
        /// \param cen center Point
        /// \param rad radius
    public ArcOfs(Point p1, Point p2, Point cen, double rad)
    {
        this._start = p1;
        this._end = p2;
        this.c = cen;
        this.r = rad;
    }

    public override String ToString()
    {
        return String.Format("ArcOfs from %s to %s r=%f\n", _start, _end, r);
    }

    public override double radius() { return r; }
    public override Point center() { return c; }
    public override Point start() { return _start; }
    public override Point end() { return _end; }
};
}
