#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../StandardLibrary/Common.hlsl"
#include "../StandardLibrary/Surface.hlsl"
#include "../StandardLibrary/Light.hlsl"
#include "../StandardLibrary/BRDF.hlsl"
#include "../StandardLibrary/Lighting.hlsl"


// 매크로
TEXTURE2D(_BaseMap);        // 프로퍼티에서 입력한 _BaseMap
SAMPLER(sampler_BaseMap);   // 유니티 엔진 Inspector 에서 설정한 샘플링 설정을 사용하겠다

// GPU Instancing
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
	UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
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
    float3 positionWS : VAR_POSITION;
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

    output.positionWS = positionWS;

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
    
	#if defined(_CLIPPING)  // 셰이더 키워드
		clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
	#endif

    
    Surface surface;

    surface.normal = normalize(input.normalWS);
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
	surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);


    #if defined(_PREMULTIPLY_ALPHA)
		BRDF brdf = GetBRDF(surface, true);
	#else
		BRDF brdf = GetBRDF(surface);
	#endif


    float3 color = GetLighting(surface, brdf);
	return float4(color, surface.alpha);
}

#endif