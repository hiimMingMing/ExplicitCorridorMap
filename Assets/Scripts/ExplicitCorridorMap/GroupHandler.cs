using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ExplicitCorridorMap.Maths;
namespace ExplicitCorridorMap
{
    class GroupHandler
    {
        public List<GameAgent> Agents { get; set; }
        private ECM Ecm;
        public GroupHandler(ECM ecm,List<GameAgent> agents)
        {
            Agents = agents;
            this.Ecm = ecm;
        }
        

        public void FindPath(Vector2 target)
        {
            if (Agents.Count == 0) return;
            var subgroupDict = new Dictionary<Edge, SubGroup>();
            int radiusIndex = Agents[0].RadiusIndex;
            foreach(var a in Agents)
            {
                if (a.RadiusIndex != radiusIndex) throw new Exception("All agents in group must have same radius");
                var e = Ecm.GetNearestEdge(a.transform.position);
                if (e == null) throw new NullReferenceException("Group Edge is null");
                if (subgroupDict.ContainsKey(e))
                {
                    subgroupDict[e].AddAgent(a);
                }
                else
                {
                    var subGroup = new SubGroup(Ecm,e);
                    subGroup.AddAgent(a);
                    subgroupDict[e] = subGroup;
                }
            }

            Debug.Log("Number of group " + subgroupDict.Values.Count);
            foreach (var sg in subgroupDict.Values)
            {
                //Debug.Log("E " + sg.Edge+" "+sg.Agents.Count);
                sg.ComputeNearestPosition();
                sg.FindPath(radiusIndex, target);
            }
        }
    }

    class SubGroup
    {
        public Edge Edge;
        public Dictionary<GameAgent,SurroundingInfomation> AgentDictionary { get; set; }
        private ECM Ecm;
        private Vector2 NearestPositionOfStart;
        private Vector2 NearestPositionOfEnd;
        public SubGroup(ECM ecm, Edge edge)
        {
            AgentDictionary = new Dictionary<GameAgent, SurroundingInfomation>();
            this.Ecm = ecm;
            this.Edge = edge;
        }

        public void AddAgent(GameAgent a)
        {
            AgentDictionary.Add(a,new SurroundingInfomation());
        }
        public void FindPath(int radiusIndex, Vector2 endPosition)
        {
            if (AgentDictionary.Count == 0) throw new Exception("Subgroup count == 0");

            var pathPortals = PathFinding2D.FindPath(Ecm, radiusIndex, Edge, NearestPositionOfStart, NearestPositionOfEnd,ref endPosition, out Vertex choosenVertex);
            //reverse edge to connect to path
            if (choosenVertex == Edge.Start) Edge = Edge.Twin;

            //compute distance infos
            foreach (var kv in AgentDictionary)
            {
                var a = kv.Key;
                var si = kv.Value;
                si.Compute(Ecm, Edge, a.transform.position);
                //Debug.Log("Agent "+a.transform.position +" L:"+ si.LeftDistance+" R:"+ si.RightDistance);
            }

            //find agent nearest to the left and the right of Edge
            var leftNearestDistance = float.PositiveInfinity;
            var rightNearestDistance = float.PositiveInfinity;
            var leftFarthestDistance = float.NegativeInfinity;
            var rightFarthestDistance = float.NegativeInfinity;
            foreach (var kv in AgentDictionary)
            {
                var a = kv.Key;
                var si = kv.Value;
                if (si.LeftDistance < leftNearestDistance)
                {
                    leftNearestDistance = si.LeftDistance;
                }
                if (si.LeftDistance > leftFarthestDistance)
                {
                    leftFarthestDistance = si.LeftDistance;
                }
                if (si.RightDistance < rightNearestDistance)
                {
                    rightNearestDistance = si.RightDistance;
                }
                if (si.RightDistance > rightFarthestDistance)
                {
                    rightFarthestDistance = si.RightDistance;
                }
            }
            var widthLeft = leftFarthestDistance - leftNearestDistance;
            var widthRight = rightFarthestDistance - rightNearestDistance;
            foreach (var kv in AgentDictionary)
            {
                var a = kv.Key;
                var si = kv.Value;
                var path = new List<Vector3>();
                pathPortals[0] = new Portal(a.transform.position);
                var shortestPathPortal = PathFinding2D.GetShortestPath(pathPortals);
                shortestPathPortal.ForEach(x => x.ComputeVector());
                foreach (var p in shortestPathPortal)
                {
                    if (p.IsLeft)
                    {
                        var d = si.LeftDistance - leftNearestDistance;
                        var phi = ComputePhi(p.Length, widthLeft);
                        var newPoint = p.Point + d * phi * p.LeftToRight;
                        path.Add(newPoint);
                    }
                    else
                    {
                        var d = si.RightDistance - rightNearestDistance;
                        var phi = ComputePhi(p.Length, widthRight);
                        var newPoint = p.Point + d * phi * p.RightToLeft;
                        path.Add(newPoint);
                    }
                }
                a.SetNewPath(path);
            }
        }
        public float ComputePhi(float portalLength, float width)
        {
            if (Mathf.Approximately(width, 0.0f)) return 1;
            else return Mathf.Min(1, portalLength / width);
        }
        public void ComputeNearestPosition()
        {
            if (AgentDictionary.Count == 0) throw new Exception("Subgroup count == 0");
            NearestPositionOfStart = FindNearestPositionOfVertex(Edge.Start);
            NearestPositionOfEnd = FindNearestPositionOfVertex(Edge.End);
        }
        Vector2 FindNearestPositionOfVertex(Vertex v)
        {
            var nearestPosition = Vector2.zero;
            var nearestDistance = float.PositiveInfinity;
            foreach(var a in AgentDictionary.Keys)
            {
                float d = SquareDistance(a.transform.position, v.Position);
                if(d < nearestDistance)
                {
                    nearestPosition = a.transform.position;
                    nearestDistance = d;
                }
            }
            return nearestPosition;
        }
        float SquareDistance(Vector2 a, Vector2 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }
    }
    class SurroundingInfomation
    {
        public float LeftDistance;
        public float RightDistance;
        public SurroundingInfomation() { }
        public void Compute(ECM ecm, Edge e, Vector2 pos)
        {
            LeftDistance = ComputeDistance(ecm, e, pos);
            RightDistance = ComputeDistance(ecm, e.Twin, pos);
        }
        protected float ComputeDistance(ECM ecm, Edge cell, Vector2 position)
        {
            if (cell.ContainsPoint)
            {
                var pointSite = ecm.RetrieveInputPoint(cell);
                return (pointSite - position).magnitude;
            }
            else
            {
                var lineSite = ecm.RetrieveInputSegment(cell);
                var startLineSite = new Vector2(lineSite.Start.x, lineSite.Start.y);
                var endLineSite = new Vector2(lineSite.End.x, lineSite.End.y);
                var nearestPointOfStartVertex = Distance.GetClosestPointOnLine(startLineSite, endLineSite, position, out float distance);
                return distance;
            }
        }
    }
    
}
