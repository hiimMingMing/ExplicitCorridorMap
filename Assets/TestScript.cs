using RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    GameAgent gameAgent;
    void Start()
    {
        gameAgent = GetComponent<GameAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("AGENT");
        //var ms = Simulator.Instance.getAgentMaxSpeed(gameAgent.Sid);
        //var v = Simulator.Instance.getAgentVelocity(gameAgent.Sid);
        //var pv = Simulator.Instance.getAgentPrefVelocity(gameAgent.Sid);
        //Debug.Log("MS:" + ms + " , V:" + Length(v) + " , PV:" + Length(pv));
    }
    float Length(RVO.Vector2 a)
    {
        return Mathf.Sqrt(a.x_ * a.x_ + a.y_ * a.y_);
    }
}
