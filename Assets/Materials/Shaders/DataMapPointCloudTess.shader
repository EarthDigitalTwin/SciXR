// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// PointCloudMeshColors Alpha

Shader "SciVR/DataMapPointCloudTess"
{
	Properties
	{

		_Color("Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_NoiseScale ("Noise Scale", Range(0,0.03)) = 0.01
		_VertNoiseScale ("Vert Noise Scale", Range(0,0.03)) = 0.01
		_Size ("Point Size", Range(0,0.003)) = 0.003

		_ColorMode("Color Mode", Float) = 1

		// Possible values:
		// 0 - No Clip
		// 1 - Min/Max Clip
		// 2 - Plane Clip
		// 3 - Radius Clip
		_ClipMode("Clip Mode", Float) = 0
		_Min("Min(Focus)", Range(0,1)) = 0
		_Max("Max(Scale)", Range(0,1)) = 1
		[Toggle(INVERT)]
		_Invert("Invert Colors", Float) = 0

		[Toggle(ENABLE_LOG)]
		_EnableLog("Enable Log", Float) = 0

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

		SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
		Blend SrcAlpha OneMinusSrcAlpha     // Alpha blending
		Fog { Mode Off }

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma hull tessBase
            #pragma domain basicDomain
			//#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NoiseTex;
			float _NoiseScale;
			float _VertNoiseScale;
			float _Size;
			fixed4 _Color;

			float _Alpha;
			fixed _AniLerp;
			float _Min;
			float _Max;
			half _Glossiness;
			half _Metallic;
			fixed4 _EdgeColor;
			half _EdgeDistance;
			float3 _PlanePoint;
			float3 _PlaneNormal;

			// TODO: Remove these and replace with features;
			half _ColorMode;
			half _EnableLog;
			half _ClipMode;
			half _Invert;
			half _ClearHalf;

			// TODO: Copied from editorVR
			float4 _ClipCenter;
			float4 _ClipExtents;
			float4x4 _ClipMatrix;


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 wpos: TEXCOORD3;
				fixed4 color : COLOR;
				float2 uv_MainTex : TEXCOORD0;
				float3 clipPos : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};


			
            #ifdef UNITY_CAN_COMPILE_TESSELLATION
           
            struct inputControlPoint{
                float4 position : WORLDPOS;
                float4 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
            };
           
            struct outputControlPoint{
                float3 position : BEZIERPOS;            
            };
           
            struct outputPatchConstant{
                float edges[3]        : SV_TessFactor;
                float inside        : SV_InsideTessFactor;
               
                float3 vTangent[4]    : TANGENT;
                float2 vUV[4]         : TEXCOORD;
                float3 vTanUCorner[4] : TANUCORNER;
                float3 vTanVCorner[4] : TANVCORNER;
                float4 vCWts          : TANWEIGHTS;
            };
           
           
            outputPatchConstant patchConstantThing(InputPatch<inputControlPoint, 3> v){
                outputPatchConstant o;
               
                o.edges[0] = 1;
                o.edges[1] = 1;
                o.edges[2] = 1;
                o.inside = 1;
               
                return o;
            }
           
            // tessellation hull shader
            [domain("tri")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("patchConstantThing")]
            [outputcontrolpoints(3)]
            inputControlPoint tessBase (InputPatch<inputControlPoint,3> v, uint id : SV_OutputControlPointID) {
                return v[id];
            }
           
            #endif // UNITY_CAN_COMPILE_TESSELLATION


			v2f vert(appdata v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				v.vertex.x =  (v.vertex.x - _NoiseScale * 0.25) + clamp(tex2Dlod(_NoiseTex, float4(v.uv + v.vertex.z,0,0)) * _NoiseScale, -0.101, 0.101);
				v.vertex.y =  (v.vertex.y - _NoiseScale * 0.25) + clamp(tex2Dlod(_NoiseTex, float4(v.uv + 0.5,0,0)) * _NoiseScale, -0.101, 0.101);
				v.vertex.z =  (v.vertex.z - _VertNoiseScale * 0.25) + clamp(tex2Dlod(_NoiseTex, float4(v.uv + 0.25,0,0)) * _VertNoiseScale, -0.101, 0.101);
				o.pos = v.vertex;//mul(unity_ObjectToWorld, v.vertex);
				o.wpos = mul(unity_ObjectToWorld, v.vertex);
				//o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color*_Color;
				o.uv_MainTex = TRANSFORM_TEX(lerp(v.uv1, v.uv, _AniLerp), _MainTex);
				//o.wpos = mul(unity_ObjectToWorld, v.vertex);
				o.clipPos = mul(_ClipMatrix, mul(unity_ObjectToWorld, v.vertex));
				return o;
			}
			
			v2f displace (appdata_tan v){
                v2f o;        
               
                o.uv_MainTex = TRANSFORM_TEX (v.texcoord, _MainTex);
               
                float localTex = tex2Dlod(_MainTex, float4(o.uv_MainTex,0,0)).r;
                v.vertex.y += localTex.r * 0.001;
               
			   	//o.pos = v.vertex;//mul(unity_ObjectToWorld, v.vertex);
				o.wpos = mul(unity_ObjectToWorld, v.vertex);
				//o.pos = UnityObjectToClipPos(v.vertex);
				//o.color = v.color*_Color;
				//o.uv_MainTex = TRANSFORM_TEX(lerp(v.uv1, v.uv, _AniLerp), _MainTex);
				//o.wpos = mul(unity_ObjectToWorld, v.vertex);
				o.clipPos = mul(_ClipMatrix, mul(unity_ObjectToWorld, v.vertex));

                o.pos = UnityObjectToClipPos(v.vertex);
               
                return o;
            }
           
            #ifdef UNITY_CAN_COMPILE_TESSELLATION
           
            // tessellation domain shader
            [domain("tri")]
            v2f basicDomain (outputPatchConstant tessFactors, const OutputPatch<inputControlPoint,3> vi, float3 bary : SV_DomainLocation) {
                appdata_tan v;
                v.vertex = vi[0].position*bary.x + vi[1].position*bary.y + vi[2].position*bary.z;
                v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
                v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
                v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
                v2f o = displace( v);
//                v2f o = vert_surf (v);
                return o;
            }
           
            #endif // UNITY_CAN_COMPILE_TESSELLATION

			half4 frag(v2f IN) : COLOR
			{
				// Clip if position is outside of clip bounds
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				float3 diff = abs(IN.clipPos - _ClipCenter);
				if (diff.x > _ClipExtents.x || diff.y > _ClipExtents.y || diff.z > _ClipExtents.z)
					clip(-1);

				if (_EnableLog == 1) {
					IN.uv_MainTex.x = log(IN.uv_MainTex.x);
					_Min = log(clamp(_Min, 0.081, 1));
					if (_ColorMode == 1) {
						_Max = log(_Max);
					}
				}

				half scaledVal;
				if (_ColorMode == 1) {
					// Albedo comes from a texture tinted by color
					if (_Min > IN.uv_MainTex.x)
						scaledVal = 0;
					else if (_Max < IN.uv_MainTex.x)
						scaledVal = 1;
					else
						scaledVal = (IN.uv_MainTex.x - _Min) / (_Max - _Min);
				}
				else {
					scaledVal = 1 - abs(IN.uv_MainTex.x - _Min) / (_Max);
				}

				fixed4 c;
				if (_Invert == 1) {
					c = tex2D(_MainTex, float2(1 - scaledVal, IN.uv_MainTex.y));
				}
				else {
					c = tex2D(_MainTex, float2(scaledVal, IN.uv_MainTex.y));
				}

				if (_ClearHalf == 1 && scaledVal > 0.4999f && scaledVal < 0.5001f) {
					if (_ClipMode == 1) {
						clip(-1);
						return c;
					}
					return fixed4(1, 1, 1, 1);
				}

				// Clipping

				// Default box clipping first
				if (_ClipMode == 1) {
					if (_ColorMode == 1) {
						clip(IN.uv_MainTex.x - _Min);
						clip(_Max - IN.uv_MainTex.x);
					}
					else if (_ColorMode == 0) {
						clip((_Max)-abs(IN.uv_MainTex.x - _Min));
					}

				}
				
				if (_ClipMode == 2) {
					//TODO Redo this with transform matrix instead
					_PlaneNormal = normalize(_PlaneNormal);
					half dist = (IN.wpos.x * _PlaneNormal.x) + (IN.wpos.y * _PlaneNormal.y) + (IN.wpos.z * _PlaneNormal.z)
						- (_PlanePoint.x * _PlaneNormal.x) - (_PlanePoint.y * _PlaneNormal.y) - (_PlanePoint.z * _PlaneNormal.z)
						/ sqrt(pow(_PlaneNormal.x, 2) + pow(_PlaneNormal.y, 2) + pow(_PlaneNormal.z, 2));
					clip(dist);
					//if (dist > _EdgeDistance) {
					//	return c;
					//}
					//else {
					//	return float4(1,1,1,1);
					//}
				}
				else if (_ClipMode == 3) {
					// Radius Clip
					float dist = distance(IN.wpos, _PlanePoint);
					clip(dist - 0.3f);
				}
				//c.a = IN.uv_MainTex.x;
				return c;
			}
			ENDCG
		}
	}
		Fallback Off
}
