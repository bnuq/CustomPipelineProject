Shader "Custom_RP/Lit" 
{
	Properties 
	{
		_BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)


		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0

		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
	}

	SubShader
	{
		Tags 
		{
			"LightMode" = "CustomLit"
		}
		
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
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			#include "LitPass.hlsl"
			ENDHLSL
		}
	}
}