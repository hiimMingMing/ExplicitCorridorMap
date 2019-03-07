﻿using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 200;
    public Transform cubes;
    // Start is called before the first frame update
    private List<Obstacle> Obstacles = new List<Obstacle>();
    private ECM ecm;
    Vector2 targetWayPoint;
    public int currentWayPoint = 0;

    private List<Vector2> wayPointList = new List<Vector2>();

    void Start()
    {
        foreach (Transform cube in cubes)
        {
            Obstacles.Add(new Obstacle( Geometry.ConvertToRect(cube)));
        }
        ecm = new ECM(Obstacles, new Obstacle(new RectInt(0, 0, 500, 500)));
        ecm.Construct();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            var finalTarget = Camera.main.ScreenToWorldPoint( Input.mousePosition);
            wayPointList = PathFinding.FindPath(ecm, transform.position, finalTarget);
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
        void walk()
        {
            // rotate towards the target
            //transform.forward = Vector3.RotateTowards(transform.forward, (Vector3)targetWayPoint - transform.position, speed * Time.deltaTime, 0.0f);

            // move towards the target
            transform.position = Vector3.MoveTowards(transform.position, targetWayPoint, speed * Time.deltaTime);

            if (transform.position == (Vector3)targetWayPoint)
            {
                currentWayPoint++;
                if(currentWayPoint< wayPointList.Count) targetWayPoint = wayPointList[currentWayPoint];
            }
        }
    }
}
