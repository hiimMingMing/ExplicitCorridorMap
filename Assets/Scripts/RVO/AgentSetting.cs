using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class AgentSetting : ScriptableObject
{
    public bool isEnable = true;
    public float radius;
    public float priority;
    public float maxSpeed;
    public Color color;
}
