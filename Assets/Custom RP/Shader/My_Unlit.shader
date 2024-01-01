Shader "Custom_RP/Unlit" 
{
	Properties 
	{
		_BaseColor("Color", Color) = (1.0, 0.0, 1.0, 1.0)
	}

	SubShader
	{
		Pass 
		{
			HLSLPROGRAM
			// GPU Instancing 을 사용하기 위해서
			#pragma multi_compile_instancing

			// 사용할 셰이더 프로그램 이름
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
		}
	}
}