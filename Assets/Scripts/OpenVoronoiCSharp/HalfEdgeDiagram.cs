using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenVoronoiCSharp.Internals;

namespace OpenVoronoiCSharp
{
    class HalfEdgeDiagram
    {

        public HashSet<Vertex> vertices = new HashSet<Vertex>();
        public HashSet<Edge> edges = new HashSet<Edge>();
        public HashSet<Face> faces = new HashSet<Face>();

        public Vertex add_vertex()
        {
            Vertex v = new Vertex();
            vertices.Add(v);
            return v;
        }
        /// Add a vertex with given properties, return vertex descriptor
        public Vertex add_vertex(Vertex v)
        {
            vertices.Add(v);
            return v;
        }

        /// return number of faces in graph
        public int num_faces()
        {
            return faces.Count;
        }

        /// return number of vertices in graph
        public int num_vertices()
        {
            return vertices.Count;
        }

        /// return number of edges in graph
        public int num_edges()
        {
            return edges.Count;
        }

        /// return number of edges on Face f
        public int num_edges(Face f)
        {
            return face_edges(f).Count;
        }

        /// Add an edge between vertices v1-v2
        public Edge add_edge(Vertex v1, Vertex v2)
        {
            Edge e = new Edge(v1, v2);
            v1.out_edges.Add(e);
            v2.in_edges.Add(e);
            edges.Add(e);
            return e;
        }

        /// return true if v1-v2 edge exists
        public bool has_edge(Vertex v1, Vertex v2)
        {
            foreach (Edge e in v1.out_edges)
            {
                if (e.target == v2)
                {
                    return true;
                }
            }
            return false;
        }

        /// return v1-v2 Edge
        public Edge edge(Vertex v1, Vertex v2)
        {
            foreach (Edge e in v1.out_edges)
            {
                if (e.target == v2)
                {
                    return e;
                }
            }
            throw new Exception("Edge not found in graph!");
        }

        /// clear given vertex. this removes all edges connecting to the vertex.
        public void clear_vertex(Vertex v)
        {
            foreach (Edge e in v.out_edges)
            {
                e.target.in_edges.Remove(e);
                edges.Remove(e);
            }
            v.out_edges.Clear();
            foreach (Edge e in v.in_edges)
            {
                e.source.out_edges.Remove(e);
                edges.Remove(e);
            }
            v.in_edges.Clear();
        }

        /// Remove given vertex. call clear_vertex() before this!
        public void remove_vertex(Vertex v)
        {
            vertices.Remove(v);
        }

        /// Remove given edge
        public void remove_edge(Edge e)
        {
            e.source.out_edges.Remove(e);
            e.target.in_edges.Remove(e);
            edges.Remove(e);
        }

        /// delete a vertex. clear and Remove.
        public void delete_vertex(Vertex v)
        {
            clear_vertex(v);
            remove_vertex(v);
        }

        /// insert Vertex \a v into the middle of Edge \a e
        public void add_vertex_in_edge(Vertex v, Edge e)
        {
            // the vertex v is inserted into the middle of edge e
            // edge e and its twin are replaced by four new edges: e1,e2 and their twins te2,te1
            // before:             face
            //                      e
            // previous-> source  ------> target -> next
            //  tw_next<- tw_trg  <-----  tw_src <- tw_previous
            //                      twin
            //                    twin_face
            //
            // after:               face
            //                    e1   e2
            // previous-> source  -> v -> target -> next
            //  tw_next<- tw_trg  <- v <- tw_src <- tw_previous
            //                    te2  te1
            //                    twin_face
            //

            Edge e_twin = e.twin;
            Debug.Assert(e_twin != null," e_twin != null ");
            Vertex esource = e.source;
            Vertex etarget = e.target;
            Face face = e.face;
            Face twin_face = e_twin.face;
            Edge previous = previous_edge(e);
            Edge twin_previous = previous_edge(e_twin);

            Debug.Assert(previous.face == e.face, " previous.face == e.face ");
            Debug.Assert(twin_previous.face == e_twin.face, " twin_previous.face == e_twin.face ");

            Edge e1 = add_edge(esource, v);
            Edge te2 = add_edge(v, esource);
            e1.twin = te2; te2.twin = e1;

            Edge e2 = add_edge(v, etarget);
            Edge te1 = add_edge(etarget, v);
            e2.twin = te1; te1.twin = e2;

            // next-pointers
            previous.next = e1; e1.next = e2; e2.next = e.next;

            twin_previous.next = te1; te1.next = te2; te2.next = e_twin.next;

            // this copies params, face, k, type
            e1.copyFrom(e);
            e2.copyFrom(e);
            te1.copyFrom(e_twin);
            te2.copyFrom(e_twin);

            // update the faces
            face.edge = e1;
            twin_face.edge = te1;

            // finally, Remove the old edge
            remove_edge(e);
            remove_edge(e_twin);
        }

        /// Add two edges, one from \a v1 to \a v2 and one from \a v2 to \a v1
        public Pair<Edge, Edge> add_twin_edges(Vertex v1, Vertex v2)
        {
            Edge e1 = add_edge(v1, v2);
            Edge e2 = add_edge(v2, v1);
            e1.twin = e2;
            e2.twin = e1;
            return new Pair<Edge, Edge>(e1, e2);
        }

        /// make e1 the twin of e2 (and vice versa)
        public void twin_edges(Edge e1, Edge e2)
        {
            Debug.Assert(e1.target == e2.source, "e1.target == e2.source");
            Debug.Assert(e1.source == e2.target, "e1.source == e2.target");
            e1.twin = e2;
            e2.twin = e1;
        }

        /// Add a face, with given properties
        public Face add_face()
        {
            Face f = new Face();
            faces.Add(f);
            return f;
        }

        /// return all vertices adjecent to given vertex
        public List<Vertex> adjacent_vertices(Vertex v)
        {
            List<Vertex> adj = new List<Vertex>();
            foreach (Edge e in v.out_edges)
            {
                adj.Add(e.target);
            }
            return adj;
        }

        /// return all vertices of given face
        public List<Vertex> face_vertices(Face face)
        {
            List<Vertex> verts = new List<Vertex>();
            Edge startedge = face.edge; // the edge where we start
            Vertex start_target = startedge.target;
            verts.Add(start_target);

            Edge current = startedge.next;
            int count = 0;
            do
            {
                Vertex current_target = current.target;
                verts.Add(current_target);
                Debug.Assert(current.face == current.next.face, "current.face == current.next.face");
                current = current.next;
                if (count > 30000)
                {
                    throw new Exception("count < 30000");
                }
                count++;
            } while (current != startedge);
            return verts;
        }

        /// return edges of face f as a vector
        /// NOTE: it is faster to write a do-while loop in client code than to call this function!
        public List<Edge> face_edges(Face f)
        {
            Edge start_edge = f.edge;
            Edge current_edge = start_edge;
            List<Edge> _out = new List<Edge>();
            do
            {
            _out.Add(current_edge);
                current_edge = current_edge.next;
            } while (current_edge != start_edge);
            return _out;
        }

        /// return the previous edge. traverses all edges in face until previous found.
        public Edge previous_edge(Edge e)
        {
            Edge previous = e.next;
            while (previous.next != e)
            {
                previous = previous.next;
            }
            return previous;
        }

        /// return adjacent faces to the given vertex
        public List<Face> adjacent_faces(Vertex q)
        {
            HashSet<Face> face_set = new HashSet<Face>();
            foreach (Edge e in q.out_edges)
            {
                face_set.Add(e.face);
            }
            return new List<Face>(face_set);
        }

        /// Remove given v1-v2 edge
        public void remove_edge(Vertex v1, Vertex v2)
        {
            remove_edge(edge(v1, v2));
        }

        /// Remove given v1-v2 edge and its twin
        void remove_twin_edges(Vertex v1, Vertex v2)
        {
            Debug.Assert(has_edge(v1, v2), " has_edge(v1,v2) ");
            Debug.Assert(has_edge(v2, v1), " has_edge(v2,v1) ");
            remove_edge(edge(v1, v2));
            remove_edge(edge(v2, v1));
        }

        /// Remove a degree-two Vertex from the middle of an Edge
        // preserve edge-properties (next, face, k)
        public void remove_deg2_vertex(Vertex v)
        {
            //                    face1 e[1]
            //    v1_prev -> v1 -> SPLIT -> v2 -> v2_next
            //    v1_next <- v1 <- SPLIT <- v2 <- v2_prev
            //                  e[0]  face2
            //
            // is replaced with a single edge:
            //                    face1
            //    v1_prev -> v1 ----------> v2 -> v2_next
            //    v1_next <- v1 <---------- v2 <- v2_prev
            //                     face2

            List<Edge> v_edges = v.out_edges;
            Debug.Assert(v_edges.Count == 2, " v_edges.Count == 2");
            Debug.Assert(v_edges[0].source == v && v_edges[1].source == v, " v_edges.[0).source == v && v_edges.[1).source == v ");

            Vertex v1 = v_edges[0].target;
            Vertex v2 = v_edges[1].target;
            Edge v1_next = v_edges[0].next;
            Edge v1_prev = previous_edge(v_edges[0].twin);
            Edge v2_next = v_edges[1].next;
            Edge v2_prev = previous_edge(v_edges[1].twin);
            Face face1 = v_edges[1].face;
            Face face2 = v_edges[0].face;

            Pair<Edge, Edge> twin_edges = add_twin_edges(v1, v2);
            Edge new1 = twin_edges.getFirst();
            Edge new2 = twin_edges.getSecond();
            set_next(new1, v2_next);
            set_next(new2, v1_next);
            set_next(v2_prev, new2);
            set_next(v1_prev, new1);
            face1.edge = new1;
            face2.edge = new2;
            new1.copyFrom(v_edges[1]);
            new2.copyFrom(v_edges[0]);
            remove_twin_edges(v, v1);
            remove_twin_edges(v, v2);
            remove_vertex(v);
        }

        /// set next-pointer of e1 to e2
        public void set_next(Edge e1, Edge e2)
        {
            Debug.Assert(e1.target == e2.source, " e1.target == e2.source ");
            e1.next = e2;
        }

        /// form a face from the edge-list:
        /// e1->e2->...->e1
        /// for all edges, set edge.face=f, and edge.k=k
        public void set_next_cycle(List<Edge> list, Face f, double k)
        {
            f.edge = list[0];
            for (int q = 0; q < list.Count; q++)
            {
                Edge e = list[q];
                e.face = f;
                e.k = k;
                if (q == list.Count - 1)
                {
                    set_next(e, list[0]);
                }
                else
                {
                    set_next(e, list[q + 1]);
                }
            }
        }

        /// set next-pointers for the given list (but don't close to form a cycle)
        // also set face and k properties for edge
        public void set_next_chain(List<Edge> list, Face f, double k)
        {
            f.edge = list[0];
            for (int q = 0; q < list.Count; q++)
            {
                Edge e = list[q];
                e.face = f;
                e.k = k;
                if (q != list.Count - 1)
                {
                    set_next(e, list[q + 1]);
                }
            }
        }

        /// set next-pointers for the list
        public void set_next_chain(List<Edge> list)
        {
            for (int q = 0; q < list.Count; q++)
            {
                Edge e = list[q];
                if (q != list.Count - 1)
                {
                    set_next(e, list[q + 1]);
                }
            }
        }

        /// on a face, search and return the left/right edge from endp
        public Pair<Edge, Edge> find_next_prev(Face f, Vertex endp)
        {
            Edge current = f.edge;
            Edge start_edge = current;
            Edge next_edge = null;
            Edge prev_edge = null;
            do
            {
                Vertex src = current.source;
                Vertex trg = current.target;
                if (src == endp)
                    next_edge = current;
                if (trg == endp)
                    prev_edge = current;
                current = current.next;
            } while (current != start_edge);
            Debug.Assert(next_edge != null, " next_edge != null ");
            Debug.Assert(prev_edge != null, " prev_edge != null ");
            return new Pair<Edge, Edge>(next_edge, prev_edge);
        }
    }
}
