using ExplicitCorridorMap;
using ExplicitCorridorMap.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExplicitCorridorMap.Exceptions
{
    public class UnsolvableVertexException:Exception, IParabolaException
    {
        public Vector2 BoostVertex { get; set; }
        public Vector2 ComputedVertex { get; set; }
        public float DistanceBoostVertexToFocus { get; set; }
        public float DistanceComputedVertexToFocus { get; set; }
        public float DistanceBoostVertexToDirectix { get; set; }
        public float DistanceComputedVertexToDirectix { get; set; }

        public ParabolaProblemInformation InputParabolaProblemInfo { get; set; }
        public ParabolaProblemInformation RoatatedParabolaProblemInfo { get; set; }

        public UnsolvableVertexException(ParabolaProblemInformation nonRotatedInformation, ParabolaProblemInformation rotatedInformation, Vector2 boostVertex, Vector2 computedVertex,
            float distanceBoostVertexToFocus, float distanceComputedVertexToFocus, float distanceBoostVertexToDirectix, float distanceComputedVertexToDirectix)
        {
            InputParabolaProblemInfo = nonRotatedInformation;
            RoatatedParabolaProblemInfo = rotatedInformation;

            BoostVertex = boostVertex;
            ComputedVertex = computedVertex;

            DistanceBoostVertexToFocus = distanceBoostVertexToFocus;
            DistanceComputedVertexToFocus = distanceComputedVertexToFocus;
            DistanceBoostVertexToDirectix = distanceBoostVertexToDirectix;
            DistanceComputedVertexToDirectix = distanceComputedVertexToDirectix;
        }

        public UnsolvableVertexException(string message)
            : base(message)
        {
        }

        public UnsolvableVertexException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
