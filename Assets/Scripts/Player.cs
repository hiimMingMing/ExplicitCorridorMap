using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public float speed = 200;
    private ECM ecm;
    public ECMMap ecmMap;
    [HideInInspector]
    public int RadiusIndex = 0;
    public float Radius;
    Vector2 targetWayPoint;
    private int currentWayPoint = 0;
    private List<Vector2> wayPointList = new List<Vector2>();

    void Start()
    {
        ecm = ecmMap.ecm;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            var finalTarget = Camera.main.ScreenToWorldPoint( Input.mousePosition);
            wayPointList = PathFinding.FindPath(ecm, RadiusIndex, transform.position, finalTarget);

            currentWayPoint = 1;
        }
        if (currentWayPoint < wayPointList.Count)
        {
            targetWayPoint = wayPointList[currentWayPoint];
            Walk();
        }
    }
    void Walk()
    {
        // move towards the target
        transform.position = Vector3.MoveTowards(transform.position, targetWayPoint, speed * Time.deltaTime);

        if (transform.position == (Vector3)targetWayPoint)
        {
            currentWayPoint++;
            if (currentWayPoint < wayPointList.Count) targetWayPoint = wayPointList[currentWayPoint];
        }
    }
}
