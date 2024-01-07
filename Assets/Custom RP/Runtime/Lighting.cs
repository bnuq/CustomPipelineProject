using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting 
{
	private const string bufferName = "Lighting";


    private const int maxDirLightCount = 4;

    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
	private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private Vector4[] dirLightDirections = new Vector4[maxDirLightCount];


    private CullingResults cullingResults;


	CommandBuffer buffer = new CommandBuffer 
    {
		name = bufferName
	};
	

	public void Setup (ScriptableRenderContext context, CullingResults cullingResults) 
    {
        this.cullingResults = cullingResults;

		buffer.BeginSample(bufferName);
		//this.SetupDirectionalLight()
        this.SetupLights();
		buffer.EndSample(bufferName);

		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}
	
	private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
		dirLightColors[index] = visibleLight.finalColor;
        // matrix 의 3번째 column 이 광원의 방향?
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }

    private void SetupLights()
    {
        // CullingResult -> 보이고자 하느 광원 여러 개를 참조할 수 있다
        // NativeArray = provides a connection to a native memory buffer, c# <-> native Unity Engine
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

        // 실제 카메라 -> 광원의 값을 가져오고
        for (int i = 0, dirLightCount = 0; i < visibleLights.Length; i++)
        {
			VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType != LightType.Directional)
            {
                continue;
            }

			SetupDirectionalLight(dirLightCount++, ref visibleLight);
            if (dirLightCount >= maxDirLightCount)
            {
                break;
            }
		}

        // Shader Property Set
		buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }
}