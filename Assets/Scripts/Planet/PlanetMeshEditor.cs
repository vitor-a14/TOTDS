#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetMesh))]
public class PlanetMeshEditor : Editor {
    private SerializedProperty radiusProp;
    private SerializedProperty collisionMeshResolutionProp;
    private SerializedProperty updateIntervalProp;
    private SerializedProperty playerProp;
    private SerializedProperty MaterialProp;
    private SerializedProperty resProp;
    private SerializedProperty rangeProp;

    private SerializedProperty terrainGenerationModeProp;
    private SerializedProperty heightMapProp;
    private SerializedProperty heightMapResolutionProp;
    private SerializedProperty heightMapPowerProp;

    private SerializedProperty useHeightmapProp;

    private void OnEnable() {
        radiusProp = serializedObject.FindProperty("radius");
        collisionMeshResolutionProp = serializedObject.FindProperty("collisionMeshResolution");
        updateIntervalProp = serializedObject.FindProperty("updateInterval");
        MaterialProp = serializedObject.FindProperty("Material");
        resProp = serializedObject.FindProperty("res");
        rangeProp = serializedObject.FindProperty("range");

        terrainGenerationModeProp = serializedObject.FindProperty("terrainGenerationMode");
        heightMapProp = serializedObject.FindProperty("heightMap");
        heightMapResolutionProp = serializedObject.FindProperty("heightMapResolution");
        heightMapPowerProp = serializedObject.FindProperty("heightMapPower");
        useHeightmapProp = serializedObject.FindProperty("useHeightmapTexture");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(radiusProp);
        EditorGUILayout.PropertyField(collisionMeshResolutionProp);
        EditorGUILayout.PropertyField(updateIntervalProp);
        EditorGUILayout.PropertyField(MaterialProp);
        EditorGUILayout.PropertyField(resProp);
        EditorGUILayout.PropertyField(rangeProp);

        EditorGUILayout.PropertyField(terrainGenerationModeProp);
        if (terrainGenerationModeProp.intValue == (int)TerrainGenerationMode.HEIGHTMAP_TEXTURE) {
            EditorGUILayout.PropertyField(heightMapProp);
            EditorGUILayout.PropertyField(heightMapResolutionProp);
            EditorGUILayout.PropertyField(heightMapPowerProp);
        } else if (terrainGenerationModeProp.intValue == (int)TerrainGenerationMode.NOISE_CALCULATION) {
            EditorGUILayout.PropertyField(useHeightmapProp);
        }   

        EditorGUILayout.Space(); EditorGUILayout.Space();
        PlanetMesh script = (PlanetMesh)target;
        if(GUILayout.Button("Preview Planet Mesh")) {
            script.RenderPreview();
        }   

        serializedObject.ApplyModifiedProperties();  
    }
}
#endif