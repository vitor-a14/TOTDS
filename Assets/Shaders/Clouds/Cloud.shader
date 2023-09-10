Shader "Planet/Cloud"
{
    Properties
    {
        _TopTexture("Top Texture", 2D) = "white" {}
        _Tess("Tessellation", Range(1, 32)) = 20
        _MaxTessDistance("Max Tess Distance", Range(1, 32)) = 20
		_Fallof("Fallof", Float) = 0
        _Tiling("Tiling", Vector) = (0,0,0,0)
		_VertexColorMult("Vertex Color Mult", Float) = 1
        _TextureColor("Texture Color", Color) = (0,0,0,0)
        _textureDetail("textureDetail", Range( 0 , 1)) = 0
        _FresnelBSP("FresnelBSP", Vector) = (0,0,0,0)
        _RimColor("Rim Color", Color) = (0,0,0,0)

        _NoiseScaleA("NoiseScale A", Vector) = (1,1,1,0)
		_3dNoiseSizeA("3dNoise Size A", Float) = 0
        _NoiseStrengthA("Noise Strength A", Float) = 0
        _SpeedA("Speed A", Float) = 0
		_DirectionA("DirectionA", Vector) = (1,0,0,0)

        _NoiseScaleB("NoiseScale B", Vector) = (1,1,1,0)
        _3dNoiseSizeB("3dNoise Size B", Float) = 0
        _NoiseStrengthB("Noise Strength B", Float) = 0
        _SpeedB("Speed B", Float) = 0
        _DirectionB("DirectionB", Vector) = (1,0,0,0)

        _NoiseScaleC("NoiseScale C", Vector) = (1,1,1,0)
		_3dNoiseSizeC("3dNoise Size C", Float) = 0
        _NoiseStrengthC("Noise Strength C", Float) = 0
        _SpeedC("Speed C", Float) = 0
        _DirectionC("DirectionC", Vector) = (1,0,0,0)
    }
 
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalRenderPipeline" 
        }
 
        Pass
        {
        Tags{ "LightMode" = "UniversalForward" }
 
        Blend SrcAlpha OneMinusSrcAlpha
        
        HLSLPROGRAM

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"    
        #include "CustomTessellation.hlsl"
        
        #pragma require tessellation
        #pragma vertex TessellationVertexProgram
        #pragma fragment frag
        #pragma hull hull
        #pragma domain domain

        sampler2D _Noise;
        float _Weight;
        float noiseCo;
        float3 _SunPos; // update globally

		uniform float _3dNoiseSizeA;
		uniform float3 _NoiseScaleA;
        uniform float _3dNoiseSizeB;
		uniform float3 _NoiseScaleB;
        uniform float _3dNoiseSizeC;
		uniform float3 _NoiseScaleC;

        uniform float _NoiseStrengthA;
        uniform float _SpeedA;
        uniform float3 _DirectionA;

        uniform float _NoiseStrengthB;
        uniform float _SpeedB;
        uniform float3 _DirectionB;

        uniform float _NoiseStrengthC;
        uniform float _SpeedC;
        uniform float3 _DirectionC;

        uniform float2 _Tiling;
		uniform float _Fallof;
        uniform float _VertexColorMult;
		uniform float4 _TextureColor;
        uniform float _textureDetail;
        uniform float3 _FresnelBSP;
		uniform float4 _RimColor;

        TEXTURE2D(_TopTexture); SAMPLER(sampler_TopTexture); float4 _TopTexture_ST;

        float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

        float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}

        float4 TriplanarSamplingSF(Texture2D topTexMap, SamplerState topTexSampler, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index)
        {
            float3 projNormal = pow(abs(worldNormal), falloff);
            projNormal /= dot(projNormal, float3(1.0, 1.0, 1.0)) + 0.00001;
            float3 nsign = sign(worldNormal);
            float4 xNorm;
            float4 yNorm;
            float4 zNorm;
            xNorm = SAMPLE_TEXTURE2D(topTexMap, topTexSampler, tiling * worldPos.zy * float2(nsign.x, 1.0));
            yNorm = SAMPLE_TEXTURE2D(topTexMap, topTexSampler, tiling * worldPos.xz * float2(nsign.y, 1.0));
            zNorm = SAMPLE_TEXTURE2D(topTexMap, topTexSampler, tiling * worldPos.xy * float2(-nsign.z, 1.0));
            
            float4 result = xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
            
            return result;
        }

        float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}

        // pre tesselation vertex program
        ControlPoint TessellationVertexProgram(Attributes v)
        {
            ControlPoint p;
    
            p.vertex = v.vertex;
            p.uv = v.uv;
            p.normal = v.normal;
            p.color = v.color;
    
            return p;
        }
    
        // after tesselation
        Varyings vert(Attributes input)
        {
            Varyings output;

            float time = _Time.y * 0.05;
            float2 offset = float2(time, 0.0);
        
            // Apply the offset to the vertex coordinates
            float3 objToWorldDir239 = mul(unity_ObjectToWorld, float4(float3(0,1,0), 0)).xyz;
			float mulTime15 = _Time.y * _SpeedA;
			float3 ase_worldPos = mul(unity_ObjectToWorld, input.vertex);
			float simplePerlin3D3 = snoise(((_DirectionA * mulTime15) + (_3dNoiseSizeA * (ase_worldPos * _NoiseScaleA))));
			float temp_output_8_0 = (0.0 + (simplePerlin3D3 - -1.0) * (1.0 - 0.0) / (1.0 - -1.0));
			float3 objToWorldDir213 = mul(unity_ObjectToWorld, float4( float3(0,1,0), 0 )).xyz;
			float mulTime75 = _Time.y * _SpeedB;
			float simplePerlin3D78 = snoise(((_DirectionB * mulTime75) + (_3dNoiseSizeB * (ase_worldPos * _NoiseScaleB))));
			float3 objToWorldDir257 = mul(unity_ObjectToWorld, float4(float3(0,1,0), 0)).xyz;
			float mulTime248 = _Time.y * _SpeedC;
			float3 temp_output_252_0 = ((_DirectionC * mulTime248) + (_3dNoiseSizeC * (ase_worldPos * _NoiseScaleC)));
			float simplePerlin3D255 = snoise(temp_output_252_0);
			input.vertex.xyz += ((objToWorldDir239 * _NoiseStrengthA * temp_output_8_0) + (objToWorldDir213 * (0.0 + (simplePerlin3D78 - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) * _NoiseStrengthB ) + ( objToWorldDir257 * (0.0 + (simplePerlin3D255 - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) * _NoiseStrengthC ));

            output.vertex = TransformObjectToHClip(input.vertex.xyz);
            output.color = input.color;
            output.normal = input.normal;
            output.uv = input.uv;

            return output;
        }
 
        [UNITY_domain("tri")]
        Varyings domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
        {
            Attributes v;
    
            #define DomainPos(fieldName) v.fieldName = \
                patch[0].fieldName * barycentricCoordinates.x + \
                patch[1].fieldName * barycentricCoordinates.y + \
                patch[2].fieldName * barycentricCoordinates.z;
    
            DomainPos(vertex)
            DomainPos(uv)
            DomainPos(color)
            DomainPos(normal)

            return vert(v);
        }
         
        half4 frag(Varyings IN) : SV_Target
        {
            float3 ase_worldPos = mul(unity_ObjectToWorld, IN.vertex);
			float3 ase_worldNormal = IN.normal; 
			float mulTime248 = _Time.y * _SpeedC;
			float3 temp_output_252_0 = (( _DirectionC * mulTime248 ) + (_3dNoiseSizeC * (ase_worldPos * _NoiseScaleC )));
			float3 NoiseWorldPos364 = temp_output_252_0;
			float4 triplanar194 = TriplanarSamplingSF(_TopTexture, sampler_TopTexture, NoiseWorldPos364, ase_worldNormal, _Fallof, _Tiling, 1.0, 0);

            float mulTime15 = _Time.y * _SpeedA;
			float simplePerlin3D3 = snoise(((_DirectionA * mulTime15) + (_3dNoiseSizeA * (ase_worldPos * _NoiseScaleA ))));
			float temp_output_8_0 = (0.0 + (simplePerlin3D3 + 1.0) * (1.0 - 0.0) / (1.0 + 1.0));
			float NoiseA366 = temp_output_8_0;
			float2 appendResult376 = (float2(ase_worldPos.x, ase_worldPos.z));
			float simplePerlin2D374 = snoise((appendResult376 * 0.1));
			float4 lerpResult210 = lerp(saturate((pow(IN.color, 0.4) * _VertexColorMult)), _TextureColor, (((1.0 - triplanar194.x) * _textureDetail) * saturate(NoiseA366) * (0.25 + (simplePerlin2D374 + 1.0) * 0.75 / 2)));

            float3 ase_worldViewDir = normalize(_WorldSpaceCameraPos - ase_worldPos);
			float fresnelNdotV346 = saturate(dot(ase_worldNormal, ase_worldViewDir));
			float fresnelNode346 = (_FresnelBSP.x + _FresnelBSP.y * pow(fresnelNdotV346, _FresnelBSP.z));

            return half4(saturate(lerpResult210 + (fresnelNode346 * _RimColor)).rgb, 1.0);
        }

        ENDHLSL
        }
    }
}