using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(GameSettingsSO))]
public class GameSettingsEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GameSettingsSO trackCheckVisual = (GameSettingsSO)target as GameSettingsSO;
        if(GUILayout.Button("Save Data")){
            trackCheckVisual.Save();
        }
        if(GUILayout.Button("Load Data")){
            trackCheckVisual.Load();
        }
    }
}