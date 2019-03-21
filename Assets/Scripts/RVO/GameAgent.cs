using System;
using System.Collections;
using System.Collections.Generic;
using RVO;
using UnityEngine;
using Random = System.Random;
using Vector2 = RVO.Vector2;

public class GameAgent : MonoBehaviour
{
    
    [HideInInspector] public int sid = -1;
    
    /** Random number generator. */
    private Random m_random = new Random();
    UnityEngine.Vector2 targetWayPoint;
    public UnityEngine.Vector2 deafaultWayPoint = UnityEngine.Vector2.zero;
    public int currentWayPoint = 0;
    private List<UnityEngine.Vector2> wayPointList = new List<UnityEngine.Vector2>();
    // Use this for initialization
    void Start() {
        
    }
   
    
    // Update is called once per frame
    void Update()
    {
        if (sid < 0) {
            Debug.Log("SID  " + sid);
            return;
        }
        if (sid >= 0)
        {
            Vector2 pos = Simulator.Instance.getAgentPosition(sid);
            Vector2 vel = Simulator.Instance.getAgentPrefVelocity(sid);
            transform.position = new Vector3(pos.x(), pos.y(), transform.position.z );
            if (Math.Abs(vel.x()) > 0.01f && Math.Abs(vel.y()) > 0.01f)
                transform.forward = new Vector3(vel.x(), vel.y(), 0).normalized;
        }

       
      
        if (Input.GetMouseButtonDown(1))
        {
            var finalTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (deafaultWayPoint != UnityEngine.Vector2.zero)
            {
                finalTarget = deafaultWayPoint;
            }
            //wayPointList = ExplicitCorridorMap.PathFinding.FindPath(GameMainManager.Instance.getECM(), transform.position, finalTarget);
            wayPointList = ExplicitCorridorMap.PathFinding.FindPath(GameMainManager.Instance.getECM(), transform.position,finalTarget);
            Debug.Log("Path found");
            foreach (var v in wayPointList)
            {
                Debug.Log(v);
            }
            currentWayPoint = 1;
        }
        if (currentWayPoint < wayPointList.Count)
        {
            targetWayPoint = wayPointList[currentWayPoint];
            walk();
        }
        else {
            Simulator.Instance.setAgentPrefVelocity(sid, new Vector2(0, 0));
            return;
        }
        void walk()
        {
            // rotate towards the target
            //transform.forward = Vector3.RotateTowards(transform.forward, (Vector3)targetWayPoint - transform.position, speed * Time.deltaTime, 0.0f);

            // move towards the target
           

            Vector2 goalVector = new Vector2(targetWayPoint.x,targetWayPoint.y) - Simulator.Instance.getAgentPosition(sid);
            if (RVOMath.absSq(goalVector) > 1.0f)
            {
                goalVector = RVOMath.normalize(goalVector);
            }

            Simulator.Instance.setAgentPrefVelocity(sid, goalVector);

            /* Perturb a little to avoid deadlocks due to perfect symmetry. */
            float angle = (float)m_random.NextDouble() * 2.0f * (float)Math.PI;
            float dist = (float)m_random.NextDouble() * 0.0001f;

            Simulator.Instance.setAgentPrefVelocity(sid, Simulator.Instance.getAgentPrefVelocity(sid) +
                                                         dist *
                                                         new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));

            
            if (RVOMath.compareVector2WithinDist(new Vector2(transform.position.x, transform.position.y), new Vector2(targetWayPoint.x, targetWayPoint.y), 15.0f))
            {
                Debug.Log("Continue to next waypoint!");
                currentWayPoint++;
                if (currentWayPoint < wayPointList.Count) targetWayPoint = wayPointList[currentWayPoint];
            }
        }


        
    }
}