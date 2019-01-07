using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class PPPSolver : Solver
    {
    public override int solve(Site s1, double k1, Site s2, double k2, Site s3, double k3, List<Solution> slns)
    {
        Debug.Assert(s1.isPoint() && s2.isPoint() && s3.isPoint(), "s1.isPoint() && s2.isPoint() && s3.isPoint()");
        Point pi = s1.position();
        Point pj = s2.position();
        Point pk = s3.position();

        if (pi.is_right(pj, pk))
        {
            Point tmp = pi;
            pi = pj;
            pj = tmp;
        }
        Debug.Assert(!pi.is_right(pj, pk), " !pi.is_right(pj,pk) ");
        // 2) point pk should have the largest angle. largest angle is opposite longest side.
        double longest_side = pi.sub(pj).norm();
        while ((pj.sub(pk).norm() > longest_side) || ((pi.sub(pk).norm() > longest_side)))
        {
            // cyclic rotation of points until pk is opposite the longest side pi-pj
            Point tmp = pk;
            pk = pj;
            pj = pi;
            pi = tmp;
            longest_side = pi.sub(pj).norm();
        }
        Debug.Assert(!pi.is_right(pj, pk), " !pi.is_right(pj,pk) ");
        Debug.Assert(pi.sub(pj).norm() >= pj.sub(pk).norm(), " pi.sub(pj).norm() >=  pj.sub(pk).norm() ");
        Debug.Assert(pi.sub(pj).norm() >= pk.sub(pi).norm(), " pi.sub(pj).norm() >=  pk.sub(pi).norm() ");

        double J2 = (pi.y - pk.y) * (Numeric.sq(pj.x - pk.x) + Numeric.sq(pj.y - pk.y)) / 2.0 -
            (pj.y - pk.y) * (Numeric.sq(pi.x - pk.x) + Numeric.sq(pi.y - pk.y)) / 2.0;
        double J3 = (pi.x - pk.x) * (Numeric.sq(pj.x - pk.x) + Numeric.sq(pj.y - pk.y)) / 2.0 -
            (pj.x - pk.x) * (Numeric.sq(pi.x - pk.x) + Numeric.sq(pi.y - pk.y)) / 2.0;
        double J4 = (pi.x - pk.x) * (pj.y - pk.y) - (pj.x - pk.x) * (pi.y - pk.y);
        Debug.Assert(J4 != 0.0, " J4 != 0.0 ");
        if (J4 == 0.0)
        {
            throw new Exception(" PPPSolver: Warning divide-by-zero!!");
        }
        Point sln_pt = new Point(-J2 / J4 + pk.x, J3 / J4 + pk.y);
        double dist = sln_pt.sub(pi).norm();
        slns.Add(new Solution(sln_pt, dist, +1));
        return 1;
    }

}
}
