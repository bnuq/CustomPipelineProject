#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

// Constant Buffer = Constant Buffer
// a specialized buffer resource that is accessed like a buffer
// 셰이더 내에서 사용할 Constant 값들을 그룹화해서 한번에 받음 <- CPU 로 부터
// GPU 메모리 내에 들고 있는 값들, CPU 로 부터 받은 Input 값들인거네
CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END


struct Light 
{
	float3 color;
	float3 direction;   // 빛이 오는 쪽 방향
	float attenuation;
};


int GetDirectionalLightCount() 
{
	return _DirectionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex, ShadowData shadowData)
{
	DirectionalShadowData data;

	data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
	data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;

	data.normalBias = _DirectionalLightShadowData[lightIndex].z;

	return data;
}

Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData) 
{
	Light light;

	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;

	DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
	light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);

	// Debug 용도
	//light.attenuation = shadowData.cascadeIndex * 0.25;


	return light;
}

#endif