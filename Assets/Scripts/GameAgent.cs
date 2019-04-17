using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using ExplicitCorridorMap.Maths;
using RVO;
using Vector2 = UnityEngine.Vector2;
using DebugUtils;

public class GameAgent : MonoBehaviour
{
    [HideInInspector] Vector2 TargetWayPoint;
    [HideInInspector] public int CurrentWayPoint;
    [HideInInspector] public List<Vector2> WayPointList;
   
    [HideInInspector] public ECMMap ECMMap;
    private ECM ECMGraph;
    [HideInInspector] public int RadiusIndex = 0;
    [HideInInspector] public float Radius;
    public float Speed = 2;
    public float Priority = 1;
    public float DestinationRadius = 5;
    [HideInInspector]public int Sid;

    void Start()
    {
        ECMMap = FindObjectOfType<ECMMap>();
        ECMGraph = ECMMap.ECMGraph;
        CurrentWayPoint = 1;
        WayPointList = new List<Vector2>();

        Sid = Simulator.Instance.addAgent(transform.position.To2DRVO(), Radius, Speed, Priority);
        ECMMap.AgentMap.Add(Sid, this);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
    }
    public void SetNewPath(List<Vector2> path)
    {
        WayPointList = path;
        CurrentWayPoint = 1;
    }
    public void FindPath(Vector2 goalPosition)
    {
        
        //if (ECMMap.Grouping) throw new System.Exception("Agent cannot self call when ECMMap.Grouping is on, use ECMMap.FindPathGroup instead");
        var path = PathFinding.FindPath(ECMGraph, RadiusIndex, GetPosition2D(), ref goalPosition);
        SetNewPath(path);
    }

    public Vector2 GetPosition2D()
    {
        return transform.position.To2D();
    }
    public void SetPosition(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, transform.position.y, pos.y);
    }
    void UpdateMovement()
    {
        RVO.Vector2 vel = Simulator.Instance.getAgentVelocity(Sid);
        RVO.Vector2 pos = Simulator.Instance.getAgentPosition(Sid);
        transform.position = new Vector3(pos.x(), transform.position.y, pos.y());

        if (Mathf.Abs(vel.x()) > 0.01f && Mathf.Abs(vel.y()) > 0.01f)
            transform.forward = new Vector3(vel.x(), 0, vel.y()).normalized;
        //follow path
        if (CurrentWayPoint == WayPointList.Count-1)
        {
          
            if (CheckDestinationStuck())
            {
                CurrentWayPoint++;
                Simulator.Instance.setAgentPrefVelocity(Sid, new RVO.Vector2(0, 0));
            }
        }
        
		if (CurrentWayPoint < WayPointList.Count)
        {

            TargetWayPoint = WayPointList[CurrentWayPoint];

            var goalVector = TargetWayPoint.To2DRVO() - Simulator.Instance.getAgentPosition(Sid);
            goalVector = RVOMath.normalize(goalVector);
            Simulator.Instance.setAgentPrefVelocity(Sid, goalVector);
            /* Perturb a little to avoid deadlocks due to perfect symmetry. */
            float angle = Random.value * 2.0f * Mathf.PI;
            float dist = Random.value * 0.0001f;

            Simulator.Instance.setAgentPrefVelocity(Sid, Simulator.Instance.getAgentPrefVelocity(Sid) +
                                                         dist * new RVO.Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));

            RVO.Vector2 position;
            position = transform.position.To2DRVO();

            //check pass waypoint
            if (CurrentWayPoint < WayPointList.Count - 1)
            {
                if (!SameSideApproach(position))
                {
                    RadiusApproach(position);
                }
            }
            else
            {
                RadiusApproach(position);
            }
        }
        else
        {
            Simulator.Instance.setAgentPrefVelocity(Sid, new RVO.Vector2(0, 0));
            return;
        }

    }
    bool RadiusApproach(RVO.Vector2 position)
    {
        if (RVOMath.compareVector2WithinDist(position, new RVO.Vector2(TargetWayPoint.x, TargetWayPoint.y), DestinationRadius))
        {

            CurrentWayPoint++;
            if (CurrentWayPoint < WayPointList.Count) TargetWayPoint = WayPointList[CurrentWayPoint];
            //else DeletePath();
            return true;
        }
        return false;
    }

    // using for waypoints expect the last one
    bool SameSideApproach(RVO.Vector2 position)
    {
        var curWayPoint = new RVO.Vector2(TargetWayPoint.x, TargetWayPoint.y);
        var nextWayPoint = new RVO.Vector2(WayPointList[CurrentWayPoint + 1].x, WayPointList[CurrentWayPoint + 1].y);
        var np = nextWayPoint - curWayPoint;
        np = new RVO.Vector2(-np.y_, np.x_);
        var y = curWayPoint + np;
        var x = curWayPoint;

        float val = ((x.y_ - y.y_) * (position.x_ - y.x_) + (y.x_ - x.x_) * (position.y_ - y.y_)) * ((x.y_ - y.y_) * (nextWayPoint.x_ - y.x_) + (y.x_ - x.x_) * (nextWayPoint.y_ - y.y_));

        if (val > 0)
        {

            CurrentWayPoint++;
            if (CurrentWayPoint < WayPointList.Count) TargetWayPoint = WayPointList[CurrentWayPoint];
            //else DeletePath();
            return true;
        }

        return false;

    }

    bool CheckDestinationStuck()
    {

        Vector2 FinalTarget = WayPointList[WayPointList.Count - 1];
        float distanceToDesSqr = (transform.position.To2D() - FinalTarget).sqrMagnitude;
        
        float sumAgentS = 0;
        IList<Agent> listOfAgent = Simulator.Instance.GetListAgents();
        for (int i = 0; i < listOfAgent.Count; i++)
        {
            var x = listOfAgent[i].position_.RVOVector2ToVector2();
            var d = (x - FinalTarget).sqrMagnitude;
            if ((listOfAgent[i].position_.RVOVector2ToVector2() - FinalTarget).sqrMagnitude <= distanceToDesSqr)
            {
                sumAgentS += listOfAgent[i].radius_ * listOfAgent[i].radius_ * 3.14f;
            }
        }

        if (sumAgentS >= 3.14f * distanceToDesSqr * 0.6f)
        {
           
            return true;
        }

        return false;

    }
#if UNITY_EDITOR
    #region Draw
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        DrawUtils.DrawPolyLine(WayPointList);
    }
    #endregion

#endif
}