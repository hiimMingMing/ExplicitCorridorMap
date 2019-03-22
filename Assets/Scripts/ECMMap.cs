using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ECMMap : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;
    public Transform Obstacles;
    public Transform DynamicObstacle;

    public ECM ecm;
    public List<float> AgentRadiusList;

    void Awake()
    {
        var obsList = new List<Obstacle>();
        foreach (Transform obs in Obstacles)
        {
            obsList.Add(new Obstacle(Geometry.ConvertToRect(obs)));
        }
        ecm = new ECM(obsList, new Obstacle(new RectInt(0, 0, 500, 500)));
        ecm.Construct();
        ecm.AddAgentRadius(AgentRadiusList);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
