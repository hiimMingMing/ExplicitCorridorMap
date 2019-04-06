using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Advanced.Algorithms.DataStructures
{
    public class SpatialData : Polygon
    {
        public MBRectangle mBRectangle;
        public virtual void ComputeMBRectangle() { }
    }
}
