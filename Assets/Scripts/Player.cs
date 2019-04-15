﻿using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using ExplicitCorridorMap.Maths;
public class Player : MonoBehaviour
{
    public float speed = 50;
    [HideInInspector]
    public ECM ecm;
    public ECMMap ecmMap;
    [HideInInspector]
    public int RadiusIndex = 0;
    [HideInInspector]
    public float Radius;
    Vector2 targetWayPoint;
    [HideInInspector]
    public int currentWayPoint = 0;
    [HideInInspector]
    public List<Vector2> wayPointList = new List<Vector2>();

    [HideInInspector]
    public Vector3 finalTarget;

    //Draw the path
    public bool drawPath = true;
    //LineDrawer lineDrawer;
    [HideInInspector]
    public List<LineDrawer> lineList = new List<LineDrawer>();
    [HideInInspector]
    public bool check = false;

    //For adding obstacles
    public Transform addObj;
    [HideInInspector]
    public RectInt obstacle;

    void Start()
    {
        ecm = ecmMap.ecm;     
    }

    // Update is called once per frame
    void Update()
    {
        if (!ecmMap.grouping)
        {
            if (Input.GetMouseButtonDown(0))
            {
                finalTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SetNewPath( PathFinding.FindPath(ecm, RadiusIndex, transform.position, finalTarget));

                DrawPath(Color.magenta); //Debug
            }
            if (Input.GetMouseButtonDown(1))
            {
                var test = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log(test.x + " - " + test.y);
            }
        }

        if (currentWayPoint < wayPointList.Count)
        {
            targetWayPoint = wayPointList[currentWayPoint];
            Walk();
        }          
    }
    public void SetNewPath(List<Vector2> path)
    {
        wayPointList = path;
        currentWayPoint = 1;
    }
    void Walk()
    {   
        //DynamicReplanning.HandleDynamicEvent(this);

        // move towards the target    
        transform.position = Vector3.MoveTowards(transform.position, targetWayPoint, speed * Time.deltaTime);
        if (transform.position.Approximately(targetWayPoint))
        {
            currentWayPoint++;
            if (currentWayPoint < wayPointList.Count) targetWayPoint = wayPointList[currentWayPoint];
            else
                if (drawPath) LineDrawer.DeletePath(lineList); //Debug
        }
    }

    #region Debug
    public void DrawPath(Color color)
    {
        if (drawPath)
        {
            for (int i = 0; i < wayPointList.Count - 1; i++)
            {
                //Draw the path
                LineDrawer lineDrawer = new LineDrawer();
                var start = new Vector3(wayPointList[i].x, wayPointList[i].y);
                var end = new Vector3(wayPointList[i + 1].x, wayPointList[i + 1].y);
                lineDrawer.DrawLineInGameView(start, end, color, 2.0f);
                lineList.Add(lineDrawer);
            }
        }
    }

    void DeletePath()
    {
        if (drawPath)
        {
            foreach (var l in lineList)
                l.Destroy();
        }
    }
    #endregion
}
