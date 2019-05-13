using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System;
public class ManipulateAgentComponent : MonoBehaviour
{
    private Vector2 mousePosition;
    private ECMMap ECMMap;
    public GameObject agentPrefab;
    public int multipleAgentPerAdd = 100;
    // Start is called before the first frame update
    void Start()
    {
        ECMMap = FindObjectOfType<ECMMap>();
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 _position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        Plane basePlane = new Plane(Vector3.up, Vector3.zero); ;


        if (basePlane.Raycast(mouseRay, out rayDistance))
        {
            Vector3 temp = mouseRay.GetPoint(rayDistance);
            _position = new Vector3(temp.x, temp.z, 0);
        }

        mousePosition = new Vector2(_position.x, _position.y);

        if (Input.GetKeyDown(KeyCode.C)) {
            CreateCustomAgent();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            CreateMultiAgent(agentPrefab, new Vector3(mousePosition.x, 0, mousePosition.y), multipleAgentPerAdd);
        }
    }

   
    void DeleteAgent()
    {
        //float rangeSq = float.MaxValue;
        int agentNo = Simulator.Instance.queryNearAgent(mousePosition, 1.5f);
        if (agentNo == -1 || !ECMMap.AgentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        Lean.LeanPool.Despawn(ECMMap.AgentMap[agentNo].gameObject);
        ECMMap.AgentMap.Remove(agentNo);
    }

    void CreateCustomAgent()
    {

        GameObject go = Lean.LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x, 0, mousePosition.y), Quaternion.identity);
        
        GameAgent ga = go.GetComponent<GameAgent>();
        ga.Radius = ECMMap.AgentRadiusList[ga.RadiusIndex];
        float currentRadius = go.GetComponent<MeshRenderer>().bounds.size.x/2;
        Vector3 newScale = go.transform.localScale * (ga.Radius  / (currentRadius));
        go.transform.localScale = newScale;


    }

    public void CreateAgent(GameObject objectPrefab, Vector3 position) {

        GameObject go = Lean.LeanPool.Spawn(objectPrefab, position, Quaternion.identity);

        GameAgent ga = objectPrefab.GetComponent<GameAgent>();
        Vector3 newScale = go.transform.localScale * (ga.Radius * 2 / (2 * go.GetComponent<MeshRenderer>().bounds.size.x));
        go.transform.localScale = newScale;
    }


    public void CreateMultiAgent(GameObject objectPrefab, Vector3 position,int number)
    {

        for (int i = 0; i < number; i++)
        {
            CreateCustomAgent(objectPrefab, position, i, number);
        }
    }
    void CreateCustomAgent(GameObject objectPrefab,Vector3 position,int index, int numberOfAgent)
    {

        int resolution = (int)Math.Round(Mathf.Sqrt(numberOfAgent)) + 1;
        Vector2 agentUnitPosition = new Vector2(index / resolution, index % resolution);
        Vector2 realPosition = (agentUnitPosition / resolution - new Vector2(0.5f, 0.5f)) + mousePosition;




        GameObject go = Lean.LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x, 0, mousePosition.y), Quaternion.identity);
        
        GameAgent ga = go.GetComponent<GameAgent>();
        ga.Radius = ECMMap.AgentRadiusList[ga.RadiusIndex];
        float currentRadius = go.GetComponent<MeshRenderer>().bounds.size.x/2;
        Vector3 newScale = go.transform.localScale * (ga.Radius  / (currentRadius));
        go.transform.localScale = newScale;





    }

}
