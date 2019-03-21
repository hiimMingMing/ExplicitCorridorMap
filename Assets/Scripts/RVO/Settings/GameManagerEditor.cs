using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameMainManager))]
public class GameManagerEditor : Editor
{
    GameMainManager manager;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawSettingEditor(manager.agentSetting);
    }

    void DrawSettingEditor(Object setting) {
        Editor editor = CreateEditor(setting);
        editor.OnInspectorGUI();
    }

    private void OnEnable() {
        manager = (GameMainManager)target;
    }
}
