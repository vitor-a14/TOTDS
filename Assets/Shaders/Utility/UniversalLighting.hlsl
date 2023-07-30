#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
	#if (SHADERPASS != SHADERPASS_FORWARD)
		#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	#endif
#endif

struct UniversalLightingData {
	float3 positionWS;
	float3 normalWS;
	float3 viewDirectionWS;
	float4 shadowCoord;

	float3 albedo;
	float smoothness;
};

float GetSmoothnessPower(float rawSmoothness) {
	return exp2(10 * rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 UniversalLightingHandling(UniversalLightingData d, Light light) {
	float3 radiance = light.color * (light.shadowAttenuation * light.distanceAttenuation);
	float3 lightDirection = normalize(_SunPos - d.positionWS);

	float diffuse = saturate(dot(d.normalWS, lightDirection)); //change here
	float specularDot = saturate(dot(d.normalWS, normalize(lightDirection + d.viewDirectionWS)));
	float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;

	float3 color = d.albedo * radiance * (diffuse + specular);
	return color;
}
#endif

float3 CalculateUniversalLighting(UniversalLightingData d) {
#ifdef SHADERGRAPH_PREVIEW
	float3 lightDir = float3(0.5, 0.5, 0.0);
	float intensity = saturate(dot(d.normalWS, lightDir)) + 
		pow(saturate(dot(d.normalWS, normalize(d.viewDirectionWS + lightDir))), GetSmoothnessPower(d.smoothness));
	return d.albedo * intensity;
#else
	Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);
	float3 color = 0;
	color += UniversalLightingHandling(d, mainLight);

	#ifdef _ADDITIONAL_LIGHTS
		uint numAdditionalLights = GetAdditionalLightsCount();
		for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
			Light light = GetAdditionalLight(lightI, d.positionWS, 1);
			color += UniversalLightingHandling(d, light);
		}
	#endif

	return color;
#endif
}

void CalculateUniversalLighting_float(float3 WorldPos, float3 Albedo, float3 Normal, float3 ViewDirection, float Smoothness, out float3 Color) {
	UniversalLightingData d;
	d.positionWS = WorldPos;
	d.normalWS = Normal;
	d.viewDirectionWS = ViewDirection;
	d.albedo = Albedo;
	d.smoothness = Smoothness;

#ifdef SHADERGRAPH_PREVIEW
	d.shadowCoord = 0;
#else
	float4 positionCS = TransformWorldToHClip(WorldPos);
	#if SHADOWS_SCREEN
		d.shadowCoord = ComputeScreenPos(positionCS);
	#else
		d.shadowCoord = TransformWorldToShadowCoord(WorldPos);
	#endif
#endif

	Color = CalculateUniversalLighting(d);
}

#endif