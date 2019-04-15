using ExplicitCorridorMap;
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
    private List<GameAgent> group = new List<GameAgent>();
    private Plane Plane = new Plane(Vector3.up, Vector3.zero);
    private Vector3 lastClick = new Vector3(0, 0);
    private float mouseHelddownTime = 0;
    private float helddownThreshhold = 0.2f;
    public Transform groupBorder;

    protected void Start()
    {
        ECMMap = GetComponent<ECMMap>();
    }
    protected void Update()
    {
        if (!ECMMap.Grouping) return;

        Vector2 mousePosition = Vector2.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (Plane.Raycast(mouseRay, out rayDistance))
        {
            mousePosition = mouseRay.GetPoint(rayDistance).To2D();
        }
        Debug.Log(group.Count+"G");
        if (Input.GetMouseButtonDown(1))
        {
            foreach (var ga in group)
            {
                ECMMap.FindPathGroup(group, mousePosition);
                //ga.FindPath(mousePosition);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            lastClick = new Vector3(mousePosition.x, 0, mousePosition.y);
        }
        if (Input.GetMouseButton(0))
        {
            mouseHelddownTime += Time.deltaTime;
            Vector3 click = new Vector3(mousePosition.x, 0, mousePosition.y);
            if (mouseHelddownTime > helddownThreshhold)
            {
                Vector3 newScale = (click - lastClick);
                newScale = new Vector3(newScale.x, 1, newScale.z);
                groupBorder.transform.localScale = newScale;
                groupBorder.transform.position = lastClick + newScale / 2;
                return;
            }

        }
        if (Input.GetMouseButtonUp(0))
        {
            groupBorder.transform.localScale = Vector3.zero;
            if (mouseHelddownTime > helddownThreshhold)
            {
                GameAgent[] allAgent = FindObjectsOfType<GameAgent>();
                resetGroup();
                Vector2 click = new Vector2(mousePosition.x, mousePosition.y);
                List<Vector2> bound = new List<Vector2> { new Vector2(lastClick.x, lastClick.z), new Vector2(lastClick.x, click.y), new Vector2(click.x, click.y), new Vector2(click.x, lastClick.z) };

                foreach (var item in allAgent)
                {
                    Vector2 position = item.transform.position.To2D();
                    if (Geometry.PolygonContainsPoint(bound, position))
                    {
                        item.transform.GetChild(0).gameObject.SetActive(true);
                        group.Add(item);
                    }
                }
                mouseHelddownTime = 0;
                return;
            }
            else
            {
                resetGroup();
            }
            
        }
    }
    void resetGroup()
    {
        foreach (var item in group)
        {
            item.transform.GetChild(0).gameObject.SetActive(false);
        }

        group.Clear();

    }

}
