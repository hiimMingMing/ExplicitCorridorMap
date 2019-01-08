using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{
    public enum FaceStatus
    {
        INCIDENT,    /*!< INCIDENT faces contain one or more IN-vertex */
        NONINCIDENT  /*!< NONINCIDENT faces contain only OUT/UNDECIDED-vertices */
    }

}
