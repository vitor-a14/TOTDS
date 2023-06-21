Shader "Planet/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlueNoise ("Blue Noise", 2D) = "white" {}
        _OpticalDepthTexture ("Optical Depth Texture", 2D) = "white" {}
        _PlanetPosition ("Planet Position", Vector) = (0,0,0)
        _SunDir ("Sun Direction", Vector) = (0,0,0)
        _AtmosphereRadius ("Atmosphere Radius", float) = 0
        _PlanetRadius ("Planet Radius", float) = 0
        _OceanRadius ("Ocean Radius", float) = 0
        _DensityFalloff ("Density Falloff", float) = 0
        _ScatteringPoints ("Scattering Points", int) = 4
        _OpticalDepthPoints ("Optical Depth Points", int) = 4
        _WaveLengths("Wave Lengths", Vector) = (730, 540, 440)
        _ScaterringStrength("Scaterring Strength", float) = 1
        _DitheringStrength("Dithering Strength", float) = 1
        _DitheringScale("Dithering Scale", float) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            static const float maxFloat = 3.402823466e+38;

            float3 _PlanetPosition;
            float3 _SunDir;
            float _AtmosphereRadius;
            float _PlanetRadius;
            float _OceanRadius;
            float _DensityFalloff;
            int _ScatteringPoints;
            int _OpticalDepthPoints;
            float _DitheringStrength;
            float _DitheringScale;

            float3 _WaveLengths;
            float _ScaterringStrength;
            static const float scatterR = pow(400 / _WaveLengths.x, 4) * _ScaterringStrength;
            static const float scatterG = pow(400 / _WaveLengths.y, 4) * _ScaterringStrength;
            static const float scatterB = pow(400 / _WaveLengths.z, 4) * _ScaterringStrength;
            static const float3 scatteringCoefficients = float3(scatterR, scatterG, scatterB);

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _FarCameraDepth;
            sampler2D _BakedOpticalDepth;
            sampler2D _BlueNoise;
            sampler2D _OpticalDepthTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv         : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                float4 vertex     : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }

            float2 squareUV(float2 uv) {
				float width = _ScreenParams.x;
				float height =_ScreenParams.y;
				float scale = 1000;
				float x = uv.x * width;
				float y = uv.y * height;
				return float2 (x/scale, y/scale);
			}

            float2 raySphere(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir)
            {
                float3 offset = rayOrigin - sphereCenter;
                float a = 1;
                float b = 2 * dot(offset, rayDir);
                float c = dot(offset, offset) - sphereRadius * sphereRadius;
                float d = b * b - 4 * a * c;

                if(d > 0)
                {
                    float s = sqrt(d);
                    float distanceNear = max(0, (-b -s) / (2 * a));
                    float distanceFar = (-b + s) / (2 * a);

                    if(distanceFar >= 0)
                    {
                        return float2(distanceNear, distanceFar - distanceNear);
                    }
                }

                return float2(maxFloat, 0);
            }

            float densityAtPoint(float3 densitySamplePoint)
            {
                float scale = (_AtmosphereRadius + 1) * _PlanetRadius;
                float heightAboveSurface = length(densitySamplePoint - _PlanetPosition) - _PlanetRadius;
                float height01 = heightAboveSurface / (scale - _PlanetRadius);
                float localDensity = exp(-height01 * _DensityFalloff) * (1 - height01);

                return localDensity;
            }

            float opticalDepthBaked(float3 rayOrigin, float3 rayDir) {
                float scale = (_AtmosphereRadius + 1) * _PlanetRadius;

				float height = length(rayOrigin - _PlanetPosition) - _PlanetRadius;
				float height01 = saturate(height / (scale - _PlanetRadius));
				float uvX = 1 - (dot(normalize(rayOrigin - _PlanetPosition), rayDir) * .5 + .5);

				return tex2Dlod(_OpticalDepthTexture, float4(uvX, height01,0,0));
			}

			float opticalDepthBaked2(float3 rayOrigin, float3 rayDir, float rayLength) {
				float3 endPoint = rayOrigin + rayDir * rayLength;
				float d = dot(rayDir, normalize(rayOrigin - _PlanetPosition));
				float opticalDepth = 0;

				const float blendStrength = 1.5;
				float w = saturate(d * blendStrength + .5);
				
				float d1 = opticalDepthBaked(rayOrigin, rayDir) - opticalDepthBaked(endPoint, rayDir);
				float d2 = opticalDepthBaked(endPoint, -rayDir) - opticalDepthBaked(rayOrigin, -rayDir);

				opticalDepth = lerp(d2, d1, w);
				return opticalDepth;
			}

            float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 col, float2 uv)
            {
                float blueNoise = tex2Dlod(_BlueNoise, float4(squareUV(uv) * _DitheringScale,0,0));
				blueNoise = (blueNoise - 0.5) * _DitheringStrength;

                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (_ScatteringPoints - 1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                for(int i = 0; i < _ScatteringPoints; i++)
                {
                    float scale = (_AtmosphereRadius + 1) * _PlanetRadius;

                    float sunRayLength = raySphere(_PlanetPosition, scale, inScatterPoint, _SunDir).y;
					float sunRayOpticalDepth = opticalDepthBaked(inScatterPoint + _SunDir * _DitheringStrength, _SunDir);
                    float localDensity = densityAtPoint(inScatterPoint);
                    viewRayOpticalDepth = opticalDepthBaked2(rayOrigin, rayDir, stepSize * i);
					float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * scatteringCoefficients);

					inScatteredLight += localDensity * transmittance;
					inScatterPoint += rayDir * stepSize;
                }

                const float intensity = 1;
				inScatteredLight *= scatteringCoefficients * intensity * stepSize / _PlanetRadius;
				inScatteredLight += blueNoise * 0.01;

				// Attenuate brightness of original col (i.e light reflected from planet surfaces)
				// This is a hacky mess, TODO: figure out a proper way to do this
				const float brightnessAdaptionStrength = 0.15;
				const float reflectedLightOutScatterStrength = 3;
				float brightnessAdaption = dot (inScatteredLight,1) * brightnessAdaptionStrength;
				float brightnessSum = viewRayOpticalDepth * intensity * reflectedLightOutScatterStrength + brightnessAdaption;
				float reflectedLightStrength = exp(-brightnessSum);
				float hdrStrength = saturate(dot(col,1)/3-1);
				reflectedLightStrength = lerp(reflectedLightStrength, 1, hdrStrength);
				float3 reflectedLight = col * reflectedLightStrength;

				float3 finalCol = reflectedLight + inScatteredLight;

				
				return finalCol;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float scale = (_AtmosphereRadius + 1) * _PlanetRadius;

                fixed4 col = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float dstToOcean = raySphere(_PlanetPosition, _OceanRadius, rayOrigin, rayDir);
				float dstToSurface = min(sceneDepth, dstToOcean);

                float2 hitInfo = raySphere(_PlanetPosition, scale, rayOrigin, rayDir);
                float distanceToAtmosphere = hitInfo.x;
                float distanceThroughAtmosphere = min(hitInfo.y, dstToSurface - distanceToAtmosphere);

                if(distanceThroughAtmosphere > 0)
                {
                    const float epsilon = 0.0001;
                    float3 pointInAtmosphere = rayOrigin + rayDir * (distanceToAtmosphere + epsilon);
                    float3 light = calculateLight(pointInAtmosphere, rayDir, distanceThroughAtmosphere - epsilon * 2, col, i.uv);
                    return float4(light, 1);
                }

                return col;
            }
            ENDCG
        }
    }
}
