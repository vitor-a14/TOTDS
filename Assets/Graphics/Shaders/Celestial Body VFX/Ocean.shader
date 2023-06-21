Shader "Planet/Ocean"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PlanetPosition("Position", Vector) = (0,0,0)
        _Radius("Ocean Radius", float) = 0
        _PlanetRadius("Planet Radius", float) = 20
        _ColorA("Color A", Color) = (1,1,1,1)
        _ColorB("Color B", Color) = (1,1,1,1)
        _DepthMultiplier("Depth Multiplier", float) = 1
        _AlphaMultiplier("Alpha Multiplier", float) = 1
        _Smoothness("Smoothness", float) = 1
        _WaveSpeed("Wave Speed", float) = 1
        _WaveStrength("Wave Strength", float) = 1
        _WaveScale("Wave Scale", float) = 1
        waveNormalA("Wave Normal A", 2D) = "white" {}
        waveNormalB("Wave Normal B", 2D) = "white" {}
    }
    SubShader
    {
        //Cull On ZWrite On ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../Triplanar.cginc"

            static const float maxFloat = 3.402823466e+38;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex     : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                o.uv = v.uv;
                return o;
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

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            sampler2D waveNormalA;
			sampler2D waveNormalB;

            float3 _PlanetPosition;
            float _Radius;
            float4 _ColorA;
            float4 _ColorB;
            float _DepthMultiplier;
            float _AlphaMultiplier;
            float _PlanetRadius;
            float _Smoothness;
            float _WaveSpeed;
            float _WaveScale;
            float _WaveStrength;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float3 rayPos = _WorldSpaceCameraPos;
				float viewLength = length(i.viewVector);
				float3 rayDir = i.viewVector / viewLength;

                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(nonlin_depth) * viewLength;

                float2 hitInfo = raySphere(_PlanetPosition, _Radius, rayPos, rayDir);
				float dstToOcean = hitInfo.x;
				float dstThroughOcean = hitInfo.y;
				float3 rayOceanIntersectPos = rayPos + rayDir * dstToOcean - _PlanetPosition;

				float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

                //return sceneDepth / 100000;

                if (oceanViewDepth > 0) 
                {
                    float opticalDepth01 = 1 - exp(-oceanViewDepth / _PlanetRadius * _DepthMultiplier);
                    float alpha = 1 - exp(-oceanViewDepth / _PlanetRadius * _AlphaMultiplier);
                    float3 oceanNormal = normalize(rayPos + rayDir * dstToOcean - _PlanetPosition);

                    //Wave normals
                    float2 waveOffsetA = float2(_Time.x * _WaveSpeed, _Time.x * _WaveSpeed * 0.8);
					float2 waveOffsetB = float2(_Time.x * _WaveSpeed * - 0.8, _Time.x * _WaveSpeed * -0.3);
					float3 waveNormal = triplanarNormal(rayOceanIntersectPos, oceanNormal, _WaveScale / _PlanetRadius, waveOffsetA, waveNormalA);
					waveNormal = triplanarNormal(rayOceanIntersectPos, waveNormal, _WaveScale / _PlanetRadius, waveOffsetB, waveNormalB);
					waveNormal = normalize(lerp(oceanNormal, waveNormal, _WaveStrength));

                    //Light calculation
                    float3 dirToSun = normalize(_WorldSpaceLightPos0.xyz);
                    float specularAngle = acos(dot(normalize(dirToSun - rayDir), waveNormal));
                    float specularExponent = specularAngle / (1 - _Smoothness);
                    float specularHighlight = exp(-specularExponent * specularExponent);
                    float diffuseLighting = saturate(dot(oceanNormal, dirToSun));

                    float4 oceanColor = lerp(_ColorA, _ColorB, opticalDepth01) * diffuseLighting + specularHighlight;
					return lerp(col, oceanColor, alpha);
				}

                return col;
            }
            ENDCG
        }
    }
}
