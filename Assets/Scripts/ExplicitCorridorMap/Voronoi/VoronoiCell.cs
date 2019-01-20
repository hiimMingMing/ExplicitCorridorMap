using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplicitCorridorMap.Voronoi
{
    public enum SourceCategory:short { Unknown = -1, SinglePoint = 0, SegmentStartPoint = 1, SegmentEndPoint = 2, InitialSegment = 3, ReverseSegment = 4, GeometryShift = 5, BitMask = 6};

    public class VoronoiCell
    {

        public int ID;
        /// <summary>
        /// The index of the source feature
        /// </summary>
        public int Site { get; set; }

        /// <summary>
        /// True if the cell is made from a point
        /// </summary>
        public bool ContainsPoint { get; set; }

        /// <summary>
        /// True if the cell is made from a segment
        /// </summary>
        public bool ContainsSegment { get; set; }
        
        /// <summary>
        /// Returns true if the cell doesn't have an incident edge. Can happen if a few input segments share a common endpoint.
        /// </summary>
        public bool IsDegnerate { get; set; }

        /// <summary>
        /// The type of element used to create the edge.
        /// </summary>
        public SourceCategory SourceCategory { get; set; }

        


        public VoronoiCell(int id,int site,short sourceCategory,bool containsPoint,bool containsSegment,bool isDegnerate)
        {
            ID = id;
            Site = site;
            SourceCategory = (SourceCategory)sourceCategory;
            ContainsPoint = containsPoint;
            ContainsSegment = containsSegment;
            IsDegnerate = isDegnerate;
        }

    }
}
