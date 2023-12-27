using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackCheckVisual))]
public class CheckPointEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        TrackCheckVisual trackCheckVisual = (TrackCheckVisual)target as TrackCheckVisual;
        if(GUILayout.Button("Get Random Visual Color")){
            trackCheckVisual.GetRandomColorForVisual();
        }
    }
}