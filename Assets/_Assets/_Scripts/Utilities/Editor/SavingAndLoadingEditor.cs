using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(SavingAndLoadingManager))]
public class SavingAndLoadingEditor : Editor{

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        SavingAndLoadingManager saveAndLoad = (SavingAndLoadingManager)target;
        if(GUILayout.Button("Save Data")){
            saveAndLoad.SaveGame();
        }
        if(GUILayout.Button("Load Data")){
            saveAndLoad.LoadGame();
        }
        serializedObject.ApplyModifiedProperties();

    }
}
