Shader "Custom_RP/Lit" 
{
	Properties 
	{
		/// [추가 설정] PropertyName, Inspector 에 보여지는 이름, 타입 = 기본 값 

		_BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0

		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		// 셰이더에서 사용하는 키워드 / 프로퍼티 이름
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0

		[KeywordEnum(On, Clip, Dither, Off)] _Shadows ("Shadows", Float) = 0

		[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha ("Premultiply Alpha", Float) = 0

		[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows", Float) = 1

		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5

		[NoScaleOffset] _EmissionMap("Emission", 2D) = "white" {}
		[HDR] _EmissionColor("Emission", Color) = (0.0, 0.0, 0.0, 0.0)

		[HideInInspector] _MainTex("Texture for Lightmap", 2D) = "white" {}
		[HideInInspector] _Color("Color for Lightmap", Color) = (0.5, 0.5, 0.5, 1.0)
	}

	SubShader
	{
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "LitInput.hlsl"
		ENDHLSL

		Pass 
		{
			Tags 
			{
				"LightMode" = "CustomLit"
			}

			// Alpha Blending
			Blend [_SrcBlend] [_DstBlend]
			// ZBuffer Writing
			ZWrite [_ZWrite]

			HLSLPROGRAM
			#pragma target 3.5

			// HLSL 에서 사용할 셰이더 키워드 선언 및 사용 <- shader_feature, multi_compile
			#pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma shader_feature _PREMULTIPLY_ALPHA
			#pragma shader_feature _RECEIVE_SHADOWS

			#pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
			#pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER

			#pragma multi_compile _ LIGHTMAP_ON

			// GPU Instancing 을 사용하기 위해서
			#pragma multi_compile_instancing

			// 사용할 셰이더 프로그램 이름
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			#include "LitPass.hlsl"
			ENDHLSL
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"  // Unity Engine 에서 ShadowCaster pass 가 있어야 DrawShadows 를 실행한다
			}

			ColorMask 0

			HLSLPROGRAM
			#pragma target 3.5
			//#pragma shader_feature _CLIPPING
			#pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "ShadowCasterPass.hlsl"
			ENDHLSL
		}

		Pass
		{
			Tags
			{
				"LightMode" = "Meta"
			}

			Cull Off

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex MetaPassVertex
			#pragma fragment MetaPassFragment
			#include "MetaPass.hlsl"
			ENDHLSL
		}
	}

	CustomEditor "CustomShaderGUI"
}