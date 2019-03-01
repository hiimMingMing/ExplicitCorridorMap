using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
 


namespace _2DMath
{
    class _2DMath
    {
        /// <summary>
        /// Return bisector between l1l2 and r1r2
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        static List<Vector2> CalculateBisector(Vector2 l1, Vector2 l2, Vector2 r1, Vector2 r2) {
            if (l1 == l2 && r1 == r2) {
                return CalculateBisectorBetweenPoint(l1, r1);
            }
            if (l1 == l2 && r1 != r2) {
                return CalculateBisectorBetweenLineAndPoint(r1, r2, l1);
            }
            if (l1 != l2 && r1 == r2) {
                return CalculateBisectorBetweenLineAndPoint(l1, l2, r1);
            }

            return CalculateBisectorBetweenLine(l1, l2, r1, r2);

        }
        /// <summary>
        /// Calculate intersect and check if it inside edge
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>Return intersection if it inside edge , if not return vector2.zore</returns>
        static Vector2 CalculateIntersectInsideEdge(ExplicitCorridorMap.Edge edge,List<Vector2> first, List<Vector2> second) {
            if (first.Count == 2 && second.Count == 2) {
                return CalculateIntersectionBetweenLines(
                    first.ElementAt(0),first.ElementAt(0) + first.ElementAt(1)*10,
                    second.ElementAt(0), second.ElementAt(0) + second.ElementAt(1) * 10
                    );
            }
            if (first.Count == 2 && second.Count == 3) {
                List<Vector2> res =  CalculateIntersectionBetweenLineAndParabola(
                    first.ElementAt(0), first.ElementAt(0) + first.ElementAt(1) * 10,
                    second.ElementAt(2),second.ElementAt(0),second.ElementAt(1)
                    );
                for (int i = 0; i < res.Count; i++)
                {
                    if (ExplicitCorridorMap.Geometry.PolygonContainsPoint(
                        new List<Vector2> { edge.Start.Position, edge.LeftObstacleOfStart, edge.LeftObstacleOfEnd, edge.End.Position, edge.RightObstacleOfEnd, edge.RightObstacleOfStart },
                        res.ElementAt(i)))
                    {
                        return res.ElementAt(i);
                    }
                }
            }
            if (first.Count == 3 && second.Count == 2)
            {
                List<Vector2> res = CalculateIntersectionBetweenLineAndParabola(
                    second.ElementAt(0), second.ElementAt(0) + second.ElementAt(1) * 10,
                    first.ElementAt(2), first.ElementAt(0), first.ElementAt(1)
                    );
                for (int i = 0; i < res.Count; i++)
                {
                    if (ExplicitCorridorMap.Geometry.PolygonContainsPoint(
                        new List<Vector2> { edge.Start.Position, edge.LeftObstacleOfStart, edge.LeftObstacleOfEnd, edge.End.Position, edge.RightObstacleOfEnd, edge.RightObstacleOfStart },
                        res.ElementAt(i)))
                    {
                        return res.ElementAt(i);
                    }
                }
            }

            if (first.Count == 3 && second.Count == 3)
            {
                // I think there is no chance that intersec between parabolal and parabolal
                return Vector2.zero;
            }
            return Vector2.zero;
        }
        /// <summary>
        /// Calculate the perpendicular vector of this vector
        /// </summary>
        /// <param name="val"> </param>
        /// <returns>Return perpendicular vector</returns>
        static Vector2 PerpendicularVector(Vector2 val) {
            if (val.x == 0)
            {
                return new Vector2(1, 0);
            }
            else {
                Vector2 ret = new Vector2(0, 0);
                ret.y = 1;
                ret.x = -val.y / val.x;
                return ret.normalized;
            }
        }
        /// <summary>
        /// Calculate bisector between two points 
        /// </summary>
        /// <returns>A line bisector </returns>
        /// First : D0 , second: n
        static List<Vector2> CalculateBisectorBetweenPoint(Vector2 l1, Vector2 r1)
        {
            Vector2 d0 = (l1 + r1) / 2;
            Vector2 p = (r1 - l1).normalized;
            Vector2 n = PerpendicularVector(p);
            return new List<Vector2> { d0, n };
        }

        /// <summary>
        /// Calculate bisector between two line 
        /// </summary>
        /// <returns>A line bisector </returns>
        /// First : D0 , second: n
        static List<Vector2> CalculateBisectorBetweenLine(Vector2 l1, Vector2 l2, Vector2 r1, Vector2 r2) {
            //TODO calculate bisector

            Vector2 n1 = (l2 - l1).normalized;
            Vector2 n2 = (r2 - r1).normalized;
            // n1 parallel with n2
            if (Mathf.Equals(n1, n2))
            {
                Vector2 d0 = (l1 + l2) / 2;
                Vector2 n = n1;

                return new List<Vector2> { d0, n };
            }
            else {
                float dx, dy;
                Vector2 p1 = PerpendicularVector(n1);
                Vector2 p2 = PerpendicularVector(n2);
                float c1 = -(p1.x * l1.x + p1.y * l1.y);
                float c2 = -(p2.x * r1.x + p2.y * r1.y);
                if (p2.x == 0)
                {
                    dy = -c2 / p2.y;
                    dx = -(c1 + p1.y * dy) / p1.x;
                }
                else {
                    dy = (-c1 * p2.x + p1.x * c2) / (-p1.x * p2.y + p1.y * p2.x);
                    dx = -(c2 + p2.y * dy) / p2.x;
                }

                Vector2 n = (n1 + n2).normalized;
                return new List<Vector2> { new Vector2(dx, dy), n };

            }
        }
        /// <summary>
        /// Calculate bisector between  line and point 
        /// </summary>
        /// <returns>A parabolic bisector </returns>
        static List<Vector2> CalculateBisectorBetweenLineAndPoint(Vector2 start, Vector2 end, Vector2 point)
        {
            return new List<Vector2> { start, end, point };
        }
        /// <summary>
        /// Calculate intersection between two lines and check if its inside polygon
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="start1">s </param>
        /// <param name="end1"> g </param>
        /// <param name="start2">Polygon edge</param>
        /// <param name="end2">Polygon edge</param>
        /// <returns>Point if inside polygon</returns>
        static Vector2 CalculateIntersectionBetweenLinesInsidePolygon(List<Vector2> polygon, Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2) {
            Vector2 result = CalculateIntersectionBetweenLines(start1, end1, start2, end2);
            if (result == Vector2.negativeInfinity)
            {
                return result;
            }
            else {
                bool isInside = ExplicitCorridorMap.Geometry.PolygonContainsPoint(polygon, result);
                if (isInside)
                {
                    //TODO check if this point lie between start and end
                     
                    return result;
                }
                else {
                    return Vector2.negativeInfinity;
                }
            }

        }
        /// <summary>
        /// Calculate intersection between lines
        /// </summary>
        /// <param name="start">The first point</param>
        /// <param name="end">The second point</param>
        /// <returns>Point</returns>
        static Vector2 CalculateIntersectionBetweenLines(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2) {

            Vector2 n1 = (end1 - start1).normalized;
            Vector2 n2 = (end2 - start2).normalized;
            // n1 parallel with n2
            if (Mathf.Equals(n1, n2))
            {
                return Vector2.negativeInfinity;
            }
            else
            {
                float dx, dy;
                Vector2 p1 = PerpendicularVector(n1);
                Vector2 p2 = PerpendicularVector(n2);
                float c1 = -(p1.x * start1.x + p1.y * start1.y);
                float c2 = -(p2.x * start2.x + p2.y * start2.y);
                if (p2.x == 0)
                {
                    dy = -c2 / p2.y;
                    dx = -(c1 + p1.y * dy) / p1.x;
                }
                else
                {
                    dy = (-c1 * p2.x + p1.x * c2) / (-p1.x * p2.y + p1.y * p2.x);
                    dx = -(c2 + p2.y * dy) / p2.x;
                }

                return new Vector2(dx, dy);

            }
        }

        /// <summary>
        /// Calculate intersection between line and parabola
        /// </summary>
        /// <param name="start">The first point</param>
        /// <param name="end">The second point</param>
        /// <param name="focus">The focus of parabola</param>
        /// <param name="directrix"> The directrix of parabola</param>
        /// <returns>The line between two lines</returns>
        static List<Vector2> CalculateIntersectionBetweenLineAndParabola(Vector2 start, Vector2 end, Vector2 focus, Vector2 dir_start, Vector2 dir_end)
        {
            #region Rotate input 
            float shift_X = Mathf.Min(dir_start.x, dir_end.x);
            float shift_Y = Math.Min(dir_start.y, dir_end.y);
            float angle = ExplicitCorridorMap.Maths.ParabolaComputation.GetLineAngleAsRadiant(dir_start, dir_end);
            Vector2 start_rotated = ExplicitCorridorMap.Maths.Rotation.Rotate(
                start,
                angle,
                shift_X,
                shift_Y
                );

            Vector2 end_rotated = ExplicitCorridorMap.Maths.Rotation.Rotate(
                end,
                angle,
                shift_X,
                shift_Y
                );

            Vector2 focus_rotated = ExplicitCorridorMap.Maths.Rotation.Rotate(
                focus,
                angle,
                shift_X,
                shift_Y
                );

            Vector2 dir_startPoint_rotated = ExplicitCorridorMap.Maths.Rotation.Rotate(
                dir_start,
                angle,
                shift_X,
                shift_Y
                );

            Vector2 dir_endPoint_rotated = ExplicitCorridorMap.Maths.Rotation.Rotate(
                dir_end,
                angle,
                shift_X,
                shift_Y
                );
            #endregion
            #region Calculate
            //Line ax+by +c = 0 => y = ax +b 
            //If b = 0, ax+c = 0
            Vector2 inter1 = Vector2.negativeInfinity;
            Vector2 inter2 = Vector2.negativeInfinity;
            Vector2 lineP = (end_rotated - start_rotated).normalized;
            if (lineP.y == 0)
            {
                // x = -c/a
                float x = start_rotated.x;
                float y1 = ExplicitCorridorMap.Maths.ParabolaComputation.ParabolaY(x, focus_rotated, dir_endPoint_rotated.y);
                inter1 = new Vector2(x, y1);

            }
            // b!= 0 y = ax+b
            else {
                //ax+by+c = 0
                float cLine = -(lineP.x * start_rotated.x + lineP.y * start_rotated.y);
                //y = ax+b
                float aLine = -(lineP.x / lineP.y);
                float bLine = -(cLine / lineP.y);
                //Parabolal  y=((x−a)2+b2−c2)/2(b−c)  a = focus.x b = focus.y c = direx.y
                //and line y = ax +b 

                //Solve 2p ax^2 +bx +c
                float a2p = 1 / (2 * (focus_rotated.y - dir_endPoint_rotated.y));
                float b2p = ((-2 * focus_rotated.x) / (2 * ((focus_rotated.y - dir_endPoint_rotated.y)))) - aLine;
                float c2p = ((focus_rotated.x * focus_rotated.x + focus_rotated.y * focus_rotated.y - dir_endPoint_rotated.y * dir_endPoint_rotated.y) / (2 * ((focus_rotated.y - dir_endPoint_rotated.y)))) - bLine;
                // D = -b^2 +4ac

                float D2p = -(b2p * b2p) + 4 * a2p * c2p;
                // D> 0 => x = (-b+-sqrt(D))/2a
                if (D2p > 0)
                {
                    float x1 = (-b2p + Mathf.Sqrt(D2p)) / (2 * a2p);
                    float x2 = (-b2p - Mathf.Sqrt(D2p)) / (2 * a2p);
                    float y1 = (aLine * x1 + bLine);
                    float y2 = (aLine * x2 + bLine);
                    inter1 = new Vector2(x1, y1);
                    inter2 = new Vector2(x2, y2);

                }
                // D = 0 => x = -b/2as
                else if (D2p == 0)
                {
                    float x = -b2p / (2 * a2p);
                    float y = (aLine * x + bLine);
                    inter1 = new Vector2(x, y);
                }
                // D< = => None
                else {
                    //Do nothing
                }
            }
            #endregion

            #region Unrotate

            if (inter1 != Vector2.negativeInfinity) {
                ExplicitCorridorMap.Maths.Rotation.Unrotate(inter1, angle, shift_X, shift_Y);

            }
            if (inter2 != Vector2.negativeInfinity) {
                ExplicitCorridorMap.Maths.Rotation.Unrotate(inter2, angle, shift_X, shift_Y);
            }
            #endregion
            return new List<Vector2> { inter1, inter2 };
        }



        ///<summary
        ///Find the ECM that contain point p
        ///</summary>
        ///<returns> Return a edge's id that contain p, -1 mean no ECM contain that point
        static int PointLocation(ExplicitCorridorMap.ECM ECM, Vector2 p) {
            //Iterate all cell 
            bool isContainPoint = false;
            foreach (var item in ECM.Edges)
            {
                //Ignore twin edge
                if (item.Key % 2 == 0) {
                    ExplicitCorridorMap.Edge edge = item.Value;

                    isContainPoint = ExplicitCorridorMap.Geometry.PolygonContainsPoint(
                        new List<Vector2> {
                            edge.Start.Position,
                            edge.RightObstacleOfStart,
                            edge.RightObstacleOfEnd,
                            edge.End.Position,
                            edge.LeftObstacleOfEnd,
                            edge.LeftObstacleOfStart
                        }, p);
                    if (isContainPoint) {
                        return item.Key;
                    }
                }

            }

            //No cell contain p
            return -1;
        }

        /// <summary>
        /// Check if the left obstacle changes at the bending point p
        /// </summary>
        /// <param name="ECM"></param>
        /// <param name="p">Bending point</param>
        /// <param name="edge">Current edge contain that bending point</param>
        /// <returns></returns>
        static bool IsRelavent(ExplicitCorridorMap.ECM ECM, ExplicitCorridorMap.Vertex p, ExplicitCorridorMap.Edge edge)
        {
            ExplicitCorridorMap.Vertex p2 = edge.Start != p ? edge.Start : edge.End;
            foreach (var item in p.Edges)
            {
                if (item != edge) {
                    if (item.Start == p || item.End == p) {

                        // Point -> Line
                        if (p == p2)
                        {
                            if (item.Start != item.End)
                            {
                                return true;
                            }
                            else {
                                return false;
                            }
                        }
                        // Line -> Point
                        else {
                            if (item.Start == item.End)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }
        ///<summary
        ///Find the next cell of current cell
        ///</summary>
        ///<param name="p1"> Current cell's start</params>
        ///<param name="p2"> Current cell's end </params>
        ///<returns> Return the iD of next cell(EDGE)
        static int NextCell(ExplicitCorridorMap.ECM ECM, ExplicitCorridorMap.Edge edge)
        {
            if (Mathf.Approximately(edge.End.Position.x, edge.LeftObstacleOfEnd.x) && Mathf.Approximately(edge.End.Position.y, edge.LeftObstacleOfEnd.y))
            {
                return -1;
            }
            else {
                ExplicitCorridorMap.Vertex endVertex = edge.End;
                foreach (var item in endVertex.Edges)
                {
                    if (item.LeftObstacleOfStart == edge.LeftObstacleOfEnd) {
                        return item.ID;
                    }
                }

            }
            return -1;
        }

        ///<summary
        ///Find the previous cell of current cell
        ///</summary>
        ///<param name="p1"> Current cell's start</params>
        ///<param name="p2"> Current cell's end </params>
        ///<returns> Return the previous cell
        static int PreviousCell(ExplicitCorridorMap.ECM ECM, ExplicitCorridorMap.Edge edge)
        {

            if (Mathf.Approximately(edge.Start.Position.x, edge.LeftObstacleOfStart.x) && Mathf.Approximately(edge.Start.Position.y, edge.LeftObstacleOfStart.y))
            {
                return -1;
            }
            else
            {
                ExplicitCorridorMap.Vertex endVertex = edge.Start;
                foreach (var item in endVertex.Edges)
                {
                    if (item.LeftObstacleOfEnd == edge.LeftObstacleOfStart)
                    {
                        return item.ID;
                    }
                }

            }
            return -1;
        }
        /// <summary>
        /// Add all element in the list to ECM
        /// </summary>
        /// <param name="ECM"></param>
        /// <param name="listEdge"></param>
        static void addListEdges(ExplicitCorridorMap.ECM ECM, List<ExplicitCorridorMap.Edge> listEdge) {
            foreach (var item in listEdge)
            {
                ECM.Edges.Add(item.ID, item);
                //Add vertices
                if (!ECM.Vertices.ContainsKey(item.Start.ID)) {
                    ECM.Vertices.Add(item.Start.ID, item.Start);
                }

                if (!ECM.Vertices.ContainsKey(item.End.ID)) {
                    ECM.Vertices.Add(item.End.ID, item.End);
                }
                //Add vertices edge
                if (!item.Start.Edges.Contains(item)) {
                    item.Start.Edges.Add(item);
                }

                if (!item.End.Edges.Contains(item))
                {
                    item.End.Edges.Add(item);
                }
            }
            //TODO add a new created edge with one vertice from current ECM to ECM edges
        }
        /// <summary>
        /// Remove all edge inside a new created ECM circle
        /// </summary>
        /// <param name="ECM"></param>
        /// <param name="listEdge"></param>
        static void RemoveEdgeInsideNewCell(ExplicitCorridorMap.ECM ECM, List<ExplicitCorridorMap.Edge> listEdge) {
            List<Vector2> listEdgeToPolygon = new List<Vector2>(listEdge.Count);
            foreach (var item in listEdge)
            {
                listEdgeToPolygon.Add(item.Start.Position);
            }

        }
        /// <summary>
        /// remove all edges inside a polygon
        /// </summary>
        /// <param name="ECM"></param>
        /// <param name="polygon"></param>
        static void RemoveEdgeInsidePolygon(ExplicitCorridorMap.ECM ECM, List<Vector2> polygon) {
            foreach (var item in ECM.Vertices)
            {
                ExplicitCorridorMap.Vertex vertex = item.Value;
                //Remove vertex from ECM vertices
                ECM.Vertices.Remove(vertex.ID);

                //Remove edge create by this vertex
                foreach (var edge in vertex.Edges) { 
                    edge.Start.Edges.Remove(edge);
                    edge.End.Edges.Remove(edge);
                    ECM.Edges.Remove(edge.ID);
                    
                    

                }
            }
        }
        struct TraceNewEdgeResult
        {
            List<Vector2> newBendingPoints;
            ExplicitCorridorMap.Edge edge;
        }
        /// <summary>
        /// Trace new edge
        /// </summary>
        /// <param name="ECM"></param>
        /// <param name="edge">Edge define ECM cell that contain p</param>
        /// <param name="p">New point obstacle</param>
        /// <param name="forward">A flag forward</param>
        /// <param name="iStart">The starting point(Optional)</param>
        static TraceNewEdgeResult TraceNewEdge_Point(ExplicitCorridorMap.ECM ECM,ExplicitCorridorMap.Edge edge, Vector2 p, bool forward, Vector2 iStart) {
            List<ExplicitCorridorMap.Vertex> result = new List<ExplicitCorridorMap.Vertex>();
            if (iStart != null) {
                //TODO check the id of vertice
                ExplicitCorridorMap.Vertex bp = new ExplicitCorridorMap.Vertex(ECM.Vertices.Last().Key+1,iStart.x,iStart.y);
                result.Add(bp);
            }

            while (true) {
                List<Vector2> bNew = CalculateBisector(edge.LeftObstacleOfStart, edge.LeftObstacleOfEnd, p, p);
                List<Vector2> bOld = CalculateBisector(edge.LeftObstacleOfStart, edge.LeftObstacleOfEnd, edge.RightObstacleOfStart, edge.RightObstacleOfEnd);
                #region Check if bNew intersects the exist ECM edge
                Vector2 iEdge = CalculateIntersectInsideEdge(edge,bNew, bOld);
                //Assume Vector2.Zero = null TODO fix it later
                if (iEdge != Vector2.zero) {
                    //TODO fix vertice Id
                    ExplicitCorridorMap.Vertex bp = new ExplicitCorridorMap.Vertex(ECM.Vertices.Last().Key + 1, iEdge.x, iEdge.y);
                    result.Add(bp);

                }
                #endregion  
            }
        }

        /// <summary>
        /// Return the next normal of polygon O
        /// </summary>
        /// <param name="O">Polygon o</param>
        /// <param name="p1">The first vertice</param>
        /// <param name="p2">The second vertice</param>
        /// <param name="forward">The forward flag</param>
        /// <returns>Return the hafl line start at o at direction dir</returns>
        static Tuple<Vector2, Vector2> NextNormal(List<Vector2> O, Vector2 p1, Vector2 p2, bool forward) {
            Vector2 origin = Vector2.zero;
            Vector2 dir = Vector2.zero;
            Vector2 nextP = Vector2.zero;
            Vector2 preP = Vector2.zero;
            if (p1 == p2)
            {
               
                for (int i = 0; i < O.Count; i++) {
                    if (p1 == O.ElementAt(i)) {
                        if (i == O.Count - 1)
                        {
                            nextP = O.ElementAt(0);
                        }
                        else {
                            nextP = O.ElementAt(i + 1);
                        }

                        if (i == 0)
                        {
                            preP = O.Last();
                        }
                        else {
                            preP = O.ElementAt(i - 1);
                        }
                    }
                }
                origin = p1;
                if (forward)
                {
                    dir = PerpendicularVector(nextP - p1);
                }
                else {
                    dir = PerpendicularVector(preP - p1);
                }
            }
            else {
                dir = PerpendicularVector(p2 - p1);
                if (forward) {
                    origin = p1;
                }
                else {
                    origin = p2;
                }
            }
            return new Tuple<Vector2, Vector2>(origin, dir);
        }
        /// <summary>
        /// Return the next site
        /// </summary>
        /// <param name="O"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="forward"></param>
        /// <returns>Return vertice that determine next site</returns>
        static Tuple<Vector2, Vector2> NextSite(List<Vector2> O, Vector2 p1, Vector2 p2, bool forward) {
            Vector2 p1Next,p2Next;
            p1Next = Vector2.zero;
            p2Next = Vector2.zero;
            if (p1 == p2)
            {
                if (forward)
                {
                    // find the vectex of O after p2(clockwise)
                    for (int i = 0; i < O.Count; i++)
                    {
                        if (O.ElementAt(i) == p2)
                        {
                            if (i == O.Count - 1)
                            {
                                p2Next = O.ElementAt(0);
                            }
                            else
                            {
                                p2Next = O.ElementAt(i + 1);
                            }
                        }
                    }
                    return new Tuple<Vector2, Vector2>(p2, p2Next);
                }
                else
                {
                    //find the vectex of O after p1(counter-clockwise)
                    for (int i = 0; i < O.Count; i++)
                    {
                        if (O.ElementAt(i) == p1)
                        {
                            if (i == 0)
                            {
                                p1Next = O.Last();
                            }
                            else
                            {
                                p1Next = O.ElementAt(i - 1);
                            }
                        }

                    }
                    return new Tuple<Vector2, Vector2>(p1Next, p1);
                }
            }
            else {
                if (forward)
                {
                    return new Tuple<Vector2, Vector2>(p2, p2);
                }
                else {
                    return new Tuple<Vector2, Vector2>(p1, p1);
                }
            }
        }
        /// <summary>
        /// Find the start cell for insert polygon
        /// </summary>
        /// <param name="ECM"></param>
        /// <param name="polygon"></param>
        /// <returns>Return ECM cell that contain p* and n*, and alse return p1,p2 edge of polygon that contain p*</returns>
        static Tuple<ExplicitCorridorMap.Edge,Vector2,Vector2> FindStartCell(ExplicitCorridorMap.ECM ECM, List<Vector2> polygon) {


            //Find the ECM edge that contain start point
            //for all edge
            //For the first edge
            //Calculate distance between its and nearest obstacel, dmin
            //Find the edge of polygon that construct by this edge intersect with start-end line
            //Move to the next ECM edge
            //Calculate distance between its and nearest obstacel, dmin
            //Until travel all the ecm edge that visit by this start-end edge
            //return the dmin
            Vector2 resPoint = Vector2.zero;
            float neareastDistance = float.PositiveInfinity;
            float tempDistance = neareastDistance;
            int EdgeId = PointLocation(ECM, polygon.First());
            ExplicitCorridorMap.Edge currentEdge = null;
            ECM.Edges.TryGetValue(EdgeId,out currentEdge);
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 start = polygon.ElementAt(i);
                Vector2 end = polygon.ElementAt((i + 1) % polygon.Count);
                while (true)
                {
                    //Calculate point coresspond to nearest obstacle in polygon
                    Vector2 pStartLeft = ExplicitCorridorMap.Maths.Distance.GetClosestPointOnLine(start, end, currentEdge.LeftObstacleOfEnd);
                    Vector2 pEndLeft = ExplicitCorridorMap.Maths.Distance.GetClosestPointOnLine(start, end, currentEdge.LeftObstacleOfStart);
                    Vector2 pStartRight = ExplicitCorridorMap.Maths.Distance.GetClosestPointOnLine(start, end, currentEdge.RightObstacleOfStart);
                    Vector2 pEndRight= ExplicitCorridorMap.Maths.Distance.GetClosestPointOnLine(start, end, currentEdge.RightObstacleOfEnd);
                    //Calculate distance
                    float dStartLeft = ExplicitCorridorMap.Maths.Distance.ComputeSquareDistanceBetweenPoints(currentEdge.LeftObstacleOfStart, pStartLeft);
                    float dEndLeft = ExplicitCorridorMap.Maths.Distance.ComputeSquareDistanceBetweenPoints(currentEdge.LeftObstacleOfEnd, pEndLeft);
                    float dStartRight = ExplicitCorridorMap.Maths.Distance.ComputeSquareDistanceBetweenPoints(currentEdge.RightObstacleOfStart, pStartRight);
                    float dEndRight= ExplicitCorridorMap.Maths.Distance.ComputeSquareDistanceBetweenPoints(currentEdge.RightObstacleOfEnd, pEndRight);




                    CalculateIntersectionBetweenLinesInsidePolygon(new List<Vector2> {
                        currentEdge.Start.Position,
                        currentEdge.LeftObstacleOfStart,
                        currentEdge.LeftObstacleOfEnd,
                        currentEdge.End.Position,
                        currentEdge.RightObstacleOfEnd,
                        currentEdge.RightObstacleOfStart},);
                  
                }
           
            }
        }
    }


}
