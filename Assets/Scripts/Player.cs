using ExplicitCorridorMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public float speed = 50;
    public Transform cubes;
    // Start is called before the first frame update
    private List<Obstacle> Obstacles = new List<Obstacle>();

    private ECM ecm;
    public ECMMap ecmMap;
    [HideInInspector]
    public int RadiusIndex = 0;
    [HideInInspector]
    public float Radius;
    Vector2 targetWayPoint;

    private int currentWayPoint = 0;
    public bool isMoving = false;

    private Vector3 finalTarget;
    private List<Edge> listEdgePath = new List<Edge>();

    private List<Vector2> wayPointList = new List<Vector2>();

    //Draw the path
    LineDrawer lineDrawer;

    //For adding obstacles
    public Transform addObj;
    public List<Vector2> obsPoints;

    void Start()
    {
        ecm = ecmMap.ecm;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){
            finalTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wayPointList = PathFinding.FindPath(ecm, transform.position, finalTarget);
            //Debug.Log("Path found");
            Debug.Log("count: "+listEdgePath.Count);
            foreach (var v in listEdgePath)
                Debug.Log(v.ID);
            for (int i = 0; i < wayPointList.Count - 1; i++)
            {
                //Debug.Log(wayPointList[i]);

                //Draw the path
                lineDrawer = new LineDrawer();          
                var start = new Vector3(wayPointList[i].x, wayPointList[i].y);
                var end = new Vector3(wayPointList[i + 1].x, wayPointList[i + 1].y);
                lineDrawer.DrawLineInGameView(start, end, Color.black, 1.0f);
            }
            currentWayPoint = 1;
        }

        if (Input.GetMouseButtonDown(1))
        {
            var test = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(test.x + " - " + test.y);
        }
        if (currentWayPoint < wayPointList.Count)
        {
            targetWayPoint = wayPointList[currentWayPoint];
            Walk();
        }
    }

    void Walk()
    {
        // move towards the target
        isMoving = true;
        if (Input.GetKeyDown(KeyCode.A) && isMoving)
        {
            //Add a dynamic obstacle
            var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z -= Camera.main.transform.position.z;

            obsPoints = DynamicReplanning.convertToPoint(addObj.localScale.x, addObj.localScale.y, position);
            Object.Instantiate(addObj, position, transform.rotation);

            //Handle the path
            //listAffectPath  = DynamicReplanning.ListAffectedPath(wayPointList, obsPoints, currentWayPoint);
            //if (listAffectPath.Count > 0)
                //wayPointList = DynamicReplanning.DynamicFindPath(ecm, transform.position, finalTarget, listAffectPath, obsPoints);
        }
        else if (Input.GetKeyDown(KeyCode.D) && isMoving)
        {
            //Delete a obstacle
            var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                BoxCollider bc = (BoxCollider)hit.collider;
                if (bc != null)
                {
                    var xScale = bc.gameObject.transform.localScale.x;
                    var yScale = bc.gameObject.transform.localScale.y;
                    obsPoints = DynamicReplanning.convertToPoint(xScale, yScale, position);

                    Destroy(bc.gameObject);
                }
            }

            //Handle the path
            wayPointList = DynamicReplanning.DynamicFindPath2(ecm, transform.position, finalTarget, obsPoints);

        }
        else transform.position = Vector3.MoveTowards(transform.position, targetWayPoint, speed * Time.deltaTime);

        if (transform.position == (Vector3)targetWayPoint)
        {
            currentWayPoint++;
            if (currentWayPoint < wayPointList.Count) targetWayPoint = wayPointList[currentWayPoint];
            else isMoving = false;
        }
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////
    public struct LineDrawer
    {
        private LineRenderer lineRenderer;
        private float lineSize;

        public LineDrawer(float lineSize)
        {
            GameObject lineObj = new GameObject("LineObj");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            //Particles/Additive
            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

            this.lineSize = lineSize;
        }

        //Draws lines through the provided vertices
        public void DrawLineInGameView(Vector3 start, Vector3 end, Color color, float lineSize)
        {
            if (lineRenderer == null)
            {
                GameObject lineObj = new GameObject("LineObj");
                lineRenderer = lineObj.AddComponent<LineRenderer>();
                //Particles/Additive
                lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

                this.lineSize = lineSize;
            }

            //Set color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            //Set width
            lineRenderer.startWidth = lineSize;
            lineRenderer.endWidth = lineSize;

            //Set line count which is 2
            lineRenderer.positionCount = 2;

            //Set the postion of both two lines
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }
}
