using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExplicitCorridorMap;
using RBush;

[CustomEditor(typeof(ECMMap))]
public class ECMMapEditor : Editor
{

    ECM ecm;
    float inputPointRadius = 2f;
    float outputPointRadius = 2f;

    bool drawNearestObstaclePoints = false;
    List<Vector2> shortestPath = null;
    List<Vector2> portalsLeft;
    List<Vector2> portalsRight;
    List<Edge> selectedEdge;
    List<Segment> segments = new List<Segment>();
    int ObstacleToDelete = 0;
    List<List<Vector2>> curveEdges = new List<List<Vector2>>();
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var map = (ECMMap)target;
        var startPosition = map.StartPoint.position;
        var endPosition = map.EndPoint.position;
        var obsTransform = map.Obstacles;
        var dynamicObstacle = map.DynamicObstacle;
        var agentRadius = map.AgentRadiusList;
        //OpenVoronoi openVoronoi = target as OpenVoronoi;
        inputPointRadius = EditorGUILayout.FloatField("Input Point Radius", inputPointRadius);
        outputPointRadius = EditorGUILayout.FloatField("Output Point Radius", outputPointRadius);
        drawNearestObstaclePoints = EditorGUILayout.Toggle("Draw Nearest Obs Points", drawNearestObstaclePoints);

        if (GUILayout.Button("Bake"))
        {
            //populate segment
            var obstacles = new List<Obstacle>();
            foreach (Transform obs in obsTransform)
            {
                obstacles.Add(new Obstacle(Geometry.ConvertToRect(obs)));
            }

            ecm = new ECM(obstacles, new Obstacle(new RectInt(0, 0, 500, 500)));
            ecm.Construct();
            ComputeCurveEdge();
            foreach (var r in agentRadius)
            {
                ecm.AddAgentRadius(r);

            }
            shortestPath = PathFinding.FindPathDebug(ecm, startPosition, endPosition, out portalsLeft,out portalsRight);

        }
        if (GUILayout.Button("Add Obstacle"))
        {
            if (ecm == null)
            {
                Debug.Log("Ecm must be baked");
            }
            else
            {
                ecm.AddPolygonDynamic(new Obstacle(Geometry.ConvertToRect(dynamicObstacle)));
                ComputeCurveEdge();
                shortestPath = PathFinding.FindPathDebug(ecm, startPosition, endPosition, out portalsLeft, out portalsRight);
            }
        }

        ObstacleToDelete = EditorGUILayout.IntField("ObstacleToDelete", ObstacleToDelete);
        if (GUILayout.Button("Delete Obstacle"))
        {
            if (ecm == null)
            {
                Debug.Log("Ecm must be baked");
            }
            else
            {
                ecm.DeletePolygonDynamic(ObstacleToDelete);
                ComputeCurveEdge();
                shortestPath = PathFinding.FindPathDebug(ecm, startPosition, endPosition, out portalsLeft, out portalsRight);
            }
        }
    }

    void OnSceneGUI()
    {

        //Draw input point
        if (ecm == null) return;

        foreach(var obs in ecm.Obstacles.Values)
        {
            Handles.Label(new Vector2((obs.Envelope.MinX+obs.Envelope.MaxX)/2, (obs.Envelope.MinY + obs.Envelope.MaxY) / 2), obs.ID.ToString());
        }
        //Draw input segment
        Handles.color = Color.yellow;
        foreach (var inputSegment in ecm.InputSegments.Values)
        {
            var startPoint = new Vector3(inputSegment.Start.x, inputSegment.Start.y);
            var endPoint = new Vector3(inputSegment.End.x, inputSegment.End.y);
            Handles.Label((startPoint+endPoint)/2, inputSegment.ID.ToString());

            Handles.DrawSolidDisc(startPoint, Vector3.forward, inputPointRadius);
            Handles.DrawSolidDisc(endPoint, Vector3.forward, inputPointRadius);
            Handles.DrawLine(startPoint, endPoint);
        }

        //Draw ouput edge and vertex
        foreach (var vertex in ecm.Vertices.Values)
        {
            DrawVertex(vertex);
            foreach (var edge in vertex.Edges)
            {
                DrawEdge(edge);
            }
        }
        Handles.color = Color.blue;
        foreach(var l in curveEdges)
        {
            DrawPolyLine(l);
        }
        //Draw Nearest Obstacle Point
        if (drawNearestObstaclePoints)
        {
            foreach (var edge in ecm.Edges.Values)
            {
                DrawObstaclePoint(edge);
            }
        }
        if ( shortestPath != null)
        {
            Handles.color = Color.red;
            DrawPolyLine(shortestPath);
        }
        if(portalsLeft != null)
        {
            for(int i = 0; i < portalsLeft.Count; i++)
            {
                DrawPortal(portalsLeft[i], portalsRight[i]);
            }
        }
    }
    


    
    void DrawPortal(Vector2 begin, Vector2 end)
    {
        Handles.color = Color.magenta;
        Handles.DrawLine(begin,end);

    }
    void DrawObstaclePoint(Edge edge)
    {
        var startVertex = edge.Start.Position;
        var endVertex = edge.End.Position;
        var begin = startVertex;
        var obsLeft = edge.LeftObstacleOfStart;
        var obsRight = edge.RightObstacleOfStart;

        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

        begin = endVertex;
        obsLeft = edge.LeftObstacleOfEnd;
        obsRight = edge.RightObstacleOfEnd;
        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

    }
    void DrawObstaclePointProperty(Edge edge, int index)
    {
        var startVertex = edge.Start.Position;
        var endVertex = edge.End.Position;
        var begin = startVertex;
        var obsLeft = edge.EdgeProperties[index].LeftObstacleOfStart;
        var obsRight = edge.EdgeProperties[index].RightObstacleOfStart;

        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

        begin = endVertex;
        obsLeft = edge.EdgeProperties[index].LeftObstacleOfEnd;
        obsRight = edge.EdgeProperties[index].RightObstacleOfEnd;
        Handles.color = Color.green;
        Handles.DrawLine(begin, obsLeft);
        Handles.color = Color.cyan;
        Handles.DrawLine(begin, obsRight);

    }
    void DrawVertex(Vertex vertex)
    {
        Handles.color = Color.red;
        var position = new Vector3((float)vertex.X, (float)vertex.Y);
        Handles.DrawSolidDisc(position, Vector3.forward, outputPointRadius);
        Handles.Label(position,vertex.ID+"");
    }
    void DrawEdge(Edge edge)
    {

        if (edge.IsLinear)
        {
            Handles.color = Color.blue;
            Handles.DrawLine(edge.Start.Position, edge.End.Position);
        }
    }
    
    void ComputeCurveEdge()
    {
        curveEdges.Clear();
        foreach(var e in ecm.Edges.Values)
        {
            if (!e.IsLinear)
            {
                List<Vector2> discretizedEdge = ecm.SampleCurvedEdge(e, 10);
                curveEdges.Add(discretizedEdge);
            }
        }
    }
    void DrawPolyLine(List<Vector2> l)
    {
        for(int i = 0; i < l.Count - 1; i++)
        {
            Handles.DrawLine(l[i],l[i+1]);
        }
    }
}



