#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "UnityInput.hlsl"

// float3 TransformObjectToWorld (float3 positionOS) 
// {
// 	return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
// }

// // homogeneous clip space
// float4 TransformWorldToHClip (float3 positionWS)
// {
//     return mul(unity_MatrixVP, float4(positionWS, 1.0));
// }


// SpaceTransforms.hlsl 에서 사용하는 매크로 이름을 맞춰준다
#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_PREV_MATRIX_M unity_prev_MatrixM
#define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
#define UNITY_MATRIX_P glstate_matrix_projection


// For GPU instancing
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

// Unity 에서 제공하는 TransformObjectToWorld, TransformWorldToHClip 를 사용
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
	

float Square (float v) {
	return v * v;
}


#endif