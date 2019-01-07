using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class KdNode<T>
    {
        // All types
        public int dimensions;
        public int bucketCapacity;
        protected int _size;

        // Leaf only
        public double[][] points;
        public Object[] data;

        // Stem only
        public KdNode<T> left, right;
        public int splitDimension;
        public double splitValue;

        // Bounds
        public double[] minBound, maxBound;
        public bool singlePoint;

        protected KdNode(int dimensions, int bucketCapacity)
        {
            // Init base
            this.dimensions = dimensions;
            this.bucketCapacity = bucketCapacity;
            this._size = 0;
            this.singlePoint = true;

            // Init leaf elements
            this.points = new double[bucketCapacity + 1][];
            this.data = new Object[bucketCapacity + 1];
        }

        /* -------- SIMPLE GETTERS -------- */

        public int size()
        {
            return _size;
        }

        public bool isLeaf()
        {
            return points != null;
        }

        /* -------- OPERATIONS -------- */

        public void addPoint(double[] point, T value)
        {
            KdNode<T> cursor = this;
            while (!cursor.isLeaf())
            {
                cursor.extendBounds(point);
                cursor._size++;
                if (point[cursor.splitDimension] > cursor.splitValue)
                {
                    cursor = cursor.right;
                }
                else
                {
                    cursor = cursor.left;
                }
            }
            cursor.addLeafPoint(point, value);
        }

        /* -------- INTERNAL OPERATIONS -------- */

        public void addLeafPoint(double[] point, T value)
        {
            // Add the data point
            points[_size] = point;
            data[_size] = value;
            extendBounds(point);
            _size++;

            if (_size == points.Length - 1)
            {
                // If the node is getting too large
                if (calculateSplit())
                {
                    // If the node successfully had it's split value calculated, split node
                    splitLeafNode();
                }
                else
                {
                    // If the node could not be split, enlarge node
                    increaseLeafCapacity();
                }
            }
        }

        private bool checkBounds(double[] point)
        {
            for (int i = 0; i < dimensions; i++)
            {
                if (point[i] > maxBound[i]) return false;
                if (point[i] < minBound[i]) return false;
            }
            return true;
        }

        private void extendBounds(double[] point)
        {
            if (minBound == null)
            {
                minBound = Arrays.copyOf(point, dimensions);
                maxBound = Arrays.copyOf(point, dimensions);
                return;
            }

            for (int i = 0; i < dimensions; i++)
            {
                if (Double.IsNaN(point[i]))
                {
                    if (!Double.IsNaN(minBound[i]) || !Double.IsNaN(maxBound[i]))
                    {
                        singlePoint = false;
                    }
                    minBound[i] = Double.NaN;
                    maxBound[i] = Double.NaN;
                }
                else if (minBound[i] > point[i])
                {
                    minBound[i] = point[i];
                    singlePoint = false;
                }
                else if (maxBound[i] < point[i])
                {
                    maxBound[i] = point[i];
                    singlePoint = false;
                }
            }
        }

        private void increaseLeafCapacity()
        {
            points = Arrays.copyOf(points, points.Length * 2);
            data = Arrays.copyOf(data, data.Length * 2);
        }

        private bool calculateSplit()
        {
            if (singlePoint) return false;

            double width = 0;
            for (int i = 0; i < dimensions; i++)
            {
                double dwidth = (maxBound[i] - minBound[i]);
                if (Double.IsNaN(dwidth)) dwidth = 0;
                if (dwidth > width)
                {
                    splitDimension = i;
                    width = dwidth;
                }
            }

            if (width == 0)
            {
                return false;
            }

            // Start the split in the middle of the variance
            splitValue = (minBound[splitDimension] + maxBound[splitDimension]) * 0.5;

            // Never split on infinity or NaN
            if (splitValue == Double.PositiveInfinity)
            {
                splitValue = Double.MaxValue;
            }
            else if (splitValue == Double.NegativeInfinity)
            {
                splitValue = -Double.MinValue;
            }

            // Don't let the split value be the same as the upper value as
            // can happen due to rounding errors!
            if (splitValue == maxBound[splitDimension])
            {
                splitValue = minBound[splitDimension];
            }

            // Success
            return true;
        }

        // I have no idea what the following code does, it's not written by me.
        // Thus, I can only suppress the warnings generated by `(T) oldData` cast.
    private void splitLeafNode()
        {
            right = new KdNode<T>(dimensions, bucketCapacity);
            left = new KdNode<T>(dimensions, bucketCapacity);

            // Move locations into children
            for (int i = 0; i < _size; i++)
            {
                double[] oldLocation = points[i];
                Object oldData = data[i];
                if (oldLocation[splitDimension] > splitValue)
                {
                    right.addLeafPoint(oldLocation, (T)oldData);
                }
                else
                {
                    left.addLeafPoint(oldLocation, (T)oldData);
                }
            }

            points = null;
            data = null;
        }
    }
}
