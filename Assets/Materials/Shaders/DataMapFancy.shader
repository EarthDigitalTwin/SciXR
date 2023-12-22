Shader "SciVR/DataMapFancy" {
	Properties { 
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		
		[Toggle(MINMAX_CLIP)]
		_MinMaxClip("Min Max Clip Mode", Float) = 0


		// Possible values:
		// 0 - No Clip
		// 1 - Plane Clip
		// 2 - Radius Clip
		_ExtraClipMode("Extra Clip Mode", Float) = 0


		_Min("Min(Focus)", Range(0,1)) = 0
		_Max("Max(Scale)", Range(0,1)) = 1

		[Toggle(COLOR_SQUASH)]
		_ColorSquash("Color Squash", Float) = 1

		[Toggle(INVERT)]
		_Invert("Invert Colors", Float) = 0

		[Toggle(ENABLE_LOG)]
		_EnableLog("Enable Log", Float) = 0

		[Toggle(CLEAR_HALF)]
		_ClearHalf("Clear at 0.5 Values", Float) = 0

		_AniLerp("Animation Lerp", Range(0,1)) = 0
		// Outline props
		_EdgeColor("Edge Color", Color) = (1,0,0,1)
		_EdgeDistance ("Edge Distance", float) = 0.01
		//Hidden Slicing Props
		_PlanePoint("Plane Point", Vector) = (0,0,0,0)
		_PlaneNormal("Plane Normal", Vector) = (1,0,0,0)
		_ClipCenter("Clip Center", Vector) = (0,0,0,0)
		_ClipExtents("Clip Extents", Vector) = (1,1,1,1)
	}

CGINCLUDE
#include "UnityCG.cginc"
sampler2D _MainTex;

struct Input {
	float2 uv_MainTex;
	float3 worldPos;
	float3 worldRefl;
	float3 worldNormal;
	float3 clipPos;
	float4 color: Color; // Vertex color
	INTERNAL_DATA
};

float _Alpha;
fixed _AniLerp;
float _Min;
float _Max;
half _Glossiness;
half _Metallic;
fixed4 _Color;
fixed4 _EdgeColor;
half _EdgeDistance;
float3 _PlanePoint;
float3 _PlaneNormal;

// TODO: Remove these and replace with features;
half _EnableLog;
half _MinMaxClip;
half _ColorSquash;
half _ExtraClipMode;
half _Invert;
half _ClearHalf;

// TODO: Copied from editorVR
float4 _ClipCenter;
float4 _ClipExtents;
float4x4 _ClipMatrix;


void vert(inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input, o);
	o.clipPos = mul(_ClipMatrix, mul(unity_ObjectToWorld, v.vertex));
	v.texcoord = lerp(v.texcoord1, v.texcoord, _AniLerp);
}

float scale_value(float startingValue, float min, float max) {
	float scaledVal = startingValue;
	if(_ColorSquash == 1 || _MinMaxClip == 0) {
		// Albedo comes from a texture tinted by color
		if (min > startingValue)
			scaledVal = 0;
		else if (max < startingValue)
			scaledVal = 1;
		else
			scaledVal = (startingValue - min) / (max - min);
	}
	else if(_EnableLog) {
		scaledVal = -(startingValue - log(0.001)) / (log(0.001) - log(1.0));
	}
	return scaledVal;
}

void surf(Input IN, inout SurfaceOutputStandard o) {
	// Clip if position is outside of clip bounds
	float3 diff = abs(IN.clipPos - _ClipCenter);
	if (diff.x > _ClipExtents.x || diff.y > _ClipExtents.y || diff.z > _ClipExtents.z)
		clip(-1);
	
	float inputVal = IN.uv_MainTex.x;
	float inputMin = _Min;
	float inputMax = _Max;
	if (_EnableLog == 1) {
		inputVal = log(IN.uv_MainTex.x);
		inputMin = log(clamp(_Min, 0.001, 1));
		inputMax = log(_Max);
	}

	float scaledVal = scale_value(inputVal, inputMin, inputMax);
	fixed4 c;
	if (_Invert == 1) {
		c = tex2D(_MainTex, float2(1 - scaledVal, IN.uv_MainTex.y)) * _Color;
	}
	else {
		c = tex2D(_MainTex, float2(scaledVal, IN.uv_MainTex.y)) * _Color;
	}

	if (_ClearHalf == 1 && scaledVal > 0.4999f && scaledVal < 0.5001f) {
		if (_MinMaxClip == 1) {
			clip(-1);
			return;
		}
		c = fixed4(1, 1, 1, 1);
		o.Metallic = 0;
		o.Smoothness = 0;

		o.Alpha = c.a;
		o.Albedo = c.rgb;
		return;
	}

	// Clipping

	// Default box clipping first
	if (_MinMaxClip == 1) {
			clip(inputVal - inputMin);
			clip(inputMax - inputVal);
	}
	if (_ExtraClipMode == 1) {
		_PlaneNormal = normalize(_PlaneNormal);
		half dist = (IN.worldPos.x * _PlaneNormal.x) + (IN.worldPos.y * _PlaneNormal.y) + (IN.worldPos.z * _PlaneNormal.z)
			- (_PlanePoint.x * _PlaneNormal.x) - (_PlanePoint.y * _PlaneNormal.y) - (_PlanePoint.z * _PlaneNormal.z)
			/ sqrt(pow(_PlaneNormal.x, 2) + pow(_PlaneNormal.y, 2) + pow(_PlaneNormal.z, 2));
		clip(dist);

		if (dist < _EdgeDistance) {
			o.Albedo = _EdgeColor.rgb;
			return;
		}
		else {
			o.Albedo = float3(1,1,1);//c.rgb;
		}
	}
	else if (_ExtraClipMode == 2) {
		// Radius Clip
		float dist = distance(IN.worldPos, _PlanePoint);
		clip(dist - 0.3f);
	}
	// Metallic and smoothness come from slider variables
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
	
	o.Alpha = c.a;// c.a; //* IN.color.a;
	o.Albedo = c.rgb; // *reflection.rgb;
}



ENDCG

	SubShader {
		Tags { "Queue" = "Geometry"  "RenderType" = "Opaque"}
		//Lighting Off
		LOD 200

		//Cull front
		//ZWrite on
		//ZTest less
		CGPROGRAM
			#pragma surface surf Standard vertex:vert //fullforwardshadows// alpha
			#pragma target 3.5
			#pragma shader_feature ENABLE_LOG
			#pragma shader_feature COLOR_MODE
			#pragma shader_feature INVERT
		ENDCG
	}
	
	Fallback "Diffuse"
}