using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


public class Shadows
{
    struct ShadowedDirectionalLight 
    {
		public int visibleLightIndex;
	}


    private static readonly int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

	private const string bufferName = "Shadows";
    private const int maxShadowedDirectionalLightCount = 4;
	
     
    readonly CommandBuffer buffer = new()
    {
		name = bufferName
	};

    readonly ShadowedDirectionalLight[] ShadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];


	ScriptableRenderContext context;
	CullingResults cullingResults;
	ShadowSettings shadowSettings;
    int shadowedDirectionalLightCount;

	public void Setup (ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings) 
    {
		this.context = context;
		this.cullingResults = cullingResults;
		this.shadowSettings = shadowSettings;

        this.shadowedDirectionalLightCount = 0;
	}

    public void ReserveDirectionalShadows (Light light, int visibleLightIndex) 
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount
            && light.shadows != LightShadows.None && light.shadowStrength > 0
            && this.cullingResults.GetShadowCasterBounds(visibleLightIndex, out _)) // 그림자가 생기는 오브젝트가 있는 지?
        {
			ShadowedDirectionalLights[shadowedDirectionalLightCount++] 
            = new ShadowedDirectionalLight 
            {
                visibleLightIndex = visibleLightIndex
            };
		}
        
    }

    public void Render()
    {
        if (this.shadowedDirectionalLightCount > 0) 
        {
			RenderDirectionalShadows();
		}
        else
        {
			buffer.GetTemporaryRT(
				dirShadowAtlasId, 1, 1,
				32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
			);
		}
    }

    public void Cleanup() 
    {
		buffer.ReleaseTemporaryRT(dirShadowAtlasId);
		ExecuteBuffer();
	}


    void RenderDirectionalShadows() 
    {
		int atlasSize = (int)shadowSettings.directional.atlasSize;

		buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize,
			32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

        buffer.SetRenderTarget(dirShadowAtlasId,RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
	
        buffer.ClearRenderTarget(true, false, Color.clear);
        
        buffer.BeginSample(bufferName);
		ExecuteBuffer();

        int split = this.shadowedDirectionalLightCount <= 1 ? 1 : 2;
		int tileSize = atlasSize / split;

        for (var i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }


	void ExecuteBuffer () 
    {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

    void RenderDirectionalShadows (int index, int split, int tileSize)
    {
        var shadowedDirectionalLight = this.ShadowedDirectionalLights[index];
        var shadowDrawingSettings 
        = new ShadowDrawingSettings(this.cullingResults, shadowedDirectionalLight.visibleLightIndex,
                                    BatchCullingProjectionType.Orthographic);

        this.cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            shadowedDirectionalLight.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
			out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
			out ShadowSplitData splitData);

        shadowDrawingSettings.splitData = splitData;
        SetTileViewport(index, split, tileSize);
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        ExecuteBuffer();
        context.DrawShadows(ref shadowDrawingSettings);
    }

    void SetTileViewport (int index, int split, float tileSize) 
    {
		Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
	}
}