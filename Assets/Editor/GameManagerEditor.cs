//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(GameMainManager))]
//public class GameManagerEditor : Editor
//{
//    GameMainManager manager;
    
//    public override void OnInspectorGUI()
//    {
//        if (manager.isNull()) return;
//        base.OnInspectorGUI();
//        DrawSettingEditor(manager.agentSetting);
//        var gameManager = (GameMainManager)target;
//        var radiusList = gameManager.defaultECMMap.AgentRadiusList;
//        var options = new string[radiusList.Count];
//        for (int i = 0; i < radiusList.Count; i++)
//        {
//            options[i] = radiusList[i].ToString();
//        }

//        gameManager.agentSetting.radiusIndex = EditorGUILayout.Popup("Radius", gameManager.agentSetting.radiusIndex, options);
//        gameManager.agentSetting.radius = radiusList[gameManager.agentSetting.radiusIndex];
//    }

//    void DrawSettingEditor(Object setting) {
//        Editor editor = CreateEditor(setting);
//        editor.OnInspectorGUI();
//    }

//    private void OnEnable() {
//        manager = (GameMainManager)target;
//    }
//}
