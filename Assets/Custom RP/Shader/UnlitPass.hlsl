#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../StandardLibrary/Common.hlsl"

// Shader Resources, global scope 에 둔다?
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);


// SRP Batcher 를 사용하기 위해서, material properties 를 concrete memory 에 담는다?
// 그게 cbuffer, constant memory buffer
// cbuffer UnityPerMaterial
// {
//     float4 _BaseColor;
// };


// SRP Batcher 를 쓸 때 사용한 옵션
// CBUFFER_START(UnityPerMaterial)
// 	float4 _BaseColor;
// CBUFFER_END


// GPU Instancing, per-instance material data 를 저장하기 위해서
// unity per material buffer?
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)


// GPU Instancing, 그려지는 각 object 의 인덱스를 이용
// vertex 함수가 struct 를 받는다고 가정 , 그래서 만들어 주어야 한다
struct Attributes  // vertex shader input
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID   // object index
};


struct Varyings  // fragment shader input
{
	float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;  // 그냥 임의로 붙인 의미
	UNITY_VERTEX_INPUT_INSTANCE_ID
};



// vertex 들의 position 값을 계산하겠다 -> clip space
// parameter 에도 이게 어떤 값인지를 알려준다 - semantic
Varyings UnlitPassVertex(Attributes input) // : SV_POSITION , vertex position 을 리턴하지 않으니까
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    float4 positionHClip = TransformWorldToHClip(positionWS);

    output.positionCS = positionHClip;

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    // xy = scale, zw = offset
	output.baseUV = input.baseUV * baseST.xy + baseST.zw;

    return output;
}



// render target 에 값은 넘긴다는 의미?
float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    // make instance index available
    UNITY_SETUP_INSTANCE_ID(input);

    // // index 를 이용해서 instance material properties 에 접근?
	// return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);

    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
	float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);

    // texture 와 basecolor 의 곱 = 최종 색깔
	float4 base = baseMap * baseColor;
    
    // clip 이라는 함수가 있어
	#if defined(_CLIPPING)
		clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
	#endif
	return base;
}



#endif