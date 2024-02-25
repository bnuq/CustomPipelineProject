Shader "Custom_RP/Unlit" 
{
	Properties 
	{
		_BaseMap("Texture", 2D) = "white" {}  // unity default texture 사용
		[HDR] _BaseColor("Color", Color) = (1.0, 0.0, 1.0, 1.0)

		// source ~ 이번에 그려지는 것
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		// destination ~ 이미 그려져 있던 것
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0

		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
	}

	SubShader
	{
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "UnlitInput.hlsl"
		ENDHLSL

		Pass 
		{
			Blend [_SrcBlend] [_DstBlend]

			// Transparent 색깔을 그린다면, z buffer 를 사용하지 않는다
			ZWrite [_ZWrite]

			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			// GPU Instancing 을 사용하기 위해서
			#pragma multi_compile_instancing

			// 사용할 셰이더 프로그램 이름
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
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