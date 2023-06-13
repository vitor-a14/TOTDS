Shader "Grass/GrassLayers" 
{
    Properties 
    {
        _FloorTexture("Floor Texture", 2D) = "white" {}
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
            #pragma target 3.0
            #pragma require geometry
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
            #pragma multi_compile_shadowcaster
            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment
            #include "GrassLayers.hlsl"
            ENDHLSL
        }
    }
}