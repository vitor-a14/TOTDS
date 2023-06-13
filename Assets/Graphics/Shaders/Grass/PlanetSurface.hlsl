#ifndef PLANETSURFACE_INCLUDED
#define PLANETSURFACE_INCLUDED

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
};

VertexOutput Vertex(Attributes input) 
{
    VertexOutput output = (VertexOutput)0;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.positionWS = vertexInput.positionWS;
    output.normalWS = normalInput.normalWS;

    output.uv = input.uv;
    return output;
}

half4 Fragment(VertexOutput input) : SV_Target 
{
    return half4(1,1,1,1);
}

#endif