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
        

        public void FindPath(Vector2 target, bool groupBehavior)
        {
            if (Agents.Count == 0) return;
            var subgroupDict = new Dictionary<Edge, SubGroup>();
            int radiusIndex = Agents[0].RadiusIndex;
            foreach(var a in Agents)
            {
                if (a.RadiusIndex != radiusIndex) throw new Exception("All agents in group must have same radius");
                var e = Ecm.GetNearestEdge(a.GetPosition2D());
                if (e == null) throw new NullReferenceException("Group Edge is null");
                if (subgroupDict.ContainsKey(e))
                {
                    subgroupDict[e].AddAgent(a);
                }
                else
                {
                    SubGroup subGroup;
                    if (groupBehavior) subGroup = new SubGroupWithBehavior(Ecm, e);
                    else subGroup = new SubGroupWithoutBehavior(Ecm, e);
                    subGroup.AddAgent(a);
                    subgroupDict[e] = subGroup;
                }
            }

            //Debug.Log("Number of group " + subgroupDict.Values.Count);
            foreach (var sg in subgroupDict.Values)
            {
                //Debug.Log("E " + sg.Edge+" "+sg.Agents.Count);
                sg.FindPath(radiusIndex, target);
            }
        }
    }

    abstract class SubGroup
    {
        public Edge Edge;
        protected ECM Ecm;
        protected Vector2 NearestPositionOfStart;
        protected Vector2 NearestPositionOfEnd;

        protected SubGroup(ECM ecm, Edge edge)
        {
            Edge = edge;
            Ecm = ecm;
        }

        public abstract void AddAgent(GameAgent a);
        public virtual void FindPath(int radiusIndex, Vector2 endPosition)
        {
            //if (AgentDictionary.Count == 0) throw new Exception("Subgroup count == 0");
            ComputeNearestPosition();
            var pathPortals = PathFinding.FindPath(Ecm, radiusIndex, Edge, NearestPositionOfStart, NearestPositionOfEnd, ref endPosition, out Vertex choosenVertex);
            //reverse edge to connect to path
            if (choosenVertex == Edge.Start) Edge = Edge.Twin;

            FindPathForGroup(pathPortals);
        }
        protected abstract void FindPathForGroup(List<Portal> pathPortals);
        protected float Theta(float portalLength, float width)
        {
            if (Mathf.Approximately(width, 0.0f)) return 1;
            else return Mathf.Min(1, portalLength / width);
        }
        protected void ComputeNearestPosition()
        {
            NearestPositionOfStart = FindNearestPositionOfVertex(Edge.Start);
            NearestPositionOfEnd = FindNearestPositionOfVertex(Edge.End);
        }
        protected abstract Vector2 FindNearestPositionOfVertex(Vertex v);
        protected float SquareDistance(Vector2 a, Vector2 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }
    }
    class SubGroupWithoutBehavior : SubGroup
    {
        List<GameAgent> Agents;
        public SubGroupWithoutBehavior(ECM ecm, Edge edge): base(ecm,edge)
        {
            Agents = new List<GameAgent>();
        }
        public override void AddAgent(GameAgent a)
        {
            Agents.Add(a);
        }

        protected override void FindPathForGroup(List<Portal> pathPortals)
        {
            //set new path for each agent
            foreach (var a in Agents)
            {
                pathPortals[0] = new Portal(a.GetPosition2D());
                var path = PathFinding.GetShortestPath(pathPortals).ConvertAll(x => x.Point);
                a.SetNewPath(path);
            }
        }

        protected override Vector2 FindNearestPositionOfVertex(Vertex v)
        {
            var nearestPosition = Vector2.zero;
            var nearestDistance = float.PositiveInfinity;
            foreach (var a in Agents)
            {
                float d = SquareDistance(a.GetPosition2D(), v.Position);
                if (d < nearestDistance)
                {
                    nearestPosition = a.GetPosition2D();
                    nearestDistance = d;
                }
            }
            return nearestPosition;
        }
    }
    class SubGroupWithBehavior : SubGroup
    {
        public Dictionary<GameAgent,SurroundingInfomation> AgentDictionary { get; set; }

        public SubGroupWithBehavior(ECM ecm, Edge edge) : base(ecm, edge)
        {
            AgentDictionary = new Dictionary<GameAgent, SurroundingInfomation>();
        }
        public override void AddAgent(GameAgent a)
        {
            AgentDictionary.Add(a, new SurroundingInfomation());
        }

        protected override void FindPathForGroup(List<Portal> pathPortals)
        {
            //compute distance infos
            foreach (var kv in AgentDictionary)
            {
                var a = kv.Key;
                var si = kv.Value;
                si.Compute(Ecm, Edge, a.GetPosition2D());
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

            //set new path for each agent
            foreach (var kv in AgentDictionary)
            {
                var a = kv.Key;
                var si = kv.Value;
                var path = new List<Vector2>();
                pathPortals[0] = new Portal(a.GetPosition2D());
                var shortestPathPortal = PathFinding.GetShortestPath(pathPortals);
                shortestPathPortal.ForEach(x => x.ComputeVector());
                foreach (var p in shortestPathPortal)
                {
                    if (p.IsLeft)
                    {
                        var d = si.LeftDistance - leftNearestDistance;
                        var theta = Theta(p.Length, widthLeft);
                        var newPoint = p.Point + d * theta * p.LeftToRight;
                        path.Add(newPoint);
                    }
                    else
                    {
                        var d = si.RightDistance - rightNearestDistance;
                        var theta = Theta(p.Length, widthRight);
                        var newPoint = p.Point + d * theta * p.RightToLeft;
                        path.Add(newPoint);
                    }
                }
                a.SetNewPath(path);
            }
        }
        protected override Vector2 FindNearestPositionOfVertex(Vertex v)
        {
            var nearestPosition = Vector2.zero;
            var nearestDistance = float.PositiveInfinity;
            
            foreach(var a in AgentDictionary.Keys)
            {
                float d = SquareDistance(a.GetPosition2D(), v.Position);
                if(d < nearestDistance)
                {
                    nearestPosition = a.GetPosition2D();
                    nearestDistance = d;
                }
            }
            return nearestPosition;
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
