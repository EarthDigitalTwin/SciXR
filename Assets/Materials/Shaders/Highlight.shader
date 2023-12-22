Shader "SciVR/Highlight"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_SeeThru("SeeThru", Range(0.0, 1.0)) = 0.25
		_Darken("Darken", Range(0.0, 1.0)) = 0.0
		_MainTex("MainTex", 2D) = "white" {}
	}

	CGINCLUDE

	#pragma target 3.5
	//#pragma only_renderers d3d11 vulkan
	#pragma exclude_renderers gles

	#include "UnityCG.cginc"

	struct VertexInput
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		fixed4 color : COLOR;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct VertexOutput
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		fixed4 color : COLOR;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	// Globals --------------------------------------------------------------------------------------------------------------------------------------------------
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _Color;
	float _SeeThru;
	float _Darken;

	VertexOutput MainVS(VertexInput i)
	{
		VertexOutput o;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		#if UNITY_VERSION >= 540
			o.vertex = UnityObjectToClipPos(i.vertex);
		#else
			o.vertex = UnityObjectToClipPos(i.vertex);
		#endif
		o.uv = TRANSFORM_TEX(i.uv, _MainTex);
		o.color = i.color;

		return o;
	}

	float4 MainPS(VertexOutput i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		float4 vTexel = tex2D(_MainTex, i.uv).rgba;
		float4 vColor = vTexel.rgba * _Color.rgba * i.color.rgba;
		vColor.rgba = saturate(2.0 * vColor.rgba);
		float flAlpha = vColor.a;

		vColor.rgb *= vColor.a;
		vColor.a = lerp(0.0, _Darken, flAlpha) * _SeeThru;
		return vColor.rgba;
	}

	float4 SeeThruPS(VertexOutput i) : SV_Target
	{
		float4 vTexel = tex2D(_MainTex, i.uv).rgba;
		float4 vColor = vTexel.rgba * _Color.rgba * i.color.rgba * _SeeThru;
		vColor.rgba = saturate(2.0 * vColor.rgba);
		float flAlpha = vColor.a;

		vColor.rgb *= vColor.a;
		vColor.a = lerp(0.0, _Darken, flAlpha ) * _SeeThru;
		return vColor.rgba;
	}

	ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100


			Pass
		{
			Blend One OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest Greater

			CGPROGRAM
			#pragma vertex MainVS
			#pragma fragment SeeThruPS
			ENDCG
		}

			Pass
		{
			Blend One OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#pragma vertex MainVS
			#pragma fragment MainPS
			ENDCG
		}

	}
}
