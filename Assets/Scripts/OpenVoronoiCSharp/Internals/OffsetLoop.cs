using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    class OffsetLoop
    {
        public List<OffsetVertex> vertices = new List<OffsetVertex>(); ///< list of offsetvertices in this loop
        public double offset_distance;
        public void add(OffsetVertex v)
        {
            vertices.Add(v);
        }
    }
}
