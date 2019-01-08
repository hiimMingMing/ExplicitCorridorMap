using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    public enum VertexStatus
    {
        OUT,          /*!< OUT-vertices will not be deleted */
        IN,           /*!< IN-vertices will be deleted */
        UNDECIDED,    /*!< UNDECIDED-vertices have not been examied yet */
        NEW           /*!< NEW-vertices are constructed on OUT-IN edges */
    }
}
