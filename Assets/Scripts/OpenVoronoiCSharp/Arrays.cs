using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class Arrays
    {
        public static T[] copyOf<T>(T[] src, int len)
        {
            T[] dest = new T[len];
            // note i is always from 0
            for (int i = 0; i < len; i++)
            {
                dest[i] = src[i]; // so 0..n = 0+x..n+x
            }
            return dest;
        }
    }
}
