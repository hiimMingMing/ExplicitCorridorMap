using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    abstract class Ofs
    {
        /// radius, -1 if line
        public abstract double radius(); // {return -1;}

        /// center (for arc)
        public abstract Point center(); //{return Point(0,0);}

        /// start point
        public abstract Point start();//{return Point(0,0);}
                                      /// end point
        public abstract Point end(); //{return Point(0,0);}
    };
}
