using ExplicitCorridorMap.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplicitCorridorMap.Exceptions
{
    public interface IParabolaException
    {
         ParabolaProblemInformation InputParabolaProblemInfo { get; set; }
    }
}
