using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using ExplicitCorridorMap.Maths;
using RVO;
 
using DebugUtils;

public class GameAgent : MonoBehaviour
{
    [HideInInspector] Vector2 TargetWayPoint;
    [HideInInspector] public int CurrentWayPoint;
    [HideInInspector] public List<Vector2> WayPointList;
   
    [HideInInspector] public ECMMap ECMMap;
    private ECM ECMGraph;
    public int RadiusIndex = 0;
    [HideInInspector] public float Radius=5;
    public float Speed = 2;
    public float Priority = 1;
    public float DestinationRadius = 2;
    [HideInInspector]public int Sid;
    [HideInInspector] private float AgentRadiusOffset =0f;
    [HideInInspector] private  Vector2 _position;
    void Start()
    {
        ECMMap = FindObjectOfType<ECMMap>();
        ECMGraph = ECMMap.ECMGraph;
        CurrentWayPoint = 1;
        WayPointList = new List<Vector2>();
        Radius = ECMMap.AgentRadiusList[RadiusIndex];
      
        Sid = Simulator.Instance.addAgent(transform.position.To2D(), Radius+AgentRadiusOffset, Speed, Priority);
        ECMMap.AgentMap.Add(Sid, this);
    }

    // Update is called once per frame
    void Update()
    {
        _position = transform.position.To2D();
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
        var path = PathFinding.FindPath(ECMGraph, RadiusIndex, _position, goalPosition);
        SetNewPath(path);
    }
    public void ReplanPath()
    {
      
        if (CurrentWayPoint >= WayPointList.Count-1) return;
        var goalPosition = WayPointList[WayPointList.Count - 1];
        
        var path = PathFinding.FindPath(ECMGraph, RadiusIndex,_position, goalPosition);
       
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
        Vector2 vel = Simulator.Instance.getAgentVelocity(Sid);
        Vector2 pos = Simulator.Instance.getAgentPosition(Sid);
        transform.position = new Vector3(pos.x, transform.position.y, pos.y);

        if (Mathf.Abs(vel.x) > 0.01f && Mathf.Abs(vel.y) > 0.01f)
            transform.forward = new Vector3(vel.x, 0, vel.y).normalized;
        //follow path
        if (CurrentWayPoint == WayPointList.Count-1)
        {
          
            if (CheckDestinationStuck())
            {
                CurrentWayPoint++;
                Simulator.Instance.setAgentPrefVelocity(Sid, new Vector2(0, 0));
            }
        }
        
		if (CurrentWayPoint < WayPointList.Count)
        {

            TargetWayPoint = WayPointList[CurrentWayPoint];

            var goalVector = TargetWayPoint - Simulator.Instance.getAgentPosition(Sid);
            goalVector = (goalVector).normalized;
            Simulator.Instance.setAgentPrefVelocity(Sid, goalVector);
            /* Perturb a little to avoid deadlocks due to perfect symmetry. */
            
            float angle = Random.value * 2.0f * Mathf.PI;
            float dist = Random.value * 0.01f;
         
            Simulator.Instance.setAgentPrefVelocity(Sid, Simulator.Instance.getAgentPrefVelocity(Sid) +
                                                         dist * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));

            Vector2 position;
            position = transform.position.To2D();

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
            Simulator.Instance.setAgentPrefVelocity(Sid, new  Vector2(0, 0));
            return;
        }

    }
    bool RadiusApproach( Vector2 position)
    {
        if (RVOMath.compareVector2WithinDist(position, new  Vector2(TargetWayPoint.x, TargetWayPoint.y), DestinationRadius))
        {

            CurrentWayPoint++;
            if (CurrentWayPoint < WayPointList.Count) TargetWayPoint = WayPointList[CurrentWayPoint];
            //else DeletePath();
            return true;
        }
        return false;
    }

    // using for waypoints expect the last one
    bool SameSideApproach(Vector2 position)
    {
        var curWayPoint = new  Vector2(TargetWayPoint.x, TargetWayPoint.y);
        var nextWayPoint = new  Vector2(WayPointList[CurrentWayPoint + 1].x, WayPointList[CurrentWayPoint + 1].y);
        var np = nextWayPoint - curWayPoint;
        np = new  Vector2(-np.y, np.x);
        var y = curWayPoint + np;
        var x = curWayPoint;

        float val = ((x.y - y.y) * (position.x - y.x) + (y.x - x.x) * (position.y - y.y)) * ((x.y - y.y) * (nextWayPoint.x - y.x) + (y.x - x.x) * (nextWayPoint.y - y.y));

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
            var x = listOfAgent[i].position;
            var d = (x - FinalTarget).sqrMagnitude;
            if ((listOfAgent[i].position  - FinalTarget).sqrMagnitude <= distanceToDesSqr)
            {
                sumAgentS += listOfAgent[i].radius * listOfAgent[i].radius * 3.14f;
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