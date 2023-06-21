#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetEffects))]
public class PlanetEffectsEditor : Editor {
    private SerializedProperty rendererDataProp;
    private SerializedProperty hasAtmosphereProp;
    private SerializedProperty hasOceanProp;
    private SerializedProperty planetRadiusProp;
    private SerializedProperty sunTransformProp;

    private SerializedProperty atmosphereShaderProp;
    private SerializedProperty blueNoiseProp;
    private SerializedProperty atmosphereScaleProp;
    private SerializedProperty densityFalloffProp;
    private SerializedProperty scatteringPointsProp;
    private SerializedProperty opticalDepthPointsProp;
    private SerializedProperty waveLenghtsProp;

   	private SerializedProperty scatteringStrengthProp;
	private SerializedProperty ditheringStrengthProp;
	private SerializedProperty ditheringScaleProp;

    private SerializedProperty opticalDepthComputeProp;
    private SerializedProperty opticalDepthTextureProp;
    private SerializedProperty textureSizeProp;

    private SerializedProperty oceanShaderProp;
    private SerializedProperty oceanScaleProp;
	public SerializedProperty highColorProp;
	public SerializedProperty lowColorProp;
	public SerializedProperty depthMultiplierProp;
	public SerializedProperty alphaMultiplierProp;
	public SerializedProperty smoothnessProp;
	public SerializedProperty waveSpeedProp;
	public SerializedProperty waveStrengthProp;
	public SerializedProperty waveScaleProp;
	public SerializedProperty waveNormalAProp;
	public SerializedProperty waveNormalBProp;


    private void OnEnable()
    {
        rendererDataProp = serializedObject.FindProperty("rendererData");
        hasAtmosphereProp = serializedObject.FindProperty("hasAtmosphere");
        hasOceanProp = serializedObject.FindProperty("hasOcean");
        planetRadiusProp = serializedObject.FindProperty("planetRadius");
        sunTransformProp = serializedObject.FindProperty("sunTransform");
        
        //atmosphere
        atmosphereShaderProp = serializedObject.FindProperty("atmosphereShader");
        blueNoiseProp = serializedObject.FindProperty("blueNoise");
        atmosphereScaleProp = serializedObject.FindProperty("atmosphereScale");
        densityFalloffProp = serializedObject.FindProperty("densityFalloff");
        scatteringPointsProp = serializedObject.FindProperty("scatteringPoints");
        opticalDepthPointsProp = serializedObject.FindProperty("opticalDepthPoints");
        waveLenghtsProp = serializedObject.FindProperty("waveLenghts");
        opticalDepthComputeProp = serializedObject.FindProperty("opticalDepthCompute");
        textureSizeProp = serializedObject.FindProperty("textureSize");

        scatteringStrengthProp = serializedObject.FindProperty("scatteringStrength");
        ditheringStrengthProp = serializedObject.FindProperty("ditheringStrength");
        ditheringScaleProp = serializedObject.FindProperty("ditheringScale");

        //ocean
        oceanShaderProp = serializedObject.FindProperty("oceanShader");
        oceanScaleProp = serializedObject.FindProperty("oceanScale");
        highColorProp = serializedObject.FindProperty("highColor");
        lowColorProp = serializedObject.FindProperty("lowColor");
        depthMultiplierProp = serializedObject.FindProperty("depthMultiplier");
        alphaMultiplierProp = serializedObject.FindProperty("alphaMultiplier");
        smoothnessProp = serializedObject.FindProperty("smoothness");
        waveSpeedProp = serializedObject.FindProperty("waveSpeed");
        waveStrengthProp = serializedObject.FindProperty("waveStrength");
        waveScaleProp = serializedObject.FindProperty("waveScale");
        waveNormalAProp = serializedObject.FindProperty("waveNormalA");
        waveNormalBProp = serializedObject.FindProperty("waveNormalB");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(rendererDataProp);
        EditorGUILayout.PropertyField(planetRadiusProp);
        EditorGUILayout.PropertyField(sunTransformProp);
        EditorGUILayout.Space(); EditorGUILayout.Space();

        EditorGUILayout.PropertyField(hasAtmosphereProp);
        if (hasAtmosphereProp.boolValue)
        {
            EditorGUILayout.PropertyField(atmosphereShaderProp);
            EditorGUILayout.PropertyField(blueNoiseProp);
            EditorGUILayout.PropertyField(atmosphereScaleProp);
            EditorGUILayout.PropertyField(densityFalloffProp);
            EditorGUILayout.PropertyField(scatteringPointsProp);
            EditorGUILayout.PropertyField(opticalDepthPointsProp);
            EditorGUILayout.PropertyField(waveLenghtsProp);
            EditorGUILayout.PropertyField(scatteringStrengthProp);
            EditorGUILayout.PropertyField(ditheringStrengthProp);
            EditorGUILayout.PropertyField(ditheringScaleProp);
            EditorGUILayout.PropertyField(opticalDepthComputeProp);
            EditorGUILayout.PropertyField(textureSizeProp);

            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        EditorGUILayout.PropertyField(hasOceanProp);
        if (hasOceanProp.boolValue)
        {
            EditorGUILayout.PropertyField(oceanShaderProp);
            EditorGUILayout.PropertyField(oceanScaleProp);
            EditorGUILayout.PropertyField(highColorProp);
            EditorGUILayout.PropertyField(lowColorProp);
            EditorGUILayout.PropertyField(depthMultiplierProp);
            EditorGUILayout.PropertyField(alphaMultiplierProp);
            EditorGUILayout.PropertyField(smoothnessProp);
            EditorGUILayout.PropertyField(waveSpeedProp);
            EditorGUILayout.PropertyField(waveStrengthProp);
            EditorGUILayout.PropertyField(waveScaleProp);
            EditorGUILayout.PropertyField(waveNormalAProp);
            EditorGUILayout.PropertyField(waveNormalBProp);

            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        PlanetEffects script = (PlanetEffects)target;
        EditorGUILayout.Space(); EditorGUILayout.Space();
        if(GUILayout.Button("Preview Planet Effects")) {
            script.PreviewEffects();
        }     

        serializedObject.ApplyModifiedProperties();
    }
}
#endif