// Cartoon FX
// (c) 2015, Jean Moreno

Shader "Hidden/Cartoon FX/Particles"
{
	Properties
	{
		_BaseColor ("Alpha Blended Color", Color) = (0.5,0.5,0.5,0.5)
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}
	
	Category
	{
		SubShader
		{
			
	//========================================================================================================================================================================================
			
			Pass
			{
				Name "ADDITIVE"
				
				Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha One
				
				ColorMask RGB
				Cull Off
				Lighting Off
				ZWrite Off
				Fog { Color (0,0,0,0) }
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"
				
				sampler2D _MainTex;
				fixed4 _TintColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD1;
					#endif
				};
				
				float4 _MainTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
				
				sampler2D _CameraDepthTexture;
				float _InvFade;
				
				fixed4 frag (v2f i) : COLOR
				{
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif
					
					return 2.0 * i.color * _TintColor * tex2D(_MainTex, i.texcoord).a;
				}
				ENDCG 
			}
			
	//========================================================================================================================================================================================
			
			Pass
			{
				Name "ADDITIVE_SOFT"
				
				Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend One OneMinusSrcColor
				
				ColorMask RGB
				Cull Off
				Lighting Off
				ZWrite Off
				Fog { Color (0,0,0,0) }
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"
				
				sampler2D _MainTex;
				fixed4 _TintColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD1;
					#endif
				};
				
				float4 _MainTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
				
				sampler2D _CameraDepthTexture;
				float _InvFade;
				
				fixed4 frag (v2f i) : COLOR
				{
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif
					
					return i.color * (i.color.a * tex2D(_MainTex, i.texcoord).a);
				}
				ENDCG
			}
		
	//========================================================================================================================================================================================
			
			Pass
			{
				Name "ALPHA_BLENDED"
				
				Tags { "Queue"="Transparent-5" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				
				ColorMask RGB
				Cull Off
				Lighting Off
				ZWrite Off
				Fog { Color (0,0,0,0) }
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"
				#define TEST
				
				sampler2D _MainTex;
				fixed4 _BaseColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD1;
					#endif
				};
				
				float4 _MainTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
				
				sampler2D _CameraDepthTexture;
				float _InvFade;
				
				fixed4 frag (v2f i) : COLOR
				{
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif
					
					fixed alpha = tex2D(_MainTex, i.texcoord).a * i.color.a * _BaseColor.a * 2.0;
					return fixed4(2.0 * _BaseColor.rgb, alpha);
				}
				ENDCG 
			}
			
	//========================================================================================================================================================================================
	
			Pass
			{
				Name "ALPHA_BLENDED_VCOLOR"
				
				Tags { "Queue"="Transparent-5" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				
				ColorMask RGB
				Cull Off
				Lighting Off
				ZWrite Off
				Fog { Color (0,0,0,0) }
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"
				#define TEST
				
				sampler2D _MainTex;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD1;
					#endif
				};
				
				float4 _MainTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
				
				sampler2D _CameraDepthTexture;
				float _InvFade;
				
				fixed4 frag (v2f i) : COLOR
				{
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif
					
					fixed alpha = tex2D(_MainTex, i.texcoord).a * i.color.a * 2.0;
					return fixed4(i.color.rgb, alpha);
				}
				ENDCG 
			}
			
	//========================================================================================================================================================================================
	
		}
	}
}
