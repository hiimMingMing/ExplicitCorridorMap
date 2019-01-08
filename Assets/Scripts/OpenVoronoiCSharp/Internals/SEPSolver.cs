using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    class SEPSolver : Solver
    {
        public override int solve(Site s1, double k1,
                          Site s2, double k2,
                          Site s3, double k3, List<Solution> slns)
        {
            Debug.Assert(s1.isLine() && s2.isPoint(), " s1.isLine() && s2.isPoint() ");
            Debug.Assert(s3.isLine(), "s3.isLine()");

            // separator direction
            Point sv = new Point(-s1.a(), -s1.b());
            double tsln = -(s3.a() * s2.x() + s3.b() * s2.y() + s3.c()) / (sv.x * s3.a() + sv.y * s3.b() + k3);

            Point psln = new Point(s2.x(), s2.y()).add(sv.mult(tsln));
            slns.Add(new Solution(psln, tsln, k3));
            return 1;
        }

};
}
