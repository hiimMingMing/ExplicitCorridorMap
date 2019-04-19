using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class ECMMapManager : MonoBehaviour {

	public NavMeshSurface surface;
	public List<ObstaclesVertices> bakedECMMap;

	public static ECMMapManager instance = null;

	[SerializeField]
	public bool enableDebug = false;


	float yLower = 99999f;

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
		setGround(GameObject.Find("GroundForECM").GetComponent<NavMeshSurface>());
		bakeECMMap();
		if(enableDebug) {
			drawDebug();
		}
	}

	public void drawDebug() {
		GameObject obstacles = GameObject.Find("Obstacles");
		for(int i = 0; i < bakedECMMap.Count; i++) {
			LineRenderer lineRenderer = obstacles.transform.GetChild(i).gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
			Vector3[] abc = new Vector3[1];
			lineRenderer.positionCount = bakedECMMap[i].vertices.Count;
			lineRenderer.SetPositions(bakedECMMap[i].vertices.ToArray());
			lineRenderer.startWidth = 0.05f;
			lineRenderer.endWidth = 0.05f;
			lineRenderer.loop = true;

			for(int j = 0; j < lineRenderer.positionCount; j++) {
				lineRenderer.SetPosition(j, lineRenderer.GetPosition(j) + new Vector3(0.0f, 0.1f, 0.0f));
			}
		}
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
			MeshCollider mesCollider = surface.transform.GetChild(0).gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
			mesCollider.convex = true;
            surface.BuildNavMesh();
			NavMeshToVertices();
			Destroy(surface.transform.GetChild(0).gameObject.GetComponent<MeshCollider>());
			surface.transform.GetChild(0).transform.parent = obstacles.transform;
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
			if(vertices[i].y < yLower) {
				yLower = vertices[i].y;
			}
		}

		for (int i = 0; i < vertices.Count; i++) {
			if(vertices[i].x == xMax ||
			   vertices[i].x == xMin ||
			   vertices[i].z == zMax ||
			   vertices[i].z == zMin
			) {
				vertices.RemoveAt(i);
				i--;
			}
		}

		Debug.Log(surface.transform.GetChild(0).gameObject.name + " " + yLower);
		if(yLower < 0.1f) {
			yLower = 0.1f;
		}

		for (int i = 0; i < vertices.Count; i++) {
			if(vertices[i].y > yLower) {
				vertices.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < vertices.Count - 1; i++) {
			for (int j = i + 1; j < vertices.Count; j++) {
				if(vertices[i] == vertices[j]) {
					vertices.RemoveAt(j);
					j--;
				}
			}
		}

		for (int i = 0; i < vertices.Count - 1; i++) {
			for (int j = i + 1; j < vertices.Count; j++) {
				if((vertices[i] - vertices[j]).magnitude < 0.3f) {
					vertices[i] = (vertices[i] + vertices[j]) / 2;
					vertices.RemoveAt(j);
					j--;
				}
			}
		}

		float minNextDist = 9999;

		for (int i = 0; i < vertices.Count - 1; i++) {
			int nextVertexIndex = 0;
			for (int j = i; j < vertices.Count; j++) {
				if(i == j) {
					continue;
				}
				Vector3 vectorA = vertices[i] - surface.transform.GetChild(0).transform.position;
				Vector3 vectorB = vertices[j] - surface.transform.GetChild(0).transform.position;
				float angle = Vector3.SignedAngle(vectorA, vectorB, Vector3.up);
				Debug.Log(angle);
				if(angle >= 0 && angle < minNextDist) {
					nextVertexIndex = j;
					minNextDist = angle;
				}
				
				// Vector3 normal = vertices[j] - vertices[i];
				// normal = new Vector3(-normal.z, 0.0f, normal.x);
				// Vector3 startPoint = (vertices[j] + vertices[i]) / 2;
				// Vector3 endPoint = startPoint + normal * 0.3f;
				// Vector3 endPoint2 = startPoint - normal * 0.3f;
				// RaycastHit hitTarget;
				// if (Physics.Raycast(startPoint, normal.normalized, out hitTarget) && !Physics.Raycast(vertices[i], vertices[j] - vertices[i])) {
				// 	Debug.Log(hitTarget.collider.gameObject.name);
				// 	if(hitTarget.collider.gameObject.name == surface.transform.GetChild(0).gameObject.name) {
				// 		if((vertices[j] - vertices[i]).magnitude < minNextDist) {
				// 			nextVertexIndex = j;
				// 			minNextDist = normal.magnitude;
				// 		}
				// 	}
				// }

				// if(Physics.Raycast(vertices[i], vertices[j] - vertices[i])) {
				// 	Debug.DrawLine(vertices[i], vertices[j], Color.red, 180);
				// }
			}
			if(nextVertexIndex == 0) {
				vertices.RemoveRange(i + 1, vertices.Count - i - 1);
				break;
			}
			Vector3 tempPoint = vertices[i + 1];
			vertices[i + 1] = vertices[nextVertexIndex];
			vertices[nextVertexIndex] = tempPoint;

			// if(minNextDist == 9999) {
			// 	vertices.RemoveRange(i + 1, vertices.Count - i - 1);
			// 	break;
			// }
			minNextDist = 9999;
		}

		for (int i = 0; i < vertices.Count; i++) {
			Vector3 vec1 = vertices[i] - vertices[(i + 1) % vertices.Count] ;
			Vector3 vec2 = vertices[(i + 1) % vertices.Count] - vertices[(i + 2) % vertices.Count];
			float angle = Vector3.Angle(vec1, vec2);
			// Debug.Log("angle: " + angle);
			if(angle < 20f || angle > 160.0f) {
				vertices.RemoveAt((i + 1) % vertices.Count);
				i--;
			}
		}

		for (int i = 0; i < vertices.Count; i++) {
			vertices[i] -= surface.transform.position;
			vertices[i] -= vertices[i].normalized * 0.3f;
			vertices[i] += surface.transform.position;
		}

		ObstaclesVertices result = new ObstaclesVertices();
		result.vertices = vertices;

		result.vertices.Reverse();
		bakedECMMap.Add(result);

		yLower = 99999f;
	}

	[System.Serializable]
	public class ObstaclesVertices
	{
		public List<Vector3> vertices = new List<Vector3>();
	}


}