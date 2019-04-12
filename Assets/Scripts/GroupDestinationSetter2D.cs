using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class GroupDestinationSetter2D : GroupDestinationSetter
{
    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (ECMMap.Grouping)
            {
                ECMMap.FindPathGroup(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
    }

}
