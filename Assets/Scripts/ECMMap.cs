using ExplicitCorridorMap;
using ExplicitCorridorMap.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ECMMap : MonoBehaviour
{
    public Transform ObstaclesTransform;
    public List<float> AgentRadiusList;
    public bool Grouping = false;
    public ECM ECMGraph { get; protected set; }
    protected List<GameAgent> AgentGroup;
    protected List<Obstacle> Obstacles;
    void Awake()
    {
        InitObstacleList();
        ECMGraph = new ECM(Obstacles, new Obstacle(new RectInt(0, 0, 500, 500)));
        ECMGraph.Construct();
        ECMGraph.AddAgentRadius(AgentRadiusList);
        AgentGroup = new List<GameAgent>();
    }
    protected virtual void InitObstacleList()
    {
        Obstacles = new List<Obstacle>();
        foreach (Transform obs in ObstaclesTransform)
        {
            Obstacles.Add(new Obstacle(Geometry.ConvertToRect(obs)));
        }
    }
    public void AddAgentToGroup(GameAgent agent)
    {
        AgentGroup.Add(agent);
    }
    public void SetAgentGroup(List<GameAgent> agents)
    {
        AgentGroup = agents;
    }
    public void FindPathGroup(Vector2 finalTarget)
    {
        if (!Grouping) throw new Exception("Cannot call is function when ECMMap.Grouping is off");
        var gh = new GroupHandler(ECMGraph, AgentGroup);
        gh.FindPath(finalTarget);
    }
    public virtual List<Vector3> FindPathForAgent(GameAgent agent, Vector2 goalPosition)
    {
        return PathFinding.FindPath(ECMGraph, agent.RadiusIndex, agent.transform.position, goalPosition);
    }
}
