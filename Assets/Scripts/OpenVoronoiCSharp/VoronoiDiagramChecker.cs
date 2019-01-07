using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVoronoiCSharp
{
    class VoronoiDiagramChecker
    {
        private HalfEdgeDiagram g; ///< vd-graph

        public VoronoiDiagramChecker(HalfEdgeDiagram gi)
        {
            this.g = gi;
        }

        /// overall sanity-check for the diagram, calls other sanity-check functions
        public bool is_valid()
        {
            return (all_faces_ok() &&
                        vertex_degree_ok() &&
                        face_count_equals_generator_count()
                        );
        }

        /// check that number of faces equals the number of generators
        /// \todo not implemented!
        public bool face_count_equals_generator_count()
        {
            // Euler formula for planar graphs
            // v - e + f = 2
            // in a half-edge diagram all edges occur twice, so:
            // f = 2-v+e
            //int vertex_count = hedi::num_vertices(g);
            /*int vertex_count = 0;
              BOOST_FOREACH( HEVertex v, hedi::vertices( g ) ) {
              if ( g[v].type == NORMAL )
              vertex_count++;
              }
              int face_count = (vertex_count- 4)/2 + 3; // degree three graph
              //int face_count = hed.num_faces();
              if (face_count != gen_count) {
              std::cout << " face_count_equals_generator_count() ERROR:\n";
              std::cout << " num_vertices = " << vertex_count << "\n";
              std::cout << " gen_count = " << gen_count << "\n";
              std::cout << " face_count = " << face_count << "\n";
              }
              return ( face_count == gen_count );
              * */
            return true;
        }

        /// check that the diagram is of degree three.
        /// however ::SPLIT and ::APEX vertices are of degree 2.
        public bool vertex_degree_ok()
        {
            foreach (Vertex v in g.vertices)
            {
                if (v.degree() != Vertex.expected_degree[v.type])
                {
                    return false;
                }
            }
            return true;
        }


        /// check that all vertices in the input vector have status ::IN
        public bool all_in(List<Vertex> q)
        {
            foreach (Vertex v in q)
            {
                if (v.status != VertexStatus.IN)
                {
                    return false;
                }
            }
            return true;
        }

        /// check that no undecided vertices remain in the face
        public bool noUndecidedInFace(Face f)
        { // is this true??
            List<Vertex> face_verts = g.face_vertices(f);
            foreach (Vertex v in face_verts)
            {
                if (v.status == VertexStatus.UNDECIDED)
                {
                    return false;
                }
            }
            return true;
        }

        /// check that for HEFace f the vertices TYPE are connected
        public bool faceVerticesConnected(Face f, VertexStatus Vtype)
        {
            List<Vertex> face_verts = g.face_vertices(f);
            List<Vertex> type_verts = new List<Vertex>();
            foreach (Vertex v in face_verts)
            {
                if (v.status == Vtype)
                    type_verts.Add(v); // build a vector of all Vtype vertices
            }
            Debug.Assert(type_verts.Count != 0, " !type_verts.isEmpty() ");
            if (type_verts.Count == 1) // set of 1 is allways connected
                return true;

            // check that type_verts are connected
            Edge currentEdge = f.edge;
            Vertex endVertex = currentEdge.source; // stop when target here
            List<Edge> startEdges = new List<Edge>();
            bool done = false;
            while (!done)
            {
                Vertex src = currentEdge.source;
                Vertex trg = currentEdge.target;
                if (src.status != Vtype)
                { // seach ?? - Vtype
                    if (trg.status == Vtype)
                    { // we have found ?? - Vtype
                        startEdges.Add(currentEdge);
                    }
                }
                currentEdge = currentEdge.next;
                if (trg == endVertex)
                {
                    done = true;
                }
            }
            Debug.Assert(startEdges.Count != 0, " !startEdges.isEmpty() ");
            if (startEdges.Count != 1) // when the Vtype vertices are connected, there is exactly one startEdge
                return false;
            else
                return true;
        }

        /// check that all faces are ok. calls face_ok()
        public bool all_faces_ok()
        {
            foreach (Face f in g.faces)
            {
                if (!face_ok(f))
                {
                    return false;
                }
            }
            return true;
        }

        /// check that the face is ok
        public bool face_ok(Face f)
        {
            Edge current_edge = f.edge;

            Edge start_edge = current_edge;
            double k = current_edge.k;
            if (!((k == 1) || (k == -1)))
            {
                // std::cout << " VoronoiDiagramChecker::face_ok() f=" << f << " ERROR:\n";
                // std::cout << " illegal k-value for edge:";
                // std::cout << g[ g.source(current_edge)].index << " - ";
                // std::cout <<  g[ g.target(current_edge)].index  ;
                // std::cout << " k= " << k << "\n";
                return false;
            }
            if (f.site != null)
            { // guard against null-faces that dont have Site
                if (f.site.isPoint())
                {
                    if (!(k == 1))
                    {
                        // std::cout << " VoronoiDiagramChecker::face_ok() f=" << f << " ERROR:\n";
                        // std::cout << " f = " << f << " site is " << g[f].site->str() << " but k=" << k  << "\n";
                        // std::cout << " null? " << g[f].null << "\n";
                        return false;
                    }
                }
            }
            int n = 0;
            do
            {
                if (current_edge.k != k)
                { // all edges should have the same k-value
                    return false;
                }
                if (!current_face_equals_next_face(current_edge))
                {// all edges should have the same face
                    return false;
                }

                if (!check_edge(current_edge))
                {
                    return false;
                }

                current_edge = current_edge.next;
                n++;
                Debug.Assert(n < 10000, " n < 10000 ");
            } while (current_edge != start_edge);
            return true;
        }

        /// check that current edge and next-edge are on the same face
        public bool current_face_equals_next_face(Edge e)
        {
            if (e.face != e.next.face)
            {
                return false;
            }
            return true;
        }

        /// sanity-check for edge
        public bool check_edge(Edge e)
        {
            Vertex src = e.source;
            Vertex trg = e.target;
            Edge twine = e.twin;

            if (twine == null)
            {
                return true;
            }
            else if (!(e == twine.twin))
            {
                return false;
            }

            Vertex tw_src = twine.source;
            Vertex tw_trg = twine.target;
            return ((src == tw_trg) && (trg == tw_src));
        }

    }
}
