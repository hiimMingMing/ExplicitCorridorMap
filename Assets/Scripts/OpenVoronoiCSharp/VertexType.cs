using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    public enum VertexType
    {
        OUTER,      /*!< OUTER vertices are special vertices added in init(), should have degree==4 */
        NORMAL,     /*!< NORMAL are normal voronoi-vertices, should have degree==6  (degree 3 graph with double-edges) */
        POINTSITE,  /*!< POINTSITE are point sites, should have degree==0 */
        ENDPOINT,   /*!< ENDPOINT vertices are end-points of line-segments or arc-segments */
        SEPPOINT,   /*!< separator start-vertices on a null-face */
        APEX,       /*!< APEX vertices split quadratic edges at their apex(closest point to site) */
        SPLIT       /*!< split-vertices of degree==2 to avoid loops in the delete-tree */
    };
}
