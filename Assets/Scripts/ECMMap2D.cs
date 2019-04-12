using ExplicitCorridorMap;
using ExplicitCorridorMap.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ECMMap2D : ECMMap
{
    protected override void InitObstacleList()
    {
        Obstacles = new List<Obstacle>();
        foreach (Transform obs in ObstaclesTransform)
        {
            Obstacles.Add(new Obstacle(Geometry.ConvertToRect2D(obs)));
        }
    }
    public override List<Vector3> FindPathForAgent(GameAgent agent, Vector2 goalPosition)
    {
        return PathFinding2D.FindPath(ECMGraph, agent.RadiusIndex, agent.transform.position, goalPosition);
    }
}
