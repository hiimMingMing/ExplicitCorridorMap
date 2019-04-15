using ExplicitCorridorMap;
using ExplicitCorridorMap.Maths;
using RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obstacle = ExplicitCorridorMap.Obstacle;
using Vector2 = UnityEngine.Vector2;

public class ECMMap : MonoBehaviour
{
    public Transform ObstaclesTransform;
    public Transform GroundPlane;
    public List<float> AgentRadiusList;
    public bool Grouping = false;
    public ECM ECMGraph { get; protected set; }
    private List<Obstacle> Obstacles;

    public Dictionary<int, GameAgent> AgentMap;

    void Awake()
    {
        Obstacles = new List<Obstacle>();
        AgentMap = new Dictionary<int, GameAgent>();
        foreach (Transform obs in ObstaclesTransform)
        {
            Obstacles.Add(Geometry.ConvertToObstacle(obs));
        }
        var mesh = GroundPlane.transform.GetComponent<MeshRenderer>();
        float width = mesh.bounds.size.x;
        float height = mesh.bounds.size.z;
        ECMGraph = new ECM(Obstacles, new Obstacle(new RectInt((int)(GroundPlane.transform.position.z - (width / 2)), (int)(GroundPlane.transform.position.x - (height / 2)), (int)width, (int)height)));
        ECMGraph.Construct();
        ECMGraph.AddAgentRadius(AgentRadiusList);

        //RVO
        Simulator.Instance.setTimeStep(1f);
        Simulator.Instance.setAgentDefaults(200.0f, 50, 0.1f, 0.05f, 10.0f, 10.0f, new RVO.Vector2(0.0f, 0.0f));
        foreach (Transform obs in ObstaclesTransform)
        {
            Simulator.Instance.addObstacle(Geometry.ConvertToListOfLine(obs));
        }
        Simulator.Instance.processObstacles();
    }

    private void FixedUpdate()
    {
        Simulator.Instance.doStep();
    }

    public void FindPathGroup( List<GameAgent> AgentGroup, Vector2 finalTarget)
    {
        if (!Grouping) throw new Exception("Cannot call is function when ECMMap.Grouping is off");
        var gh = new GroupHandler(ECMGraph, AgentGroup);
        gh.FindPath(finalTarget);
    }
}
