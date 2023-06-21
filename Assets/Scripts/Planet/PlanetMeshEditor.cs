#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetMesh))]
public class PlanetMeshEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        EditorGUILayout.Space(); EditorGUILayout.Space();
        PlanetMesh script = (PlanetMesh)target;
        if(GUILayout.Button("Preview Planet Mesh")) {
            script.RenderPreview();
        }     
    }
}
#endif