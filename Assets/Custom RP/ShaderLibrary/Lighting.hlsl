#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED


float3 IncomingLight (Surface surface, Light light) 
{
	return saturate(dot(surface.normal, light.direction) * light.attenuation) * light.color;
}

float3 GetLighting (Surface surface, BRDF brdf, Light light) 
{
	return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting (Surface surfaceWS, BRDF brdf) 
{
	float3 color = 0.0;

	for (int i = 0; i < GetDirectionalLightCount(); i++)
	{
		// 광원 정보를 가져와서 GetLighting 진행
		// 광원 정보는 언제 GPU 로 입력되었나? = Pipeline 을 통해서 입력됨
		color += GetLighting(surfaceWS, brdf, GetDirectionalLight(i, surfaceWS));
	}
	
	return color;
}

#endif