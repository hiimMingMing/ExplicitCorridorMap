using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    abstract class Filter
    {
        protected HalfEdgeDiagram g; ///< vd-graph
                                     /// set graph
        public void set_graph(HalfEdgeDiagram g)
        {
            this.g = g;
        }
        /// does this edge belong to the filtered graph?
        public abstract bool apply(Edge e);
    };
}
