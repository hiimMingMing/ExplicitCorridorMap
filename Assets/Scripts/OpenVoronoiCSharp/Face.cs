using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class Face
    {
        public Edge edge;
        public Site site;
        public FaceStatus status;
        public bool is_null_face;
        public Face() { }
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("F(");
            Edge current = edge;
            int c = 0;
            do
            {
                if (current == null) break;
                sb.Append(current.source.position);
                sb.Append(">");
                current = current.next;
                c++;
            } while (current != edge && c < 100);
            if (c >= 100)
            {
                sb.Append("...");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}
