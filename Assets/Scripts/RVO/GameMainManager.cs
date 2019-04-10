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
using ExplicitCorridorMap;
public class GameMainManager : SingletonBehaviour<GameMainManager>
{

    public bool isGroupTarget = false;
    public List<GameObject> group;
    public Vector3 lastClick = new Vector3(0,0);
    public float mouseHelddownTime = 0;
    private float helddownThreshhold = 0.2f;
    LineDrawer[] lineDrawer = new LineDrawer[4];

    public bool DebugMode = true;

    public GameObject agentPrefab;
    public bool is3D = true;
    public ECMMap defaultECMMap;
    [HideInInspector] public Vector2 mousePosition;
    private ExplicitCorridorMap.ECM ecm;
    public  ECMMap ecmmap;
    private Plane m_hPlane ;
    [HideInInspector]
    public Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();
    public Transform cubes;
    // use to store current added obstacle
    [HideInInspector]
    public RectInt addedObstacle;
    public Transform defaultObstacle;
    [Header("Agent setting")]
    public AgentSetting agentSetting;
    public ExplicitCorridorMap.ECM getECM() {
        return ecmmap.ecm;
    }

    //Use in SampleScene(NguyenQuocBao)
    public bool isNull() {
        if (agentPrefab == null) return true;
        else return false;
    }
   
    // Use this for initialization
    void Start()
    {

       
        if (ecmmap == null) {
            Debug.Log("No ECMMap!");
        }
         

        //End
        Simulator.Instance.setTimeStep(1f);
        Simulator.Instance.setAgentDefaults(120.0f, 50, 1.0f, 0.5f, 10.0f, 10.0f, new Vector2(0.0f, 0.0f));
        if (is3D)
        {
            m_hPlane = new Plane(Vector3.up, Vector3.zero);
        }
        else {
            m_hPlane = new Plane(Vector3.forward, Vector3.zero);
        }
        // add in awake
        Simulator.Instance.processObstacles();
    }

    private void UpdateMousePosition()
    {
        Vector3 position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (is3D)
        {
            if (m_hPlane.Raycast(mouseRay, out rayDistance))
                position = mouseRay.GetPoint(rayDistance).to2D();
        }
        else
        {
            if (m_hPlane.Raycast(mouseRay, out rayDistance))
                position = mouseRay.GetPoint(rayDistance);

        }
        mousePosition.x_ = position.x;
        mousePosition.y_ = position.y;
    }

    void DeleteAgent()
    {
        //float rangeSq = float.MaxValue;
        int agentNo = Simulator.Instance.queryNearAgent(mousePosition, 1.5f);
        if (agentNo == -1 || !m_agentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        LeanPool.Despawn(m_agentMap[agentNo].gameObject);
        m_agentMap.Remove(agentNo);
    }

    void CreateCustomAgent() {
        int sid = Simulator.Instance.addAgent(mousePosition, agentSetting.radius, agentSetting.maxSpeed, agentSetting.priority);
        if (is3D)
        {
            

            if (sid >= 0)
            {
                GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x(), mousePosition.y(), 0).to3D(), Quaternion.identity);

                //TODO change to 3D

                Vector3 newScale = go.transform.localScale * (agentSetting.radius * 2 / (go.GetComponent<MeshRenderer>().bounds.size.x));
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
        else
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x(), mousePosition.y(), 0), Quaternion.identity);

            //TODO change to 3D

            Vector3 newScale = go.transform.localScale * (agentSetting.radius * 2 / (go.GetComponent<MeshRenderer>().bounds.size.x));
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
        if (is3D)
        {
            if (sid >= 0)
            {
                GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(realPosition.x(), realPosition.y(), 0).to3D(), Quaternion.identity);

                //TODO change to 3D
                Vector3 newScale = go.transform.localScale * (agentSetting.radius * 2 / (2 * go.GetComponent<MeshRenderer>().bounds.size.x));
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
        else {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(realPosition.x(), realPosition.y(), 0), Quaternion.identity);

            //TODO change to 3D
            Vector3 newScale = go.transform.localScale * (agentSetting.radius * 2 / (2 * go.GetComponent<MeshRenderer>().bounds.size.x));
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
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x(), mousePosition.y(), 0).to3D(), Quaternion.identity);
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
        //// Use in sampleScene
        if (isNull()) return;
        UpdateMousePosition();
        
        if (isGroupTarget)
        {
            if (Input.GetMouseButtonDown(1)) {
                foreach (var item in group)
                {
                    GameAgent ga = item.GetComponent<GameAgent>();
                    ga.newPath();
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                lastClick = new Vector3(mousePosition.x_, 0, mousePosition.y_);

            }
            if (Input.GetMouseButton(0))
            {

                mouseHelddownTime += Time.deltaTime;
                Vector3 click = new Vector3(mousePosition.x_, 0, mousePosition.y_);
                if (mouseHelddownTime > helddownThreshhold)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        if (lineDrawer[i] != null)
                        {
                            lineDrawer[i].Destroy();
                        }
                        lineDrawer[i] = new LineDrawer();
                    }
                    Vector3[] bound = { new Vector3(lastClick.x, 0, lastClick.z), new Vector3(lastClick.x, 0, click.z), new Vector3(click.x, 0, click.z), new Vector3(click.x, 0, lastClick.z) };


                    for (int i = 0; i < 4; i++)
                    {
                        lineDrawer[i].DrawLineInGameView(bound[i], bound[(i + 1) % 4], Color.blue, 1f);
                    }
                    return;
                }

            }
          
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (isGroupTarget)
            {
                
                if (mouseHelddownTime > helddownThreshhold)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (lineDrawer[i] != null)
                        {
                            lineDrawer[i].Destroy();
                        }

                    }
                    GameObject[] allAgent = GameObject.FindGameObjectsWithTag("Player");
                    group.Clear();

                    Vector2 click = new Vector2(mousePosition.x_, mousePosition.y_);
                    List<UnityEngine.Vector2> bound = new List<UnityEngine.Vector2> { new UnityEngine.Vector2(lastClick.x, lastClick.z), new UnityEngine.Vector2(lastClick.x, click.y_), new UnityEngine.Vector2(click.x_, click.y_), new UnityEngine.Vector2(click.x_, lastClick.z) };

                    foreach (var item in allAgent)
                    {
                        UnityEngine.Vector2 position = new UnityEngine.Vector2(item.transform.position.x, item.transform.position.z);
                        if (Geometry.PolygonContainsPoint(bound, position))
                        {
                            group.Add(item);
                        }
                    }
                    mouseHelddownTime = 0;
                    return;
                }
            }
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
        
        DynamicReplanning.HandleDynamicEvent( );
    }

    void FixedUpdate() {

        Simulator.Instance.doStep();
    }

}