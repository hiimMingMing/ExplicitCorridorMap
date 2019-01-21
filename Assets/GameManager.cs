using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform[] cubes;
    // Start is called before the first frame update
    [HideInInspector]
    public List<RectInt> Obstacles = new List<RectInt>();

    void Start()
    {
        foreach(var cube in cubes)
        {
            int w = (int)cube.localScale.x;
            int h = (int)cube.localScale.y;
            int x = (int)cube.position.x;
            int y = (int)cube.position.y;
            Obstacles.Add(new RectInt(x-w/2,y-h/2,w,h));
        }
        foreach(var r in Obstacles)
        {
            Debug.Log(r);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
