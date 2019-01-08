using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    class ALTSEPSolver : Solver
    {

    public override int solve(Site s1, double k1,
                      Site s2, double k2,
                      Site s3, double k3, List<Solution> slns)
    {
        Site lsite;
        Site psite;
        Site third_site;
        double lsite_k, third_site_k;

        if (type == 0)
        {
            lsite = s3; lsite_k = k3;
            psite = s1; // psite_k = k1;    l3 / p1 form a separator
            third_site = s2; third_site_k = 1;
        }
        else if (type == 1)
        {
            lsite = s3; lsite_k = k3;
            psite = s2; // psite_k = k2;    l3 / p2 form a separator
            third_site = s1; third_site_k = 1;
        }
        else
        {
            throw new Exception("ALTSEPSolver FATAL ERROR! type not known.");
        }
        // separator direction
        Point sv = (k3 == -1) ? new Point(lsite.a(), lsite.b()) : new Point(-lsite.a(), -lsite.b());

        // now we should have this:
        Debug.Assert(lsite.isLine() && psite.isPoint(), " lsite.isLine() && psite.isPoint() ") ;

        double tsln = 0;

        if (third_site.isPoint())
        {
            double dx = psite.x() - third_site.x();
            double dy = psite.y() - third_site.y();
            if (Math.Abs(2 * (dx * sv.x + dy * sv.y)) > 0)
            {
                tsln = -(dx * dx + dy * dy) / (2 * (dx * sv.x + dy * sv.y)); // check for divide-by-zero?
            }
            else
            {
                //std::cout << " no solutions. (isPoint)\n";
                return 0;
            }
        }
        else if (third_site.isLine())
        {
            if (Math.Abs((sv.x * third_site.a() + sv.y * third_site.b() + third_site_k)) > 0)
            {
                tsln = -(third_site.a() * psite.x() + third_site.b() * psite.y() + third_site.c()) /
                    (sv.x * third_site.a() + sv.y * third_site.b() + third_site_k);
            }
            else
            {
                //std::cout << " no solutions. (isLine)\n";
                return 0;
            }
        }
        else
        {
            Debug.Assert(false,"false");
        }
        Point psln = new Point(psite.x(), psite.y()).add(sv.mult(tsln));
        slns.Add(new Solution(psln, tsln, k3));
        return 1;
    }

}
}
