// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "pfc Dome Tools/DomemasterFisheyeShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	CGINCLUDE
	#pragma multi_compile_local MODE_DOME MODE_DEBUG_STRETCH

	#include "UnityCG.cginc"
	#include "DomemasterInclude.cginc"
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	} 

	float4 frag(v2f i) : SV_Target 
	{
		float2 pos = 2.0 * i.uv - 1.0;

		float lengthFromCenter = length(pos);
		if (lengthFromCenter > 1.0)
			return half4(0, 0, 0, 1);  
		
		float2 uv = GetUV(pos);

		#if MODE_DEBUG_STRETCH
		return GetDebugStretch(uv, _MainTex_TexelSize);
		#endif

		float4 color = tex2D (_MainTex, uv);
		return color;
	}

	ENDCG 
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
  
}

Fallback off
	
} // shader
