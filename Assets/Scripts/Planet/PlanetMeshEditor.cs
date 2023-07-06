#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetMesh))]
public class PlanetMeshEditor : Editor {
    private SerializedProperty radiusProp;
    private SerializedProperty collisionMeshResolutionProp;
    private SerializedProperty updateIntervalProp;
    private SerializedProperty playerProp;
    private SerializedProperty useShadowsProp;
    private SerializedProperty MaterialProp;
    private SerializedProperty resProp;
    private SerializedProperty rangeProp;
    private SerializedProperty floorTagProp;

    private SerializedProperty heightMapProp;
    private SerializedProperty heightMapResolutionProp;
    private SerializedProperty heightMapPowerProp;

    private void OnEnable() {
        radiusProp = serializedObject.FindProperty("radius");
        collisionMeshResolutionProp = serializedObject.FindProperty("collisionMeshResolution");
        updateIntervalProp = serializedObject.FindProperty("updateInterval");
        MaterialProp = serializedObject.FindProperty("Material");
        useShadowsProp = serializedObject.FindProperty("useShadows");
        resProp = serializedObject.FindProperty("res");
        rangeProp = serializedObject.FindProperty("range");
        floorTagProp = serializedObject.FindProperty("floorTag");

        heightMapProp = serializedObject.FindProperty("heightMap");
        heightMapResolutionProp = serializedObject.FindProperty("heightMapResolution");
        heightMapPowerProp = serializedObject.FindProperty("heightMapPower");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(radiusProp);
        EditorGUILayout.PropertyField(collisionMeshResolutionProp);
        EditorGUILayout.PropertyField(updateIntervalProp);
        EditorGUILayout.PropertyField(MaterialProp);
        EditorGUILayout.PropertyField(useShadowsProp);
        EditorGUILayout.PropertyField(resProp);
        EditorGUILayout.PropertyField(rangeProp);
        EditorGUILayout.PropertyField(floorTagProp);

        EditorGUILayout.PropertyField(heightMapProp);
        EditorGUILayout.PropertyField(heightMapResolutionProp);
        EditorGUILayout.PropertyField(heightMapPowerProp);

        EditorGUILayout.Space(); EditorGUILayout.Space();
        PlanetMesh script = (PlanetMesh)target;
        if(GUILayout.Button("Preview Planet Mesh")) {
            script.RenderPreview();
        }   

        serializedObject.ApplyModifiedProperties();  
    }
}
#endif