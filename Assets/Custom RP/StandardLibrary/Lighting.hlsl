#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 IncomingLight (Surface surface, Light light) 
{
	// Surface 에 얼마나 많은 Light 가 들어오는 지
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting (Surface surface, Light light) 
{
	return IncomingLight(surface, light) * surface.color;
}

float3 GetLighting (Surface surface) 
{
	float3 color = 0.0;

	for (int i = 0; i < GetDirectionalLightCount(); i++)
	{
		color += GetLighting(surface, GetDirectionalLight(i));
	}
	
	return color;
}

#endif