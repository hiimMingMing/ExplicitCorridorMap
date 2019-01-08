using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    class Numeric
    {

        /// solve quadratic eqn: a*x*x + b*x + c = 0
        /// returns real roots (0, 1, or 2) as vector
        public static List<double> quadratic_roots(double a, double b, double c)
        {
            List<double> roots = new List<double>();
            if ((a == 0) && (b == 0))
            {
                return roots;
            }
            if (a == 0)
            {
                roots.Add(-c / b);
                return roots;
            }
            if (b == 0)
            {
                double sqr = -c / a;
                if (sqr > 0)
                {
                    roots.Add(Math.Sqrt(sqr));
                    roots.Add(-roots[0]);
                    return roots;
                }
                else if (sqr == 0)
                {
                    roots.Add(0d);
                    return roots;
                }
                else
                {
                    //std::cout << " quadratic_roots() b == 0. no roots.\n";
                    return roots;
                }
            }
            double disc = chop(b * b - 4 * a * c); // discriminant, chop!
            if (disc > 0)
            {
                double q;
                if (b > 0)
                    q = (b + Math.Sqrt(disc)) / -2;
                else
                    q = (b - Math.Sqrt(disc)) / -2;
                roots.Add(q / a);
                roots.Add(c / q);
                return roots;
            }
            else if (disc == 0)
            {
                roots.Add(-b / (2 * a));
                return roots;
            }
            //std::cout << " quadratic_roots() disc < 0. no roots. disc= " << disc << "\n";
            return roots;
        }

        public static double determinant(double a, double b, double c,
                                         double d, double e, double f,
                                         double g, double h, double i)
        {
            return a * (e * i - h * f) - b * (d * i - g * f) + c * (d * h - g * e);
        }

        public static double sq(double a)
        {
            return a * a;
        }

        public static double chop(double val, double tol)
        {
            double _epsilon = tol;
            if (Math.Abs(val) < _epsilon)
                return 0;
            else
                return val;
        }

        public static double chop(double val)
        {
            double _epsilon = 1e-10;
            if (Math.Abs(val) < _epsilon)
                return 0;
            else
                return val;
        }

        public static double diangle(double x, double y)
        {
            if (y >= 0)
                return (x >= 0 ? y / (x + y) : 1 - x / (-x + y));
            else
                return (x < 0 ? 2 - y / (-x - y) : 3 + x / (x - y));
        }
        public static double diangle_x(double a)
        {
            return (a < 2 ? 1 - a : a - 3);
        }
        public static double diangle_y(double a)
        {
            return (a < 3 ? ((a > 1) ? 2 - a : a) : a - 4);
        }


        public static Pair<double, double> diangle_xy(double a)
        {
            double x = diangle_x(a);
            double y = diangle_y(a);
            double norm = Math.Sqrt(x * x + y * y);
            return new Pair<double, double>(x / norm, y / norm);
        }

        // return true if a lies in [less,more]
        public static bool diangle_bracket(double less, double a, double more)
        {
            if (less == more)
            {
                return false;
            }
            else if (less <= more)
            { // normal case..
                if ((less <= a) && (a < more))
                    return true;
            }
            else if (less > more)
            { // we cross zero
                if (((less <= a) && (a <= 4)) || ((0 <= a) && (a < more)))
                    return true;
            }
            else
            {
                Debug.Assert(false, "false");
                return false;
            }

            return false;
        }

        // return average of input angles
        public static double diangle_mid(double alfa1, double alfa2)
        {
            if (alfa1 <= alfa2)
                return (alfa1 + alfa2) / 2;
            else
            {
                double opposite_mid = alfa2 + (alfa1 - alfa2) / 2;
                double mid = opposite_mid + 2;
                if (mid > 4)
                    mid = mid - 4;
                Debug.Assert((0 <= mid) && (mid <= 4), " (0<=mid) && (mid<=4) ");
                return mid;
            }
        }


    }
}
