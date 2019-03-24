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
 
    public ECMMap defaultECMMap;
    [HideInInspector] public Vector2 mousePosition;
    private ExplicitCorridorMap.ECM ecm;
    private Plane m_hPlane = new Plane(Vector3.forward, Vector3.zero);
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();
    public Transform cubes;
    
    [Header("Agent setting")]
    public AgentSetting agentSetting;
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
        Simulator.Instance.setAgentDefaults(60.0f, 10, 1.0f, 0.5f, 10.0f, 10.0f, new Vector2(0.0f, 0.0f));

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

            //TODO change to 3D
            Vector3 newScale = go.transform.localScale*(agentSetting.radius * 2 / go.GetComponent<MeshRenderer>().bounds.size.x);
            go.transform.localScale = newScale;
            go.GetComponent<MeshRenderer>().material.color = agentSetting.color;
            GameAgent ga = go.GetComponent<GameAgent>();
            ga.destinationRadius = agentSetting.destinationRadius;
            //TODO change to custom ecm map 
            ga.ecmMap = defaultECMMap;
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
           
        }

    }


    void CreateCustomAgent(int index)
    {
        int numberOfAgent = agentSetting.numberOfAgent;
        int resolution =(int) Math.Round( Mathf.Sqrt(numberOfAgent))+1;
        Vector2 agentUnitPosition = new Vector2(index / resolution, index % resolution);
        Vector2 realPosition = (agentUnitPosition/resolution - new Vector2(0.5f, 0.5f))+mousePosition;
        
        int sid = Simulator.Instance.addAgent(realPosition, agentSetting.radius, agentSetting.maxSpeed, agentSetting.priority);

        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(realPosition.x(), realPosition.y(), 0), Quaternion.identity);

            //TODO change to 3D
            Vector3 newScale = go.transform.localScale * (agentSetting.radius * 2 / go.GetComponent<MeshRenderer>().bounds.size.x);
            go.transform.localScale = newScale;
            go.GetComponent<MeshRenderer>().material.color = agentSetting.color;
            GameAgent ga = go.GetComponent<GameAgent>();
            ga.destinationRadius = agentSetting.destinationRadius;
            //TODO change to custom ecm map 
            ga.ecmMap = defaultECMMap;
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
            ga.destinationRadius = Simulator.Instance.getAgentRadius(sid)*4;
            ga.ecmMap = defaultECMMap;
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
        }
    }

    
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
                    for (int i = 0; i < agentSetting.numberOfAgent; i++)
                    {
                        CreateCustomAgent(i);
                    }
                    
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