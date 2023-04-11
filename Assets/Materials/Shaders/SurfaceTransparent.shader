Shader "SciVR/SurfaceTrans" {
	Properties{
			_Color("Color", Color) = (1,1,1,1)
			_MainTex("Albedo (RGB)", 2D) = "white" {}
			_BumpMap("Normal Map", 2D) = "bump" {}
			_BumpScale("Normal Scale", Range(0,3)) = 1
			_OcclusionStrength("Occlusion Strength", Range(0.0, 5.0)) = 1.0
			//_OcclusionMap("Occlusion", 2D) = "white" {}
			_EmissionMap("Emission", 2D) = "black" {}
			_EmissionColor("Emission Color", Color) = (0,0,0)
			_Glossiness("Smoothness", Range(0,1)) = 0.5
			_SpecMap("Specular Map", 2D) = "white" {}
			_SpecScale("Specular Scale", Range(0,20)) = 1
	}
	SubShader{
		Pass{
			//ZWrite On
			ColorMask 0
		}
	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

	LOD 200
	CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;

		struct Input {
			float2 uv_MainTex;
		};

		half _BumpScale;
		half _Glossiness;
		half _SpecScale;
		half _OcclusionStrength;
		fixed4 _EmissionColor;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
			o.Emission = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor * c.a;
			o.Specular = half3(clamp(c.r * _SpecScale, 0, 1), clamp(c.g * _SpecScale, 0, 1), clamp(c.b * _SpecScale, 0, 1));
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Occlusion = _OcclusionStrength;
		}
	ENDCG
	}
	FallBack "Diffuse"
}