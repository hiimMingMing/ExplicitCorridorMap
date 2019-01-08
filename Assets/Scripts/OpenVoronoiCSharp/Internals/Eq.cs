using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    class Eq
    {

        public bool q; ///< true for quadratic, false for linear
        public double a; ///< a parameter of line-equation
        public double b; ///< b parameter of line equation
        public double c; ///< c parameter of line equation
        public double k; ///< offset direction parameter

        /// default ctor
        public Eq()
        {
            a = 0;
            b = 0;
            c = 0;
            k = 0;
            q = false;
        }

        public Eq(Eq other)
        {
            a = other.a;
            b = other.b;
            c = other.c;
            k = other.k;
            q = other.q;
        }

        public override bool Equals(Object other)
        {
            if (other.GetType().IsInstanceOfType(typeof(Eq))) {
                Eq o = (Eq)other;
                return a == o.a && b == o.b && c == o.c;
            } else {
                return false;
            }
        }

        
        public override int GetHashCode()
        {
            int h = 0;
            h *= 42;
            h += a.GetHashCode();
            h *= 42;
            h += b.GetHashCode();
            h *= 42;
            h += c.GetHashCode();
            return h;
        }

        /// subtract two equations from eachother
        public void subEq(Eq other)
        {
            a -= other.a;
            b -= other.b;
            c -= other.c;
            k -= other.k;
        }

        /// subtraction
        public Eq sub(Eq other)
        {
            Eq res = new Eq();
            res.subEq(other);
            return res;
        }

        /// access parameters through operator[]
        public double get(int idx)
        {
            switch (idx)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return k;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override String ToString()
        {
            return String.Format("Eq(q=%s,a=%s,b=%s,c=%s,k=%s)", q, a, b, c, k);
        }
        
    }
}
