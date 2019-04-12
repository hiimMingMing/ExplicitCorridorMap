using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using ExplicitCorridorMap.Maths;

public class GameAgent : MonoBehaviour
{
    [HideInInspector] public int RadiusIndex = 0;
    [HideInInspector] public float Radius;
    [HideInInspector] Vector2 TargetWayPoint;
    [HideInInspector] public int CurrentWayPoint;
    [HideInInspector] public List<Vector3> WayPointList;
    [HideInInspector] public ECMMap ECMMap;
    private ECM ECMGraph;
    public float Speed = 200;

    void Start()
    {
        ECMMap = FindObjectOfType<ECMMap>();
        ECMGraph = ECMMap.ECMGraph;
        CurrentWayPoint = 1;
        WayPointList = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        //follow path
        if (CurrentWayPoint < WayPointList.Count)
        {
            TargetWayPoint = WayPointList[CurrentWayPoint];
            // move towards the target    
            transform.position = Vector3.MoveTowards(transform.position, TargetWayPoint, Speed * Time.deltaTime);
            if (transform.position.Approximately(TargetWayPoint))
            {
                CurrentWayPoint++;
                if (CurrentWayPoint < WayPointList.Count) TargetWayPoint = WayPointList[CurrentWayPoint];
            }
        }
    }
    public void SetNewPath(List<Vector3> path)
    {
        WayPointList = path;
        CurrentWayPoint = 1;
    }
    public void FindPath(Vector3 goalPosition)
    {
        if (ECMMap.Grouping) throw new System.Exception("Agent cannot self call when ECMMap.Grouping is on, use ECMMap.FindPathGroup instead");
        SetNewPath(ECMMap.FindPathForAgent(this,goalPosition));
    }

}