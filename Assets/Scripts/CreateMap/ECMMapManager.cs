using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ECMMapManager : MonoBehaviour {
	public NavMeshSurface surface;
	public List<ObstaclesVertices> bakedECMMap;

	public static ECMMapManager instance = null;

	float yLower = 0.1f;

    float Round(float input, int decimalPlaces = 1)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return Mathf.Round(input * multiplier) / multiplier;
    }

	void Awake() {
		if(instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);
	}

	void Start () {
		bakeECMMap();
	}

	public void setGround(NavMeshSurface surfaceIn) {
		surface = surfaceIn;
	}

	public void setLowestHeight(int value) {
		yLower = value;
	}

	public List<List<Vector3>> bakeECMMap() {
		GameObject obstacles = GameObject.Find("Obstacles");

		int obstaclesCOunt = obstacles.transform.childCount;

		for (int i = 0; i < obstaclesCOunt; i++) 
        {
			surface.transform.position = new Vector3(obstacles.transform.GetChild(0).transform.position.x, 0, obstacles.transform.GetChild(0).transform.position.z);
			obstacles.transform.GetChild(0).transform.parent = surface.transform;
            surface.BuildNavMesh();   
			surface.transform.GetChild(0).transform.parent = obstacles.transform;
			NavMeshToVertices();
        }   

		return getBakedMap();
	}

	public List<List<Vector3>> getBakedMap() {
		List<List<Vector3>> result = new List<List<Vector3>>();
		for(int i = 0; i < bakedECMMap.Count; i++) {
			result.Add(bakedECMMap[i].vertices);
		}
		return result;
	}

	void NavMeshToVertices() {
		NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

		List<Vector3> vertices;
 		vertices =  new List<Vector3>(triangulatedNavMesh.vertices);

		float xMax = -999999999.0f;
		float xMin = 999999999.0f;
		float zMax = -999999999.0f;
		float zMin = 999999999.0f;
		for (int i = 0; i < vertices.Count; i++) {
			if(vertices[i].x > xMax) {
				xMax = vertices[i].x;
			}
			if(vertices[i].x < xMin) {
				xMin = vertices[i].x;
			}
			if(vertices[i].z > zMax) {
				zMax = vertices[i].z;
			}
			if(vertices[i].z < zMin) {
				zMin = vertices[i].z;
			}
		}

		for (int i = 0; i < vertices.Count; i++) {
			if(Round(vertices[i].x) == Round(xMax) ||
			   Round(vertices[i].x) == Round(xMin) ||
			   Round(vertices[i].z) == Round(zMax) ||
			   Round(vertices[i].z) == Round(zMin)
			) {
				vertices.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < vertices.Count; i++) {
			if(vertices[i].y > yLower) {
				vertices.RemoveAt(i);
				i--;
			}
		}

		int iCount = 0;
		int temp = vertices.Count;
		int vertexToPutOnList_Index = 0;
		ObstaclesVertices result = new ObstaclesVertices();
		while (iCount < temp) {
		Debug.Log(vertexToPutOnList_Index);
			result.vertices.Add(vertices[vertexToPutOnList_Index]);
			for (int index = 0; index < vertices.Count; index++) {
				if(index != vertexToPutOnList_Index && vertices[index] == vertices[vertexToPutOnList_Index + 1] && index % 2 == 0) {
					vertexToPutOnList_Index = index;
					Debug.Log(vertexToPutOnList_Index);
					break;
				}
			}
			iCount += 2;
		}

		result.vertices.Reverse();
		bakedECMMap.Add(result);
	}

	[System.Serializable]
	public class ObstaclesVertices
	{
		public List<Vector3> vertices = new List<Vector3>();
	}
	
}
