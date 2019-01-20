using ExplicitCorridorMap.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplicitCorridorMap.Exceptions
{
    public class FocusOnDirectixException : Exception,IParabolaException
    {
        public ParabolaProblemInformation InputParabolaProblemInfo { get; set; }

        public FocusOnDirectixException(ParabolaProblemInformation parabolaProblemInformation)
        {
            InputParabolaProblemInfo = parabolaProblemInformation;
        }

        public FocusOnDirectixException(string message)
            : base(message)
        {
        }

        public FocusOnDirectixException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
