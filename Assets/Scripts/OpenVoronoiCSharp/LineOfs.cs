using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class LineOfs : Ofs
    {
    protected Point _start; ///< start point
    protected Point _end;  ///< end point
    /// \param p1 start point
    /// \param p2 end point
    public LineOfs(Point p1, Point p2)
    {
        this._start = p1;
        this._end = p2;
    }

    public override double radius() { return -1; }
    public override Point center() { return new Point(0, 0); }
    public override Point start() { return _start; }
    public override Point end() { return _end; }
};
}
