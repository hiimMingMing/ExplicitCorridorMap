using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(GameAgent))]
public class DestinationSetter : MonoBehaviour
{
    private Vector2 MousePosition;
    private GameAgent GameAgent;
    private Plane Plane = new Plane(Vector3.up, Vector3.zero);
    protected void Start()
    {
        GameAgent = GetComponent<GameAgent>();
    }
    protected void Update()
    {
        if (GameAgent.ECMMap.Grouping) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDistance;
            if (Plane.Raycast(mouseRay, out rayDistance))
            {
                var destination = mouseRay.GetPoint(rayDistance).To2D();
                GameAgent.FindPath(destination);
            }
        }

    }

}
