using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OpenVoronoiCSharp.DataStructures
{
     class NearestNeighborIterator<T>  {
    private DistanceFunction distanceFunction;
    private double[] searchPoint;
    private MinHeap<KdNode<T>> pendingPaths;
    private IntervalHeap<T> evaluatedPoints;
    private int pointsRemaining;
    private double lastDistanceReturned;

    public NearestNeighborIterator(KdNode<T> treeRoot, double[] searchPoint, int maxPointsReturned, DistanceFunction distanceFunction)
    {
        this.searchPoint = Arrays.copyOf(searchPoint, searchPoint.Length);
        this.pointsRemaining = Math.Min(maxPointsReturned, treeRoot.size());
        this.distanceFunction = distanceFunction;
        this.pendingPaths = new Min<KdNode<T>>();
        this.pendingPaths.offer(0, treeRoot);
        this.evaluatedPoints = new IntervalHeap<T>();
    }

    /* -------- INTERFACE IMPLEMENTATION -------- */

    public bool hasNext()
    {
        return pointsRemaining > 0;
    }

    public T next()
    {
        if (!hasNext())
        {
            throw new Exception("NearestNeighborIterator has reached end!");
        }

        while (pendingPaths.size() > 0 && (evaluatedPoints.size() == 0 || (pendingPaths.getMinKey() < evaluatedPoints.getMinKey())))
        {
            KdTree<T>.nearestNeighborSearchStep(pendingPaths, evaluatedPoints, pointsRemaining, distanceFunction, searchPoint);
        }

        // Return the smallest distance point
        pointsRemaining--;
        lastDistanceReturned = evaluatedPoints.getMinKey();
        T value = evaluatedPoints.getMin();
        evaluatedPoints.removeMin();
        return value;
    }

    public double distance()
    {
        return lastDistanceReturned;
    }

    public void remove()
    {
        throw new NotSupportedException();
    }

    public NearestNeighborIterator<T> iterator()
    {
        return this;
    }
}
}
