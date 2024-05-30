Shader "pfc Dome Tools/Unlit Display Dome"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MipBias("Mip Bias", Range(-2,1)) = -0.5
        _Brightness("Brightness", Float) = 1
        [Toggle(ENABLE_SMAA)]
        _SMAA("Enable SMAA", Float) = 0
        
    }
    SubShader
    {        
    	PackageRequirements
        {
            "com.unity.render-pipelines.universal": "10.0.0"
        }
    	
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

		// Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma shader_feature ENABLE_SMAA

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				#if UNITY_ANY_INSTANCING_ENABLED
				uint instanceID : INSTANCEID_SEMANTIC;
            	#endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float fogCoords : TEXCOORD1;
                float4 vertex : SV_POSITION;
				float3 pos : TEXCOORD2;
            	
	            #if UNITY_ANY_INSTANCING_ENABLED
	            uint instanceID : CUSTOM_INSTANCE_ID;
	            #endif
	            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
	            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
	            #endif
	            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
	            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
	            #endif            	
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Brightness;
            float _MipBias;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                // UNITY_TRANSFER_INSTANCE_ID(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex);
				o.pos = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.fogCoords = ComputeFogFactor (o.vertex.z);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
				// float PI = 3.1415926535;

				float dist = sqrt(i.pos.x * i.pos.x + i.pos.z * i.pos.z);
				float phi = atan2(i.pos.y, dist);

				float a = atan2(i.pos.z, i.pos.x);
				float a_angle = a / PI * 180;
				// return frac(a_angle / 10);

				float p_angle = phi / PI * 180;
				// return frac(p_angle / 10);

				// return frac(a_angle / 10) * frac(p_angle / 10);

				// sample the texture
                float2 uv = float2(0.5, 0.5) + float2(sin(a), cos(a)) * (0.5 - phi / PI);
                
                
                // mip bias
                // https://medium.com/@bgolus/sharper-mipmapping-using-shader-based-supersampling-ed7aadb47bec
				
                #ifndef ENABLE_SMAA
                _MipBias = 0;
                #endif

                #ifndef ENABLE_SMAA
                // single biased sample
                half4 col = tex2Dbias(_MainTex, float4(uv, 0, _MipBias));
                #else
                // SMAA with bias
                // per pixel partial derivatives
                float2 dx = ddx(uv);
                float2 dy = ddy(uv);
                // rotated grid uv offsets
                float2 uvOffsets = float2(0.125, 0.375);
                float4 offsetUV = float4(0.0, 0.0, 0.0, _MipBias);
                // supersampled using 2x2 rotated grid
                half4 col = 0;
                offsetUV.xy = uv + uvOffsets.x * dx + uvOffsets.y * dy;
                col += tex2Dbias(_MainTex, offsetUV);
                offsetUV.xy = uv - uvOffsets.x * dx - uvOffsets.y * dy;
                col += tex2Dbias(_MainTex, offsetUV);
                offsetUV.xy = uv + uvOffsets.y * dx - uvOffsets.x * dy;
                col += tex2Dbias(_MainTex, offsetUV);
                offsetUV.xy = uv - uvOffsets.y * dx + uvOffsets.x * dy;
                col += tex2Dbias(_MainTex, offsetUV);
                col *= 0.25;
                #endif

				// apply fog
				col.rgb = MixFog (col.rgb * _Brightness, i.fogCoords);
            	col.a = 1;
				return col;// * _Brightness;
            }
            ENDHLSL
        }
    }
}
