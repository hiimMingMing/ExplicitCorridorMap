using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[ExecuteInEditMode]
public class ECMMap_Editor : MonoBehaviour {
    [MenuItem("GameObject/ECM/Create Empty Level", false, 0)]
	public static void createEmptyLevel() {
		Instantiate(Resources.Load("Level/Level"));
        GameObject level = GameObject.Find("Level(Clone)");
        level.name = "Level";
        GameObject.Find("ECMMapManager").GetComponent<ECMMapManager>().surface = level.transform.GetChild(1).GetComponent<NavMeshSurface>();
	}
}