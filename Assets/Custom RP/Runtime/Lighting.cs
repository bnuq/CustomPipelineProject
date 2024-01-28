using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting 
{
	private static readonly string bufferName = "Lighting";
    private static readonly int maxDirLightCount = 4;

    // 광원에 대한 정보를 셰이더에 넘기기 위해서, Shader Property Id 참조
    private static readonly int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static readonly int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
	private static readonly int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");


    private readonly Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private readonly Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    private readonly CommandBuffer buffer = new()
    {
		name = bufferName
	};
	
    private CullingResults cullingResults;
    private Shadows shadows = new();


	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings) 
    {
        this.cullingResults = cullingResults;
        this.shadows.Setup(context, cullingResults, shadowSettings);

		buffer.BeginSample(bufferName); // Adds a command to begin profile sampling.
        this.SetupLights();             // Lighting 계산에 필요한 동작 ~ Command 를 설정하고
		buffer.EndSample(bufferName);

        this.shadows.Render();

        // Command Buffer 내 명령어 실행 ~ during ScriptableRenderContext.Submit
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}
	
    public void Cleanup() 
    {
		shadows.Cleanup();
	}


    private void SetupLights()
    {
        // NativeArray = provides a connection to a native memory buffer, c# <-> native Unity Engine
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights; // 보이는 광원들

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
        // CPU 에서 얻은 광원 정보를 GPU 로 넘겨주는 ~ 명령어를 버퍼에 세팅
		buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
		dirLightColors[index] = visibleLight.finalColor;
        // matrix 의 3번째 column 이 광원의 방향?
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);

        shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }
}