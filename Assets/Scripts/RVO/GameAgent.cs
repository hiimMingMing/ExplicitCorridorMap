using System;
using System.Collections;
using System.Collections.Generic;
using RVO;
using UnityEngine;
using Random = System.Random;
using Vector2 = RVO.Vector2;
using ExplicitCorridorMap;
public class GameAgent : MonoBehaviour
{
    [HideInInspector]
    public int sid = -1;
    public ECMMap ecmMap;
    ECM ecm;
    /** Random number generator. */
    private Random m_random = new Random();
    LineRenderer lineRenderer;
    [Header("Pathfinding attribute")]
    public UnityEngine.Vector2 targetWayPoint;
    public Vector3 finalTarget;
    public int currentWayPoint = 0;
    public Vector3 debugTarget = Vector3.zero;
    public float destinationRadius { get; set; }
    public List<UnityEngine.Vector2> wayPointList = new List<UnityEngine.Vector2>();
    List<LineDrawer> lineList = new List<LineDrawer>();
    public int radiusIndex;
    // Use this for initialization
    void Start() {
        ecm = ecmMap.ecm;
        float rad = Simulator.Instance.getAgentRadius(sid);
        lineRenderer = GetComponent<LineRenderer>();

        radiusIndex = ecmMap.AgentRadiusList.FindIndex(x=>(x==rad));
        
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
            Vector2 vel = Simulator.Instance.getAgentVelocity(sid);
            if (GameMainManager.Instance.is3D)
            {
                transform.position = new Vector3(pos.x(), pos.y(), 0).to3D();
            }
            else {
                transform.position = new Vector3(pos.x(), pos.y(), transform.position.z);
            }
            if (Math.Abs(vel.x()) > 0.01f && Math.Abs(vel.y()) > 0.01f)
                transform.forward = new Vector3(vel.x(), vel.y(), 0).to3D().normalized;
        }

        #region Debug
        if (GameMainManager.Instance.agentSetting.debugMode)
        {
            if (targetWayPoint.x != 0 && targetWayPoint.y != 0)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, targetWayPoint.to3D());
                lineRenderer.startWidth = 2.0f;
                lineRenderer.endWidth = 2.0f;
            }
        }
        #endregion
        if (Input.GetMouseButtonDown(1))
        {
            DeletePath();
            if (debugTarget != Vector3.zero) {
                finalTarget = debugTarget;
            }
            else
            {
                if (GameMainManager.Instance.is3D)
                {
                    Vector2 mousePosition = GameMainManager.Instance.mousePosition;
                    finalTarget = new Vector3(mousePosition.x_, mousePosition.y_, 0);
                }
                else {
                    finalTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
               
            }
            
            if (GameMainManager.Instance.is3D)
            {
                wayPointList = ExplicitCorridorMap.PathFinding.FindPath(ecm, radiusIndex, transform.position.to2D(), finalTarget);
            }
            else {
                wayPointList = ExplicitCorridorMap.PathFinding.FindPath(ecm,radiusIndex ,transform.position, finalTarget);
            }

            DrawPath(Color.yellow);
            currentWayPoint = 1;
        }
        //check if destination stuck

        if (currentWayPoint == wayPointList.Count) {
            Debug.Log("check");
            if (checkDestinationStuck()) {
                // immediately stop
                currentWayPoint = wayPointList.Count;
                Simulator.Instance.setAgentVelocity(sid, new Vector2(0, 0));

            }
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
            

            Vector2 goalVector = new Vector2(targetWayPoint.x, targetWayPoint.y) - Simulator.Instance.getAgentPosition(sid);
            
           
           
            goalVector = RVOMath.normalize(goalVector);
          

            Simulator.Instance.setAgentPrefVelocity(sid, goalVector);

            /* Perturb a little to avoid deadlocks due to perfect symmetry. */
            float angle = (float)m_random.NextDouble() * 2.0f * (float)Math.PI;
            float dist = (float)m_random.NextDouble() * 0.0001f;

            Simulator.Instance.setAgentPrefVelocity(sid, Simulator.Instance.getAgentPrefVelocity(sid) +
                                                         dist *
                                                         new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));

            Vector2 position;
            if (GameMainManager.Instance.is3D)
            {
                position = new Vector2(transform.position.x, transform.position.z);
            }
            else {
                position = new Vector2(transform.position.x, transform.position.y);
            }
            if (RVOMath.compareVector2WithinDist(position, new Vector2(targetWayPoint.x, targetWayPoint.y), destinationRadius))
            {
                Debug.Log("Continue to next waypoint!");
                currentWayPoint++;
                if (currentWayPoint < wayPointList.Count) targetWayPoint = wayPointList[currentWayPoint];
                else DeletePath();
            }
        }


        
    }


    bool checkDestinationStuck() {
        float distanceToDesSqr;
        if (GameMainManager.Instance.is3D)
        {
            distanceToDesSqr = (transform.position.to2D().Vector3ToVector2() - finalTarget.Vector3ToVector2()).sqrMagnitude;
        }
        else {
            distanceToDesSqr = (transform.position.Vector3ToVector2() - finalTarget.Vector3ToVector2()).sqrMagnitude;

        }
        float sumAgentS = 0;
        IList<Agent> listOfAgent = Simulator.Instance.GetListAgents();
        for (int i = 0; i < listOfAgent.Count; i++)
        {
            if ((listOfAgent[i].position_.RVOVector2ToVector2() - finalTarget.Vector3ToVector2()).sqrMagnitude < distanceToDesSqr)
            {
                sumAgentS += listOfAgent[i].radius_ * listOfAgent[i].radius_ * 3.14f;
            }
        }

        if (sumAgentS >= 3.14f * distanceToDesSqr * 0.1f) {
            Debug.Log("Stuck");
            return true;
        }
           
        return false;

    }
    #region Debug
    public void DrawPath(Color color)
    {
        if (GameMainManager.Instance.agentSetting.debugMode)
        {
            for (int i = 0; i < wayPointList.Count - 1; i++)
            {
                //Draw the path
                LineDrawer lineDrawer = new LineDrawer();
                var start = new Vector3(wayPointList[i].x, wayPointList[i].y).to3D();
                var end = new Vector3(wayPointList[i + 1].x, wayPointList[i + 1].y).to3D();
                lineDrawer.DrawLineInGameView(start, end, color, 0.5f);
                lineList.Add(lineDrawer);
            }
        }
    }

    void DeletePath()
    {
        if (GameMainManager.Instance.agentSetting.debugMode)
        {
            foreach (var l in lineList)
                l.Destroy();
        }
    }
    #endregion
}