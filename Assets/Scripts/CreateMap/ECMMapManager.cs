using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ECMMapManager : MonoBehaviour {

	public NavMeshSurface surface;
	public List<ObstaclesVertices> bakedECMMap;
    
	public static ECMMapManager instance = null;
	public List<GameObject> gameObjects;
	[SerializeField]
	public bool enableDebug = false;
    [Header("Save/Load data")]
    
    public bool bDeleteAllSaveObject = false;
    public bool isLoadedScene = false;
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
		setGround(GameObject.Find("GroundForECM").GetComponent<NavMeshSurface>());
        string sceneName = SceneManager.GetActiveScene().name;
        if (bDeleteAllSaveObject) {
            DeleteSaveObj(sceneName);
        }
        if (isLoadedScene)
        {
            
            LoadData(sceneName);
        }
        else
        {
            bakeECMMap();
        }
		if(enableDebug) {
			drawDebug();
		}
        

    }
    private void DeleteSaveObj(string sceneName) {
        if (File.Exists(Application.persistentDataPath + "/gamesave" + sceneName + ".save"))
        {
           File.Delete(Application.persistentDataPath + "/gamesave" + sceneName + ".save");
        }
    }
    private void LoadData(string sceneName)
    {
        if (!File.Exists(Application.persistentDataPath + "/gamesave" + sceneName +".save"))
        {
            SaveData(sceneName);
        }
        else
        {
            Debug.Log("Load" + Application.persistentDataPath + "/gamesave" + sceneName + ".save");

            BinaryFormatter bf = new BinaryFormatter();
            FileStream fileStream = File.Open(Application.persistentDataPath + "/gamesave" + sceneName + ".save", FileMode.Open);
            ListSave listSave = (ListSave)bf.Deserialize(fileStream);
            fileStream.Close();
            bakedECMMap = listSave.toObstacleVertices();
            GameObject obstacles = GameObject.Find("Obstacles");

            int obstaclesCOunt = obstacles.transform.childCount;
            gameObjects = new List<GameObject>(obstaclesCOunt);
            for (int i = 0; i < obstaclesCOunt; i++)
            {
                gameObjects.Add(obstacles.transform.GetChild(i).gameObject);

            }
        }
    }

    private void SaveData( string sceneName)
    {
        bakeECMMap();
        ListSave listSave = createSaveGameObject();
        BinaryFormatter bf = new BinaryFormatter();
        Debug.Log(Application.persistentDataPath + "/gamesave" + sceneName + ".save");
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave" + sceneName + ".save");
        bf.Serialize(file, listSave);
        file.Close();
        Debug.Log("GameSave");

    }

    void Start () {
		
	}

	public void drawDebug() {
		GameObject obstacles = GameObject.Find("Obstacles");
		for(int i = 0; i < bakedECMMap.Count; i++) {
			LineRenderer lineRenderer = obstacles.transform.GetChild(i).gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
			Vector3[] abc = new Vector3[1];
			lineRenderer.material.color = Color.red;
			lineRenderer.positionCount = bakedECMMap[i].vertices.Count;
			lineRenderer.SetPositions(bakedECMMap[i].vertices.ToArray());
			lineRenderer.startWidth = 0.2f;
			lineRenderer.endWidth = 0.2f;
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
		gameObjects = new List<GameObject>(obstaclesCOunt);
		for (int i = 0; i < obstaclesCOunt; i++)
        {
			gameObjects.Add(obstacles.transform.GetChild(0).gameObject);

			surface.transform.position = new Vector3(obstacles.transform.GetChild(0).transform.position.x, 0, obstacles.transform.GetChild(0).transform.position.z);
			obstacles.transform.GetChild(0).transform.parent = surface.transform;
			if(surface.transform.GetChild(0).gameObject.GetComponent<Collider>() == null) {
				MeshCollider mesCollider = surface.transform.GetChild(0).gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
				mesCollider.convex = true;
			}
			surface.transform.GetChild(0).gameObject.layer = 31;
            surface.BuildNavMesh();
			NavMeshToVertices();
			Destroy(surface.transform.GetChild(0).gameObject.GetComponent<MeshCollider>());
			surface.transform.GetChild(0).gameObject.layer = 0;
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

		// Debug.Log(surface.transform.GetChild(0).gameObject.name + " " + yLower);
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

		float minNextDist = 9999;

		for (int i = 0; i < vertices.Count - 1; i++) {
			int nextVertexIndex = 0;
			for (int j = i; j < vertices.Count; j++) {
				Vector3 pointA = vertices[i] + new Vector3(0.0f, 0.15f, 0.0f);
				Vector3 pointB = vertices[j] + new Vector3(0.0f, 0.15f, 0.0f);
				if(Physics.Linecast(pointA, pointB)) {
					continue;
				}

				// if(Physics.Raycast(vertices[i], 10 * (vertices[j] - vertices[i])) || Physics.Raycast(vertices[j], 10 * (vertices[i] - vertices[j]))) {
				// 	continue;
				// }

				if(i == j) {
					continue;
				}
				Vector3 vectorA = vertices[i] - surface.transform.GetChild(0).transform.position;
				Vector3 vectorB = vertices[j] - surface.transform.GetChild(0).transform.position;
				float angle = Vector3.SignedAngle(vectorA, vectorB, Vector3.up);
				if(angle >= 0 && angle < minNextDist ) {
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

			if(minNextDist == 9999) {
				vertices.RemoveRange(i + 1, vertices.Count - i - 1);
				break;
			}
			minNextDist = 9999;
		}

		for (int i = 0; i < vertices.Count; i++) {
			vertices[i] -= surface.transform.position;
			vertices[i] -= vertices[i].normalized * 0.3f;
			vertices[i] += surface.transform.position;
		}

		for (int i = 0; i < vertices.Count - 1; i++) {
			Vector3 pointA = vertices[i] + new Vector3(0.0f, 0.15f, 0.0f);
			Vector3 pointB = vertices[i + 1] + new Vector3(0.0f, 0.15f, 0.0f);
			Vector3 direction = (pointB - pointA).normalized;
			RaycastHit hitTarget;
			if(Physics.Raycast(pointA, direction, out hitTarget, Mathf.Infinity, 1<<31) || Physics.Raycast(pointB, -direction, out hitTarget, Mathf.Infinity, 1<<31)) {
				
				vertices.RemoveAt(i + 1);
				i--;
			}
		}

		for (int i = 0; i < vertices.Count; i++) {
			vertices[i] -= surface.transform.position;
			vertices[i] -= vertices[i].normalized * 0.1f;
			vertices[i] += surface.transform.position;
		}

		for (int i = 0; i < vertices.Count; i++) {
			// Debug.Log(i);
			// Debug.Log((i + 1) % vertices.Count);
			// Debug.Log((i + 2) % vertices.Count);

			Vector3 vec1 = vertices[i] - vertices[(i + 1) % vertices.Count] ;
			Vector3 vec2 = vertices[(i + 2) % vertices.Count] - vertices[(i + 1) % vertices.Count];
			float angle = Mathf.Abs(Vector3.SignedAngle(vec1, vec2, Vector3.up));
			// Debug.Log("angle: " + angle);

			if(angle > 160.0f) {
				vertices.RemoveAt((i + 1) % vertices.Count);
				i--;
			}
		}

		for (int i = 0; i < vertices.Count - 1; i++) {
			for (int j = i + 1; j < vertices.Count; j++) {
				if((vertices[i] - vertices[j]).magnitude < 0.45f) {
					vertices[i] = (vertices[i] + vertices[j]) / 2;
					vertices.RemoveAt(j);
					j--;
				}
			}
		}

		ObstaclesVertices result = new ObstaclesVertices();
		result.vertices = vertices;

		//result.vertices.Reverse();
		bakedECMMap.Add(result);

		yLower = 99999f;
	}

	[System.Serializable]
	public class ObstaclesVertices
	{
		public List<Vector3> vertices = new List<Vector3>();
	}
    [System.Serializable]
    public class Vector3Serializable {
        public List<float> xs= new List<float>();
        public List<float> ys= new List<float>();
        public List<float> zs= new List<float>();
    }
    [System.Serializable]
    public class ListSave {
        public List<Vector3Serializable> listSave = new List<Vector3Serializable>();
        public List<ObstaclesVertices> toObstacleVertices() {
            List<ObstaclesVertices> listObstacleVertices = new List<ObstaclesVertices>();
            for (int i = 0; i < listSave.Count; i++)
            {
                ObstaclesVertices obstaclesVertices = new ObstaclesVertices();
                for (int j = 0; j < listSave[i].xs.Count; j++)
                {
                    obstaclesVertices.vertices.Add(new Vector3(listSave[i].xs[j], listSave[i].ys[j], listSave[i].zs[j]));

                }
                listObstacleVertices.Add(obstaclesVertices);
            }

            return listObstacleVertices;
        }
    }

    private ListSave createSaveGameObject() {
        ListSave listSave = new ListSave();
        for (int i = 0; i < bakedECMMap.Count; i++) {
            Vector3Serializable vector3Serializable = new Vector3Serializable();
            for (int j = 0; j < bakedECMMap[i].vertices.Count; j++) {
                vector3Serializable.xs.Add(bakedECMMap[i].vertices[j].x);
                vector3Serializable.ys.Add(bakedECMMap[i].vertices[j].y);
                vector3Serializable.zs.Add(bakedECMMap[i].vertices[j].z);

            }
            listSave.listSave.Add(vector3Serializable);
        }
        return listSave;
    }
    private void SaveAsJson() {
        ListSave listSave = createSaveGameObject();
        string JSon = JsonUtility.ToJson(listSave);
        Debug.Log("Saving object" + JSon);
    }
   
}