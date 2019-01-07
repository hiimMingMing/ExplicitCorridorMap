using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class IntervalHeap<T> : MinHeap<T>, MaxHeap<T>
    {
        private static int defaultCapacity = 64;
        private Object[] data;
        private double[] keys;
        private int _capacity;
        private int _size;

        public IntervalHeap() : this(IntervalHeap<T>.defaultCapacity)
        {
        }

        public IntervalHeap(int _capacity)
        {
            this.data = new Object[_capacity];
            this.keys = new double[_capacity];
            this._capacity = _capacity;
            this._size = 0;
        }

        public void offer(double key, T value)
        {
            // If move room is needed, double array _size
            if (_size >= _capacity)
            {
                _capacity *= 2;
                data = Arrays.copyOf(data, _capacity);
                keys = Arrays.copyOf(keys, _capacity);
            }

            // Insert new value at the end
            _size++;
            data[_size - 1] = value;
            keys[_size - 1] = key;
            siftInsertedValueUp();
        }

        public void removeMin()
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            _size--;
            data[0] = data[_size];
            keys[0] = keys[_size];
            data[_size] = null;
            siftDownMin(0);
        }

        public void replaceMin(double key, T value)
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            data[0] = value;
            keys[0] = key;
            if (_size > 1)
            {
                // Swap with pair if necessary
                if (keys[1] < key)
                {
                    swap(0, 1);
                }
                siftDownMin(0);
            }
        }

        public void removeMax()
        {
            if (_size == 0)
            {
                throw new Exception();
            }
            else if (_size == 1)
            {
                removeMin();
                return;
            }

            _size--;
            data[1] = data[_size];
            keys[1] = keys[_size];
            data[_size] = null;
            siftDownMax(1);
        }

        public void replaceMax(double key, T value)
        {
            if (_size == 0)
            {
                throw new Exception();
            }
            else if (_size == 1)
            {
                replaceMin(key, value);
                return;
            }

            data[1] = value;
            keys[1] = key;
            // Swap with pair if necessary
            if (key < keys[0])
            {
                swap(0, 1);
            }
            siftDownMax(1);
        }

        public T getMin()
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            return (T)data[0];
        }

        public T getMax()
        {
            if (_size == 0)
            {
                throw new Exception();
            }
            else if (_size == 1)
            {
                return (T)data[0];
            }

            return (T)data[1];
        }

        public double getMinKey()
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            return keys[0];
        }

        public double getMaxKey()
        {
            if (_size == 0)
            {
                throw new Exception();
            }
            else if (_size == 1)
            {
                return keys[0];
            }

            return keys[1];
        }

        private int swap(int x, int y)
        {
            Object yData = data[y];
            double yDist = keys[y];
            data[y] = data[x];
            keys[y] = keys[x];
            data[x] = yData;
            keys[x] = yDist;
            return y;
        }

        /**
         * Min-side (u % 2 == 0):
         * - leftchild:  2u + 2
         * - rightchild: 2u + 4
         * - parent:     (x/2-1)&~1
         *
         * Max-side (u % 2 == 1):
         * - leftchild:  2u + 1
         * - rightchild: 2u + 3
         * - parent:     (x/2-1)|1
         */

        private void siftInsertedValueUp()
        {
            int u = _size - 1;
            if (u == 0)
            {
                // Do nothing if it's the only element!
            }
            else if (u == 1)
            {
                // If it is the second element, just sort it with it's pair
                if (keys[u] < keys[u - 1])
                { // If less than it's pair
                    swap(u, u - 1); // Swap with it's pair
                }
            }
            else if (u % 2 == 1)
            {
                // Already paired. Ensure pair is ordered right
                int p = (u / 2 - 1) | 1; // The larger value of the parent pair
                if (keys[u] < keys[u - 1])
                { // If less than it's pair
                    u = swap(u, u - 1); // Swap with it's pair
                    if (keys[u] < keys[p - 1])
                    { // If smaller than smaller parent pair
                      // Swap into min-heap side
                        u = swap(u, p - 1);
                        siftUpMin(u);
                    }
                }
                else
                {
                    if (keys[u] > keys[p])
                    { // If larger that larger parent pair
                      // Swap into max-heap side
                        u = swap(u, p);
                        siftUpMax(u);
                    }
                }
            }
            else
            {
                // Inserted in the lower-value slot without a partner
                int p = (u / 2 - 1) | 1; // The larger value of the parent pair
                if (keys[u] > keys[p])
                { // If larger that larger parent pair
                  // Swap into max-heap side
                    u = swap(u, p);
                    siftUpMax(u);
                }
                else if (keys[u] < keys[p - 1])
                { // If smaller than smaller parent pair
                  // Swap into min-heap side
                    u = swap(u, p - 1);
                    siftUpMin(u);
                }
            }
        }

        private void siftUpMin(int c)
        {
            // Min-side parent: (x/2-1)&~1
            for (int p = (c / 2 - 1) & ~1; p >= 0 && keys[c] < keys[p]; c = p, p = (c / 2 - 1) & ~1)
            {
                swap(c, p);
            }
        }

        private void siftUpMax(int c)
        {
            // Max-side parent: (x/2-1)|1
            for (int p = (c / 2 - 1) | 1; p >= 0 && keys[c] > keys[p]; c = p, p = (c / 2 - 1) | 1)
            {
                swap(c, p);
            }
        }

        private void siftDownMin(int p)
        {
            for (int c = p * 2 + 2; c < _size; p = c, c = p * 2 + 2)
            {
                if (c + 2 < _size && keys[c + 2] < keys[c])
                {
                    c += 2;
                }
                if (keys[c] < keys[p])
                {
                    swap(p, c);
                    // Swap with pair if necessary
                    if (c + 1 < _size && keys[c + 1] < keys[c])
                    {
                        swap(c, c + 1);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void siftDownMax(int p)
        {
            for (int c = p * 2 + 1; c <= _size; p = c, c = p * 2 + 1)
            {
                if (c == _size)
                {
                    // If the left child only has half a pair
                    if (keys[c - 1] > keys[p])
                    {
                        swap(p, c - 1);
                    }
                    break;
                }
                else if (c + 2 == _size)
                {
                    // If there is only room for a right child lower pair
                    if (keys[c + 1] > keys[c])
                    {
                        if (keys[c + 1] > keys[p])
                        {
                            swap(p, c + 1);
                        }
                        break;
                    }
                }
                else if (c + 2 < _size)
                {
                    // If there is room for a right child upper pair
                    if (keys[c + 2] > keys[c])
                    {
                        c += 2;
                    }
                }
                if (keys[c] > keys[p])
                {
                    swap(p, c);
                    // Swap with pair if necessary
                    if (keys[c - 1] > keys[c])
                    {
                        swap(c, c - 1);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public int size()
        {
            return _size;
        }

        public int Capacity()
        {
            return _capacity;
        }



        private bool validateHeap()
        {
            // Validate left-right
            for (int i = 0; i < _size - 1; i += 2)
            {
                if (keys[i] > keys[i + 1]) return false;
            }
            // Validate within parent interval
            for (int i = 2; i < _size; i++)
            {
                double maxParent = keys[(i / 2 - 1) | 1];
                double minParent = keys[(i / 2 - 1) & ~1];
                if (keys[i] > maxParent || keys[i] < minParent) return false;
            }
            return true;
        }
    }
}
