using System;
using System.Collections;
using System.Collections.Generic;
using Lean;
using RVO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Experimental.UIElements;
using Random = System.Random;
using Vector2 = RVO.Vector2;

public class GameMainManager : SingletonBehaviour<GameMainManager>
{
    public GameObject agentPrefab;
    public AgentSetting agentSetting;
    [HideInInspector] public Vector2 mousePosition;
    private ExplicitCorridorMap.ECM ecm;
    private Plane m_hPlane = new Plane(Vector3.forward, Vector3.zero);
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();
    public Transform cubes;
    public ExplicitCorridorMap.ECM getECM() {
        return ecm;
    }

   
    // Use this for initialization
    void Start()
    {
        //Construct ECM
        List<ExplicitCorridorMap.Obstacle> obstacles = new List<ExplicitCorridorMap.Obstacle>();
        foreach (Transform cube in cubes)
        {
            obstacles.Add(new ExplicitCorridorMap.Obstacle(ExplicitCorridorMap.Geometry.ConvertToRect(cube)));
            //Obstacles.Add(new Obstacle(Geometry.ConvertToRect(cube)));
        }
        ecm = new ExplicitCorridorMap.ECM(obstacles, new ExplicitCorridorMap.Obstacle(new RectInt(0, 0, 500, 500)));
        ecm.Construct();

        //End
        Simulator.Instance.setTimeStep(1f);
        Simulator.Instance.setAgentDefaults(60.0f, 10, 5.0f, 5.0f, 10.0f, 10.0f, new Vector2(0.0f, 0.0f));

        // add in awake
        Simulator.Instance.processObstacles();
    }

    private void UpdateMousePosition()
    {
        Vector3 position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (m_hPlane.Raycast(mouseRay, out rayDistance))
            position = mouseRay.GetPoint(rayDistance);

        mousePosition.x_ = position.x;
        mousePosition.y_ = position.y;
    }

    void DeleteAgent()
    {
        float rangeSq = float.MaxValue;
        int agentNo = Simulator.Instance.queryNearAgent(mousePosition, 1.5f);
        if (agentNo == -1 || !m_agentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        LeanPool.Despawn(m_agentMap[agentNo].gameObject);
        m_agentMap.Remove(agentNo);
    }

    void CreateCustomAgent() {
        int sid = Simulator.Instance.addAgent(mousePosition,agentSetting.radius,agentSetting.maxSpeed, agentSetting.priority);

        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x(), mousePosition.y(), 0), Quaternion.identity);
            go.GetComponent<MeshRenderer>().material.color = agentSetting.color;
            GameAgent ga = go.GetComponent<GameAgent>();
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
           
        }

    }

    void CreatDefaultAgent()
    {
        int sid = Simulator.Instance.addAgent(mousePosition);
         
        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x(), mousePosition.y(), 0), Quaternion.identity);
            GameAgent ga = go.GetComponent<GameAgent>();
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
        }
    }

    //public void CreatAgent(Vector2 position,GameAgent ga)
    //{
    //    Simulator.Instance.setAgentDefaults(60.0f, 10, 5.0f, 5.0f, 10.0f, 2.0f, new Vector2(0.0f, 0.0f));
    //    int sid = Simulator.Instance.addAgent(position);
    //    if (sid >= 0)
    //    {
            
    //        Assert.IsNotNull(ga);
    //        ga.sid = sid;
    //        m_agentMap.Add(sid, ga);
    //    }
    //}

    // Update is called once per frame
    private void Update()
    {
        UpdateMousePosition();
        if (Input.GetMouseButtonUp(0))
        {
            if (Input.GetKey(KeyCode.Delete))
            {
                DeleteAgent();
            }
            else
            {
                if (agentSetting.isEnable)
                {
                    CreateCustomAgent();
                }
                else
                {
                    CreatDefaultAgent();
                }
            }
        }

       
    }

    void FixedUpdate() {

        Simulator.Instance.doStep();
    }

}