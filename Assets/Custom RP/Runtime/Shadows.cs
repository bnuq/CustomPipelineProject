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
    private static readonly int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static readonly Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];

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

    // (shadow strength, shadow tile offwet)
    public Vector2 ReserveDirectionalShadows (Light light, int visibleLightIndex) 
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount
            && light.shadows != LightShadows.None && light.shadowStrength > 0
            && this.cullingResults.GetShadowCasterBounds(visibleLightIndex, out _)) // 그림자가 생기는 오브젝트가 있는 지?
        {
			ShadowedDirectionalLights[shadowedDirectionalLightCount] 
            = new ShadowedDirectionalLight 
            {
                visibleLightIndex = visibleLightIndex
            };

            return new Vector2(light.shadowStrength, this.shadowedDirectionalLightCount++);
		}
        else
        {
            return Vector2.zero;
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

        // Send Matrices to GPU
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
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
        dirShadowMatrices[index] 
            = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(index, split, tileSize), split);        
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        ExecuteBuffer();
        context.DrawShadows(ref shadowDrawingSettings);
    }

    Vector2 SetTileViewport (int index, int split, float tileSize) 
    {
		Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));

        return offset;
	}

    Matrix4x4 ConvertToAtlasMatrix (Matrix4x4 m, Vector2 offset, int split) 
    {
        // If Reversed Z Buffer used
        if (SystemInfo.usesReversedZBuffer) 
        {
			m.m20 = -m.m20;
			m.m21 = -m.m21;
			m.m22 = -m.m22;
			m.m23 = -m.m23;
		}

        // clip space -> texture coordinate 계산
        // Apply Tile Offset and Scale
        var scale = 1.0f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
		m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
		m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
		m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
		m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
		m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
		m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
		m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
		m.m20 = 0.5f * (m.m20 + m.m30);
		m.m21 = 0.5f * (m.m21 + m.m31);
		m.m22 = 0.5f * (m.m22 + m.m32);
		m.m23 = 0.5f * (m.m23 + m.m33);

        


		return m;
	}
}