using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DestinationSetter2D : DestinationSetter
{

    protected override void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!GameAgent.ECMMap.Grouping)
            {
                GameAgent.FindPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
    }

}
