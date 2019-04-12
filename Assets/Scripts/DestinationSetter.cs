using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(GameAgent))]
public class DestinationSetter : MonoBehaviour
{
    protected GameAgent GameAgent;
    protected virtual void Start()
    {
        GameAgent = GetComponent<GameAgent>();
    }
    protected virtual void Update()
    {

    }

}
