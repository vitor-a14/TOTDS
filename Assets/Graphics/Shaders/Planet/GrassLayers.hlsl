#ifndef GRASSLAYERS_INCLUDED
#define GRASSLAYERS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "NMGGrassLayersHelpers.hlsl"

#define GRASS_LAYERS 16

struct Attributes 
{
    float4 positionOS   : POSITION; 
    float3 normalOS     : NORMAL; 
    float4 tangentOS    : TANGENT; 
    float2 uv           : TEXCOORD0;
};

struct VertexOutput 
{
    float3 positionWS   : TEXCOORD0;
    float3 normalWS     : TEXCOORD1;
    float2 uv           : TEXCOORD2;
    float3 positionOS   : TEXCOORD3;
};

struct GeometryOutput 
{
    float3 uv           : TEXCOORD0;
    float3 positionWS   : TEXCOORD1;
    float3 normalWS     : TEXCOORD2;
    float3 positionOS   : TEXCOORD3;
    float4 positionCS   : SV_POSITION; 
};

TEXTURE2D(_FloorTexture); SAMPLER(sampler_FloorTexture); float4 _FloorTexture_ST;
TEXTURE2D(_SteepTexture); SAMPLER(sampler_SteepTexture); float4 _SteepTexture_ST;
TEXTURE2D(_SteepNoise); SAMPLER(sampler_SteepNoise); float4 _SteepNoise_ST;

TEXTURE2D(_DetailNoiseTexture); SAMPLER(sampler_DetailNoiseTexture); float4 _DetailNoiseTexture_ST;
TEXTURE2D(_SmoothNoiseTexture); SAMPLER(sampler_SmoothNoiseTexture); float4 _SmoothNoiseTexture_ST;
TEXTURE2D(_WindNoiseTexture); SAMPLER(sampler_WindNoiseTexture); float4 _WindNoiseTexture_ST;

float4 _BaseColor;
float4 _TopColor;
float4 _SteepColor;
float _TotalHeight; 
float _ColorStrength;

float _FlatToSteepBlend;
float _SteepnessThreshold;
float _SteepNoiseStrength;

float _DetailDepthScale;
float _SmoothDepthScale;

float _WindTimeMult;
float _WindAmplitude;

float _LODDistance;
float _LODFactor;

VertexOutput Vertex(Attributes input) 
{
    VertexOutput output = (VertexOutput)0;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.positionWS = vertexInput.positionWS;
    output.normalWS = normalInput.normalWS;
    
    output.positionOS = input.positionOS.xyz;
    output.uv = input.uv;
    return output;
}

void SetupVertex(in VertexOutput input, inout GeometryOutput output, float height) 
{
    float3 positionWS = input.positionWS + input.normalWS * (height * _TotalHeight);

    output.positionWS = positionWS;
    output.normalWS = input.normalWS;
    output.uv = float3(input.uv, height); 
    output.positionCS = CalculatePositionCSWithShadowCasterLogic(positionWS, input.normalWS);
    output.positionOS = input.positionOS;
}

int GetNumLayer(VertexOutput a, VertexOutput b, VertexOutput c)
{
    float dA = distance(a.positionWS, _WorldSpaceCameraPos);
    float dB = distance(b.positionWS, _WorldSpaceCameraPos);
    float dC = distance(c.positionWS, _WorldSpaceCameraPos);

    float d = min(dA, min(dB, dC));
    d = 1 - smoothstep(0, _LODDistance, d);
    d = pow(d, _LODFactor);
    return max(1, ceil(d * GRASS_LAYERS));
}

[maxvertexcount(3 * GRASS_LAYERS)]
void Geometry(triangle VertexOutput inputs[3], inout TriangleStream<GeometryOutput> outputStream) 
{
    GeometryOutput output = (GeometryOutput)0;
    float colorOffset = 0;

    for (int t = 0; t < 3; t++) 
    {
        SetupVertex(inputs[t], output, 0);
        output.uv.z = 0;
        outputStream.Append(output);
    }

    outputStream.RestartStrip();
            
    if(distance(output.positionWS, _WorldSpaceCameraPos) < _LODDistance * 1.5)
    {
        for (int l = 1; l < GRASS_LAYERS; l++) 
        {
            float h = l / (float)GRASS_LAYERS;

            for (int t = 0; t < 3; t++) 
            {
                SetupVertex(inputs[t], output, h);
                float colorHeight = (float) (colorOffset + l) / (GRASS_LAYERS - 1);
                output.uv.z = colorHeight;
                outputStream.Append(output);
            }

            outputStream.RestartStrip();
        }
    }
}

float blend(float startHeight, float blendDst, float height) {
	 return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
}

half4 Fragment(GeometryOutput input) : SV_Target 
{
    // planet surface
    float3 sphereNormal = normalize(input.positionWS - mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz);
    float steepness = 1 - dot (sphereNormal, input.normalWS);
    steepness = saturate(steepness / _SteepnessThreshold);

    float2 steepNoiseUV = TRANSFORM_TEX(input.uv.xy, _SteepNoise);
    float3 steepNoise = SAMPLE_TEXTURE2D(_SteepNoise, sampler_SteepNoise, steepNoiseUV).rgb;
    float flatBlendNoise = (steepNoise.r) * _SteepNoiseStrength;
    
    float flatStrength = 1 - blend(_SteepnessThreshold + flatBlendNoise, _FlatToSteepBlend, steepness);

    float2 flatColorUV = TRANSFORM_TEX(input.uv.xy, _FloorTexture);
    float3 flatColor = SAMPLE_TEXTURE2D(_FloorTexture, sampler_FloorTexture, flatColorUV).rgb * _BaseColor;

    float2 steepColorUV = TRANSFORM_TEX(input.uv.xy, _SteepTexture);
    float3 steepColor = SAMPLE_TEXTURE2D(_SteepTexture, sampler_SteepTexture, steepColorUV).rgb * _SteepColor;

    float3 surfaceColor = lerp(steepColor, flatColor, flatStrength); 

    // grass
    float dist = distance(input.positionWS, _WorldSpaceCameraPos) / _LODDistance;
    float height = input.uv.z;

    float2 windUV = TRANSFORM_TEX(input.uv.xy, _WindNoiseTexture) + _Time.y * _WindTimeMult;
    float2 windNoise = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, windUV).xy * 2 - 1;
    float2 uv = input.uv.xy + windNoise * (_WindAmplitude * height);

    float detailNoise = SAMPLE_TEXTURE2D(_DetailNoiseTexture, sampler_DetailNoiseTexture, TRANSFORM_TEX(uv, _DetailNoiseTexture)).r * flatStrength;
    float smoothNoise = SAMPLE_TEXTURE2D(_SmoothNoiseTexture, sampler_SmoothNoiseTexture, TRANSFORM_TEX(uv, _SmoothNoiseTexture)).r;

    detailNoise = 1 - (1 - detailNoise) * _DetailDepthScale;
    smoothNoise = 1 - (1 - smoothNoise) * _SmoothDepthScale;
    clip(detailNoise * smoothNoise - height);

    InputData lightingInput = (InputData)0;
    lightingInput.positionWS = input.positionWS;
    lightingInput.normalWS = NormalizeNormalPerPixel(input.normalWS); 
    lightingInput.viewDirectionWS = GetViewDirectionFromPosition(input.positionWS); 
    lightingInput.shadowCoord = CalculateShadowCoord(input.positionWS, input.positionCS);
    
    float maxDistance = 60.0;
    float dist2 = distance(input.positionWS, _WorldSpaceCameraPos);
    float fadeOut = 1.0 - smoothstep(maxDistance - 30.0, maxDistance, dist2);

    //result
    float3 albedo = lerp(surfaceColor, _TopColor, pow(height * fadeOut, _ColorStrength)).rgb;

    SurfaceData surfaceInput = (SurfaceData)0;
    surfaceInput.albedo = albedo;  

    return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
}

#endif