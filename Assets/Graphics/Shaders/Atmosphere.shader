Shader "Planet/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlueNoise ("Blue Noise", 2D) = "white" {}
        _PlanetCenter ("Planet Position", Vector) = (0,0,0)
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

            float3 _PlanetCenter;
            float _AtmosphereRadius;
            float _PlanetRadius;
            float _OceanRadius;
            float _DensityFalloff;
            int _ScatteringPoints;
            int _OpticalDepthPoints;
            float3 _SunDir;
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
                float heightAboveSurface = length(densitySamplePoint - _PlanetCenter) - _PlanetRadius;
                float height01 = heightAboveSurface / (_AtmosphereRadius - _PlanetRadius);
                float localDensity = exp(-height01 * _DensityFalloff) * (1 - height01);

                return localDensity;
            }

            float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 densitySamplePoint = rayOrigin;
                float stepSize = rayLength / (_OpticalDepthPoints - 1);
                float opticalDepth = 0;

                for(int i = 0; i < _OpticalDepthPoints; i++)
                {
                    float localDensity = densityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity * stepSize;
                    densitySamplePoint += rayDir * stepSize;
                }

                return opticalDepth;
            }

            float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 col, float2 uv)
            {
                _SunDir = normalize(_WorldSpaceLightPos0.xyz);
                float blueNoise = tex2Dlod(_BlueNoise, float4(squareUV(uv) * _DitheringScale,0,0));
				blueNoise = (blueNoise - 0.5) * _DitheringStrength;

                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (_ScatteringPoints - 1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                for(int i = 0; i < _ScatteringPoints; i++)
                {
                    float sunRayLength = raySphere(_PlanetCenter, _AtmosphereRadius, inScatterPoint, _SunDir).y;
                    float sunRayOpticalDepth = opticalDepth(inScatterPoint, _SunDir * _DitheringStrength, sunRayLength);
                    viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i);
                    float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * scatteringCoefficients);
                    float localDensity = densityAtPoint(inScatterPoint);

                    inScatteredLight += localDensity * transmittance * scatteringCoefficients * stepSize;
                    inScatterPoint += rayDir * stepSize;
                }
                inScatteredLight += blueNoise * 0.01;

                float colTransmittance = exp(-viewRayOpticalDepth);
                return col * colTransmittance + inScatteredLight;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float dstToOcean = raySphere(_PlanetCenter, _OceanRadius, rayOrigin, rayDir);
				float dstToSurface = min(sceneDepth, dstToOcean);

                float2 hitInfo = raySphere(_PlanetCenter, _AtmosphereRadius, rayOrigin, rayDir);
                float distanceToAtmosphere = hitInfo.x;
                float distanceThroughAtmosphere = min(hitInfo.y, dstToSurface - distanceToAtmosphere);

                //return sceneDepth / 100000;

                if(distanceThroughAtmosphere > 0)
                {
                    const float epsilon = 0.0001;
                    float3 pointInAtmosphere = rayOrigin + rayDir * (distanceToAtmosphere + epsilon);
                    float3 light = calculateLight(pointInAtmosphere, rayDir, distanceThroughAtmosphere - epsilon * 2, col, i.uv);
                    return float4(light, 0);
                }

                return col;
            }
            ENDCG
        }
    }
}
