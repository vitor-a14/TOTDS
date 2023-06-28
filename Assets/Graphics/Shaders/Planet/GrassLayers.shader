Shader "Grass/GrassLayers" 
{
    Properties 
    {
        _FloorTexture("Floor Texture", 2D) = "white" {}
        _SteepTexture("Steep Texture", 2D) = "white" {}
        _SteepNoise("Steep Noise", 2D) = "white" {}
        _ShoreTexture("Shore Texture", 2D) = "white" {}
        _FlatToSteepBlend("Flat To Steep Blend", Float) = 1 
        _SteepnessThreshold("Steepness Threshold", Float) = 1 
        _SteepNoiseStrength("Steep Noise Strength", Float) = 1 
        _SteepColor("Top color", Color) = (0, 1, 0, 1) 

        _ShoreHeight("Shore Height", Float) = 1 
        _ShoreBlend("Shore Blend", Float) = 1 

        _ColorStrength("Color strength", Float) = 1 
        _BaseColor("Base color", Color) = (0, 0.5, 0, 1) 
        _TopColor("Top color", Color) = (0, 1, 0, 1) 
        _TotalHeight("Grass height", Float) = 1 
        _DetailNoiseTexture("Grainy noise", 2D) = "white" {} 
        _DetailDepthScale("Grainy depth scale", Range(0, 1)) = 1
        _SmoothNoiseTexture("Smooth noise", 2D) = "white" {} 
        _SmoothDepthScale("Smooth depth scale", Range(0, 1)) = 1
        _WindNoiseTexture("Wind noise texture", 2D) = "white" {}
        _WindTimeMult("Wind frequency", Float) = 1 
        _WindAmplitude("Wind strength", Float) = 1 

        _LODDistance("LOD Distance", Float) = 5
        _LODFactor("LOD Factor", Float) = 1
    }
    SubShader 
    {
        Tags{"RenderType" = "Opaque"}

        Pass 
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0
            #pragma require geometry

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment

            #include "GrassLayers.hlsl"    
            ENDHLSL
        }

        Pass 
        {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0
            #pragma require geometry

            #pragma multi_compile_shadowcaster
            #pragma editor_sync_compilation
            #pragma NoSelfShadow

            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment

            #include "GrassLayers.hlsl"
            ENDHLSL
        }


        Pass 
        {
            Name "DepthOnly"
            Tags {"LightMode" = "DepthNormals"}

            HLSLPROGRAM
            #pragma target 3.0
            #pragma require geometry
            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment
            #include "GrassLayers.hlsl"
            ENDHLSL
        }
    }
}