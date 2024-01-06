#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../StandardLibrary/Common.hlsl"
#include "../StandardLibrary/Surface.hlsl"
#include "../StandardLibrary/Lighting.hlsl"


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)



struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID   // object index
};


struct Varyings
{
	float4 positionCS : SV_POSITION;
    float3 normalWS : VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;    // 그냥 임의로 붙인 의미
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


Varyings LitPassVertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    float4 positionHClip = TransformWorldToHClip(positionWS);
    output.positionCS = positionHClip;

    output.normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    // xy = scale, zw = offset
	output.baseUV = input.baseUV * baseST.xy + baseST.zw;

    return output;
}


float4 LitPassFragment(Varyings input) : SV_TARGET
{
    // make instance index available
    UNITY_SETUP_INSTANCE_ID(input);

    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
	float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);

    // texture 와 basecolor 의 곱 = 최종 색깔
	float4 base = baseMap * baseColor;
    
    // clip 이라는 함수가 있어
	#if defined(_CLIPPING)
		clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
	#endif

    
    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.color = base.rgb;
    surface.alpha = base.a;

    float3 color = GetLighting(surface);
	return float4(color, surface.alpha);
}



#endif