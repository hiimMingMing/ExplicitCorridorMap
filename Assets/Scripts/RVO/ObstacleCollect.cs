//using System.Collections;
//using System.Collections.Generic;
//using RVO;
//using UnityEngine;
//using Vector2 = RVO.Vector2;

//public class ObstacleCollect : MonoBehaviour
//{
//    void Awake()
//    {
//        BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>();
//        for (int i = 0; i < boxColliders.Length; i++)
//        {
//            float minX, minY, maxX, maxY;
//            if (GameMainManager.Instance.is3D)
//            {
//                minX = boxColliders[i].transform.position.x -
//                       boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
//                minY = boxColliders[i].transform.position.z -
//                             boxColliders[i].size.z * boxColliders[i].transform.lossyScale.z * 0.5f;
//                maxX = boxColliders[i].transform.position.x +
//                             boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
//                maxY = boxColliders[i].transform.position.z +
//                             boxColliders[i].size.z * boxColliders[i].transform.lossyScale.z * 0.5f;
//            }
//            else {
//                minX = boxColliders[i].transform.position.x -
//                       boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
//                minY = boxColliders[i].transform.position.y -
//                             boxColliders[i].size.y * boxColliders[i].transform.lossyScale.y * 0.5f;
//                maxX = boxColliders[i].transform.position.x +
//                             boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
//                maxY = boxColliders[i].transform.position.y +
//                             boxColliders[i].size.y * boxColliders[i].transform.lossyScale.y * 0.5f;
//            }
          

//            IList<Vector2> obstacle = new List<Vector2>();
//            obstacle.Add(new Vector2(maxX, maxY));
//            obstacle.Add(new Vector2(minX, maxY));
//            obstacle.Add(new Vector2(minX, minY));
//            obstacle.Add(new Vector2(maxX, minY));
//            Simulator.Instance.addObstacle(obstacle);
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//    }
//}