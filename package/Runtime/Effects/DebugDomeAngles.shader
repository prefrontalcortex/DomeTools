Shader "pfc Dome Tools/DebugDomeAngles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Steps("Steps", Vector) = (10,10,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 pos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Steps;

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float filteredGrid( in float2 p, in float2 dpdx, in float2 dpdy )
            {
                const float N = 30.0;
                float2 w = max(abs(dpdx), abs(dpdy));
                float2 a = p + 0.5*w;                        
                float2 b = p - 0.5*w;           
                float2 i = (floor(a) + min(frac(a) * N, 1.0)-
                          floor(b) - min(frac(b) * N, 1.0)) / (N * w);
                return (1.0-i.x) * (1.0-i.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
				float PI = 3.1415926535;

				float dist = sqrt(i.pos.x * i.pos.x + i.pos.z * i.pos.z);
				float phi = atan2(i.pos.y, dist);

				float a = atan2(i.pos.z, i.pos.x);
				float a_angle = a / PI * 180;
				// return frac(a_angle / 10);

				float p_angle = phi / PI * 180;
				// return frac(p_angle / 10);

                float2 _steps = _Steps.xy;
                _steps.y = _steps.y * 3.6;
                float2 p = i.uv * 1;
                //p = float2(a_angle / 360.0, phi);
                p *= _steps.xy;
                float dx = ddx(p);
                float dy = ddy(p);
                float grid = filteredGrid(p, dx, dy);

                // return float4(1,1,1,1 - grid);
                
                
                float perimeterLines = frac(a_angle * _steps.x / 360); // 10° steps
                float concentricLines = frac(p_angle * _steps.y / 360); // 10° steps
                
                perimeterLines = abs(0.5 - perimeterLines) * 2;
                concentricLines = abs(0.5 - concentricLines) * 2;

                perimeterLines = smoothstep(0.95, 1.0, perimeterLines);
                concentricLines = smoothstep(0.95, 1.0, concentricLines);

                float lines = max(perimeterLines,concentricLines);
                clip(lines - 0.1);
				return lines;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
