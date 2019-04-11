using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace ExplicitCorridorMap
{
    class GroupHandler
    {
        public List<Player> Agents { get; set; }
        private ECM Ecm;
        public GroupHandler(ECM ecm,List<Player> agents)
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
        public List<Player> Agents { get; set; }
        private ECM Ecm;
        private Vector2 NearestPositionOfStart;
        private Vector2 NearestPositionOfEnd;
        public SubGroup(ECM ecm, Edge edge)
        {
            Agents = new List<Player>();
            this.Ecm = ecm;
            this.Edge = edge;
        }

        public void AddAgent(Player a)
        {
            Agents.Add(a);
        }
        public void FindPath(int radiusIndex, Vector2 endPosition)
        {
            if (Agents.Count == 0) throw new Exception("Subgroup count == 0");

            var path = PathFinding.FindPath(Ecm, radiusIndex, Edge, NearestPositionOfStart, NearestPositionOfEnd, endPosition);
            foreach(var a in Agents)
            {
                a.SetNewPath(path);
            }
        }
        public void ComputeNearestPosition()
        {
            if (Agents.Count == 0) throw new Exception("Subgroup count == 0");
            NearestPositionOfStart = FindNearestPositionOfVertex(Edge.Start);
            NearestPositionOfEnd = FindNearestPositionOfVertex(Edge.End);
        }
        Vector2 FindNearestPositionOfVertex(Vertex v)
        {
            var nearestPosition = Vector2.zero;
            var nearestDistance = float.PositiveInfinity;
            foreach(var a in Agents)
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
        public SurroundingInfomation()
        {

        }
        public void Compute(Vector2 pos, Edge e)
        {

        }
    }
}
