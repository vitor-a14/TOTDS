using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetMesh))]
public class PlanetMeshEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        PlanetMesh script = (PlanetMesh)target;
        if(GUILayout.Button("Preview Planet Mesh")) {
            script.RenderPreview();
        }     
    }
}