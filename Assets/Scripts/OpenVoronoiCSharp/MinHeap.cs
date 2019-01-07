using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    interface MinHeap<T>
    {
        int size();
        void offer(double key, T value);
        void replaceMin(double key, T value);
        void removeMin();
        T getMin();
        double getMinKey();
    }
}
