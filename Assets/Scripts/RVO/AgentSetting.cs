using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class AgentSetting : ScriptableObject
{
    public bool isEnable = true;
    [HideInInspector]
    public float radius;
    [HideInInspector]
    public int radiusIndex;
    public float priority;
    public float maxSpeed;
    public Color color;
    public ECMMap ecmMap;
    public float destinationRadius;
    [Range(1,1000)]
    public int numberOfAgent;
   
}
