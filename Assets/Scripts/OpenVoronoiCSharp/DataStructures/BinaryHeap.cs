using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp.DataStructures
{

    /**
     * An implementation of an implicit binary heap. Min-heap and max-heap both supported 
     */
    public abstract class BinaryHeap<T>
    {
        protected static int defaultCapacity = 64;
        private int direction;
        private Object[] data;
        private double[] keys;
        private int _capacity;
        private int _size;

        protected BinaryHeap(int _capacity, int direction)
        {
            this.direction = direction;
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
            data[_size] = value;
            keys[_size] = key;
            siftUp(_size);
            _size++;
        }

        protected void removeTip()
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            _size--;
            data[0] = data[_size];
            keys[0] = keys[_size];
            data[_size] = null;
            siftDown(0);
        }

        protected void replaceTip(double key, T value)
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            data[0] = value;
            keys[0] = key;
            siftDown(0);
        }

        protected T getTip()
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            return (T)data[0];
        }

        protected double getTipKey()
        {
            if (_size == 0)
            {
                throw new Exception();
            }

            return keys[0];
        }

        private void siftUp(int c)
        {
            for (int p = (c - 1) / 2; c != 0 && direction * keys[c] > direction * keys[p]; c = p, p = (c - 1) / 2)
            {
                Object pData = data[p];
                double pDist = keys[p];
                data[p] = data[c];
                keys[p] = keys[c];
                data[c] = pData;
                keys[c] = pDist;
            }
        }

        private void siftDown(int p)
        {
            for (int c = p * 2 + 1; c < _size; p = c, c = p * 2 + 1)
            {
                if (c + 1 < _size && direction * keys[c] < direction * keys[c + 1])
                {
                    c++;
                }
                if (direction * keys[p] < direction * keys[c])
                {
                    // Swap the points
                    Object pData = data[p];
                    double pDist = keys[p];
                    data[p] = data[c];
                    keys[p] = keys[c];
                    data[c] = pData;
                    keys[c] = pDist;
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

        public int capacity()
        {
            return _capacity;
        }
    }
        class Max<T> : BinaryHeap<T> , MaxHeap<T> {
            public Max():base(defaultCapacity, 1)
            {
                
            }
            public Max(int _capacity):base(_capacity, 1)
            {
            }
            public void removeMax()
            {
                removeTip();
            }
            public void replaceMax(double key, T value)
            {
                replaceTip(key, value);
            }
            public T getMax()
            {
                return getTip();
            }
            public double getMaxKey()
            {
                return getTipKey();
            }
        }
        class Min<T> : BinaryHeap<T> , MinHeap<T> {
            public Min() : base(defaultCapacity, -1) { }
            public Min(int _capacity) : base(_capacity, -1) { }
            
            public void removeMin()
            {
                removeTip();
            }
            public void replaceMin(double key, T value)
            {
                replaceTip(key, value);
            }
            public T getMin()
            {
                return getTip();
            }
            public double getMinKey()
            {
                return getTipKey();
            }
        }
    
}
