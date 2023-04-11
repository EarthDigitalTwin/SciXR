// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// PointCloudMeshColors Alpha

Shader "SciVR/DataMapPointCloudFancy"
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

		_MinMaxClip("Clip Mode", Float) = 0

		// Possible values:
		// 0 - No Clip
		// 1 - Plane Clip
		// 2 - Radius Clip
		_ExtraClipMode("Clip Mode", Float) = 2
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
			#pragma geometry geom
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
			half _MinMaxClip;
			half _ExtraClipMode;
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

			struct v2g
			{
				float4 pos : SV_POSITION;
				float4 wpos: TEXCOORD3;
				fixed4 color : COLOR;
				float2 uv_MainTex : TEXCOORD0;
				float3 clipPos : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv_MainTex : TEXCOORD0;
				float3 clipPos : TEXCOORD2;
				float4 wpos: TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2g vert(appdata v)
			{
				v2g o = (v2g)0;
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
			

			[maxvertexcount(24)] 
             // ----------------------------------------------------
             // Using "point" type as input, not "triangle"
             void geom2(point v2g vert[1], inout TriangleStream<g2f> triStream)
             {                    
				_Size = 0.002;
                 float f = _Size/2; //half size
                 
				const float4 vc[24] = { 
						float4( -f,  f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f,  f, -f, 0.0f), float4( -f,  f, -f, 0.0f),    //Top
                                          
                        float4(  f,  f, -f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f, -f,  f, 0.0f), float4(  f, -f, -f, 0.0f),    //Right
                                          
                        float4( -f,  f, -f, 0.0f), float4(  f,  f, -f, 0.0f), float4(  f, -f, -f, 0.0f), float4( -f, -f, -f, 0.0f),    //Front
                                          
                        float4( -f, -f, -f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f, -f,  f, 0.0f), float4( -f, -f,  f, 0.0f),    //Bottom
                                          
                        float4( -f,  f,  f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f, -f, -f, 0.0f), float4( -f, -f,  f, 0.0f),    //Left
                                          
                        float4( -f,  f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4(  f, -f,  f, 0.0f), float4(  f,  f,  f, 0.0f)    //Back
                };
                                          
                                                             
                 g2f v[24];
                 int i;
                 
                 // Assign new vertices positions 
                 for (i=0;i<24;i++) { 
					UNITY_INITIALIZE_OUTPUT(g2f, v[i]);
					
					v[i].uv_MainTex = vert[0].uv_MainTex;
					
					v[i].clipPos = vert[0].clipPos; 
					v[i].color = vert[0].color; 
					
					v[i].pos = UnityObjectToClipPos(vert[0].pos + vc[i]); 
				}

                 // Build the cube tile by submitting triangle strip vertices
                 for (i=0;i<24/4;i++)
                 { 
                     triStream.Append(v[i*4+0]);
                     triStream.Append(v[i*4+1]);
                     triStream.Append(v[i*4+2]);    
                     triStream.Append(v[i*4+3]);              
                     triStream.RestartStrip();
                 }

			}

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
			{
			/*
				float4 v[4];
				float size = 0.01;
				v[0] = float4(p[0].pos + float4(size, -size, 0, 1));
				v[1] = float4(p[0].pos + float4(size, size, 0, 1));
				v[3] = float4(p[0].pos + float4(-size, size, 0, 1));
				v[2] = float4(p[0].pos + float4(-size, -size, 0, 1));
				*/

				/*
				float3 cameraUp = UNITY_MATRIX_IT_MV[1].xyz;
				float3 cameraForward =  _WorldSpaceCameraPos - p[0].wpos;

				//float3 up = UNITY_MATRIX_IT_MV[1].xyz;
				//float3 right = -UNITY_MATRIX_IT_MV[0].xyz;
				float3 right = normalize(cross(cameraUp, cameraForward));
				float4 v[4];
				_Size = 0.002;
				v[0] = float4(p[0].pos + _Size * right - _Size * cameraUp, 1.0f);
				v[1] = float4(p[0].pos + _Size * right + _Size * cameraUp, 1.0f);
				v[2] = float4(p[0].pos - _Size * right - _Size * cameraUp, 1.0f);
				v[3] = float4(p[0].pos - _Size * right + _Size * cameraUp, 1.0f);
				*/

					float3 up = float3(0, 1, 0);
					float3 look = mul(unity_WorldToObject, _WorldSpaceCameraPos - p[0].wpos);
					//look.y = 0;
					look = normalize(look);
					float3 right = cross(up, look);
					//_Size = 0.003;
					float halfS = 0.5f * _Size;
							
					float4 v[4];
					v[0] = float4(p[0].pos + (halfS * right) - (halfS * up), 1.0f);
					v[1] = float4(p[0].pos + (halfS * right) + (halfS * up), 1.0f);
					v[2] = float4(p[0].pos - (halfS * right) - (halfS * up), 1.0f);
					v[3] = float4(p[0].pos - (halfS * right) + (halfS * up), 1.0f);

				g2f newVert;
				UNITY_INITIALIZE_OUTPUT(g2f, newVert);
				newVert.uv_MainTex = p[0].uv_MainTex;
				newVert.clipPos = p[0].clipPos;
				newVert.wpos = p[0].wpos;
				newVert.color = p[0].color;

				newVert.pos = UnityObjectToClipPos(v[0]);
				triStream.Append(newVert);
				
				newVert.pos =  UnityObjectToClipPos(v[1]);
				triStream.Append(newVert);

				newVert.pos =  UnityObjectToClipPos(v[2]);
				triStream.Append(newVert);

				newVert.pos =  UnityObjectToClipPos(v[3]);
				triStream.Append(newVert);									
			}
			


			half4 frag(g2f IN) : COLOR
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
					if (_MinMaxClip == 1) {
						clip(-1);
						return c;
					}
					return fixed4(1, 1, 1, 1);
				}

				// Clipping

				// Default box clipping first
				if (_MinMaxClip == 1) {
					if (_ColorMode == 1) {
						clip(IN.uv_MainTex.x - _Min);
						clip(_Max - IN.uv_MainTex.x);
					}
					else if (_ColorMode == 0) {
						clip((_Max)-abs(IN.uv_MainTex.x - _Min));
					}

				}
				
				if (_ExtraClipMode == 1) {
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
				else if (_ExtraClipMode == 2) {
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
