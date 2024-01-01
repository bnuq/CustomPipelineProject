#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED

// Uniform Value
// unity engine 에 의해서 자동으로 들어오는 값
CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4x4 unity_WorldToObject;
    float4 unity_LODFade;  // 사용하지 않아도 그룹에 속하는 값이여서 넣어주어야 한다
	real4 unity_WorldTransformParams;
CBUFFER_END


float4x4 unity_MatrixVP;    // view-projection matrix
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_prev_MatrixM;
float4x4 unity_prev_MatrixIM;
float4x4 glstate_matrix_projection;


#endif