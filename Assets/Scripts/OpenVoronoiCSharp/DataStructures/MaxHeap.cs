using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.DataStructures
{
     interface MaxHeap<T>
    {
         int size();
         void offer(double key, T value);
         void replaceMax(double key, T value);
         void removeMax();
         T getMax();
         double getMaxKey();
    }
}
