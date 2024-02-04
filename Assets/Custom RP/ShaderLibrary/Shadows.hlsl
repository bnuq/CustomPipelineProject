#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

// Shadow Map 샘플링에 사용되는 매크로
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
// there's only one appropriate way to sample the shadow map
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);


CBUFFER_START(_CustomShadows)
	int _CascadeCount;
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];	
	float4x4 _DirectionalShadowMatrices [MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
	float4 _ShadowDistanceFade;
CBUFFER_END


struct DirectionalShadowData 
{
	float strength;
	int tileIndex;
};


float SampleDirectionalShadowAtlas (float3 positionSTS) 
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation (DirectionalShadowData data, Surface surfaceWS) 
{
	if (data.strength <= 0.0) 
	{
		return 1.0;
	}

	float3 positionSTS 
	= mul(_DirectionalShadowMatrices[data.tileIndex],float4(surfaceWS.position, 1.0)).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);

	return lerp(1.0, shadow, data.strength);
}



struct ShadowData
{
	int cascadeIndex;
	float strength;
};


float FadedShadowStrength (float distance, float scale, float fade) 
{
	return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData (Surface surfaceWS)
{
	ShadowData data;

	data.strength = FadedShadowStrength(
		surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y
	);

	int i;
	for (i = 0; i < _CascadeCount; i++)
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);

		// culling sphere 안에 있는 지 확인
		if (distanceSqr < sphere.w)
		{
			// last cascade
			if (i == _CascadeCount - 1)
			{
				data.strength *= FadedShadowStrength(distanceSqr, 1.0 / sphere.w, _ShadowDistanceFade.z);
			}

			break;
		}
	}

	// loop 를 끝까지 돌았다 = 그림자를 적용하지 않음
	if (i == _CascadeCount)
	{
		data.strength = 0.0f;
	}

	data.cascadeIndex = i;
	return data;
}

#endif