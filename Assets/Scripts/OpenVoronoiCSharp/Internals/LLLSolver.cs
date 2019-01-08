using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    class LLLSolver : Solver
    {

//  a1 x + b1 y + c1 + k1 t = 0
//  a2 x + b2 y + c2 + k2 t = 0
//  a3 x + b3 y + c3 + k3 t = 0
//
// or in matrix form
//
//  ( a1 b1 k1 ) ( x )    ( c1 )
//  ( a2 b2 k2 ) ( y ) = -( c2 )          Ax = b
//  ( a3 b3 k3 ) ( t )    ( c3 )
//
//  Cramers rule x_i = det(A_i)/det(A)
//  where A_i is A with column i replaced by b

    public override int solve(Site s1, double k1,
                      Site s2, double k2,
                      Site s3, double k3, List<Solution> slns)
    {

        Debug.Assert(s1.isLine() && s2.isLine() && s3.isLine(), " s1.isLine() && s2.isLine() && s3.isLine() ");

        List<Eq> eq = new List<Eq>(); // equation-parameters, in quad-precision
        Site[] sites = new Site[] { s1, s2, s3 };
        double[] kvals = new double[] { k1, k2, k3 };
        for (int ir = 0; ir < 3; ir++)
            eq.Add(sites[ir].eqp(kvals[ir]));

        int i = 0, j = 1, k = 2;
        double d = Numeric.chop(Numeric.determinant(eq[i].a, eq[i].b, eq[i].k,
                                      eq[j].a, eq[j].b, eq[j].k,
                                      eq[k].a, eq[k].b, eq[k].k));
        double det_eps = 1e-6;
        if (Math.Abs(d) > det_eps)
        {
            double t = Numeric.determinant(eq[i].a, eq[i].b, -eq[i].c,
                                     eq[j].a, eq[j].b, -eq[j].c,
                                     eq[k].a, eq[k].b, -eq[k].c) / d;
            if (t >= 0)
            {
                double sol_x = Numeric.determinant(-eq[i].c, eq[i].b, eq[i].k,
                                             -eq[j].c, eq[j].b, eq[j].k,
                                             -eq[k].c, eq[k].b, eq[k].k) / d;
                double sol_y = Numeric.determinant(eq[i].a, -eq[i].c, eq[i].k,
                                             eq[j].a, -eq[j].c, eq[j].k,
                                             eq[k].a, -eq[k].c, eq[k].k) / d;

                slns.Add(new Solution(new Point(sol_x, sol_y), t, k3)); // kk3 just passes through without any effect!?
                return 1;
            }
        }
        else
        {
            // Try parallel solver as fallback, if the small Numeric.determinant is due to nearly parallel edges
            for (i = 0; i < 3; i++)
            {
                j = (i + 1) % 3;
                double delta = Math.Abs(eq[i].a * eq[j].b - eq[j].a * eq[i].b);
                if (delta <= 1e-300)
                {
                    LLLPARASolver para_solver = new LLLPARASolver();
                    List<Solution> paraSolutions = new List<Solution>();
                    para_solver.solve(sites[i], kvals[i], sites[j], kvals[j], sites[(i + 2) % 3], kvals[(i + 2) % 3], paraSolutions);
                    int solution_count = 0;
                    foreach (Solution s in paraSolutions)
                    {
                        // check that solution has proper offset-direction
                        if (s3.end().sub(s3.start()).cross(s.p.sub(s3.start())) * k3 >= 0)
                        {
                            slns.Add(s);
                            solution_count++;
                        }
                    }
                    return solution_count;
                }
            }
        }
        return 0; // no solution if Numeric.determinant zero, or t-value negative
    }

}
}
