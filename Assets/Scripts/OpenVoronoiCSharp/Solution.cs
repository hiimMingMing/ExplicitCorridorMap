using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class Solution
    {
        /// position
        public Point p;
        /// clearance-disk radius
        public double t;
        /// offset direction to third adjacent Site
        public double k3;

        /// \param pt vertex position
        /// \param tv clearance-disk radius
        /// \param k offset direction
        public Solution(Point pt, double tv, double k)
        {
            this.p = pt;
            this.t = tv;
            this.k3 = k;
        }

        public override String  ToString()
        {
            return String.Format("Solution(p = %s, t = %s, k = %d)",
                                 p, t, (int)k3);
        }
    }
}
