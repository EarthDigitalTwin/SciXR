// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "SciVR/DataMapUI"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Min("Min(Focus)", Range(0,1)) = 0
		_Max("Max(Scale)", Range(0,1)) = 1
		[Toggle(INVERT)]
		_Invert("Invert Colors", Float) = 0
		[Toggle(COLOR_MODE)]
		_ColorMode("Toggle Color Mode", Float) = 1
		[Toggle(ENABLE_LOG)]
		_EnableLog("Enable Log", Float) = 0

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord  : TEXCOORD0;
			float4 worldPosition : TEXCOORD1;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		fixed4 _Color;
		fixed4 _TextureSampleAdd;
		float4 _ClipRect;

		// TODO: Remove these and replace with features;
		float _Min;
		float _Max;
		half _ColorMode;
		half _EnableLog;
		half _Invert;

		v2f vert(appdata_t v)
		{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
			OUT.worldPosition = v.vertex;
			OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
			OUT.texcoord = v.texcoord;

			OUT.color = v.color * _Color;
			return OUT;
		}

		sampler2D _MainTex;

		fixed4 frag(v2f IN) : SV_Target
		{
			if (_EnableLog == 1) {
				IN.texcoord = log(IN.texcoord.x);
				_Min = log(clamp(_Min, 0.081, 1));
				if (_ColorMode == 1) {
					_Max = log(_Max);
				}
			}

			half scaledVal;
			if (_ColorMode == 1) {
				// Albedo comes from a texture tinted by color
				if (_Min > IN.texcoord.x)
					scaledVal = 0;
				else if (_Max < IN.texcoord.x)
					scaledVal = 1;
				else
					scaledVal = (IN.texcoord.x - _Min) / (_Max - _Min);
			}
			else {
				scaledVal = 1 - abs(IN.texcoord.x - _Min) / (_Max);
			}
			if (_Invert == 1) {
				scaledVal = 1 - scaledVal;
			}
			fixed4 color = tex2D(_MainTex, float2(scaledVal, 0)) + _TextureSampleAdd;
			//half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
			color.a = _Color.a;

			#ifdef UNITY_UI_CLIP_RECT
			color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
			#endif

			#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
			#endif

			return color;
		}
		ENDCG
		}
	}
}
