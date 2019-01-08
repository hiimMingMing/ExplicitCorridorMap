using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.Internals
{

    class Pair<A, B>
    {
        private A first;
        private B second;

        public Pair(A first, B second)
        {
            this.first = first;
            this.second = second;
        }

        public int hashCode()
        {
            int hashFirst = first != null ? first.GetHashCode() : 0;
            int hashSecond = second != null ? second.GetHashCode() : 0;

            return (hashFirst + hashSecond) * hashSecond + hashFirst;
        }

        public bool equals(Object other)
        {
            if (other.GetType().IsInstanceOfType(typeof(Pair<A, B>)))
                {
                    Pair <A, B> otherPair = (Pair <A, B>) other;
                    return
                        ((this.first.Equals(otherPair.first) ||
                            (this.first != null && otherPair.first != null &&
                              this.first.Equals(otherPair.first))) &&
                         (this.second.Equals(otherPair.second) ||
                          (this.second != null && otherPair.second != null &&
                            this.second.Equals(otherPair.second))));
                }

            return false;
            
        }

        public String toString()
        {
            return "(" + first + ", " + second + ")";
        }

        public A getFirst()
        {
            return first;
        }

        public void setFirst(A first)
        {
            this.first = first;
        }

        public B getSecond()
        {
            return second;
        }

        public void setSecond(B second)
        {
            this.second = second;
        }
    }

}
