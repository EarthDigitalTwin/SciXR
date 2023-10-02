Shader "SciVR/Standard (Separate Emission)" {
	Properties{
			_Color("Color", Color) = (1,1,1,1)
			_MainTex("Albedo (RGB)", 2D) = "white" {}
			_BumpMap("Normal Map", 2D) = "bump" {}
			_BumpScale("Normal Scale", Range(0,3)) = 1
			_OcclusionStrength("Occlusion Strength", Range(0.0, 5.0)) = 1.0
			_EmissionMap("Emission", 2D) = "white" {}
			_EmissionColor("Emission Color", Color) = (0,0,0)
			_Metallic("Metallic", Range(0,1)) = 0.5
			_Smoothness("Smoothness", Range(0,1)) = 0.5
	}
	SubShader{
		Pass{
			//ZWrite On
			ColorMask 0
		}
	Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

	LOD 200
	CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows// alpha:fade

		// Use shader model 3.5 target, to get nicer looking lighting
		#pragma target 3.5

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionMap;
		};

		half _BumpScale;
		half _Metallic;
		half _Smoothness;
		half _SpecScale;
		half _OcclusionStrength;
		fixed4 _EmissionColor;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
			o.Emission = tex2D(_EmissionMap, IN.uv_EmissionMap) * _EmissionColor * c.a;

			//o.Specular = half3(clamp(c.r * _SpecScale, 0, 1), clamp(c.g * _SpecScale, 0, 1), clamp(c.b * _SpecScale, 0, 1));
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = c.a;
			o.Occlusion = _OcclusionStrength;
		}
	ENDCG
	}
	FallBack "Diffuse"
}