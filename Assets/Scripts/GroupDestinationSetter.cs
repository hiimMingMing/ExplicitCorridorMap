using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(ECMMap))]
public class GroupDestinationSetter : MonoBehaviour
{
    protected ECMMap ECMMap;
    protected virtual void Start()
    {
        ECMMap = GetComponent<ECMMap>();
    }
    protected virtual void Update()
    {

    }

}
