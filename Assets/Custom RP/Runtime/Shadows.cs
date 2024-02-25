using UnityEngine;
using UnityEngine.Rendering;


public class Shadows
{
    struct ShadowedDirectionalLight 
    {
		public int visibleLightIndex;
        public float slopeScaleBias;
        public float nearPlaneOffset;
	}


    private static readonly int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static readonly int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static readonly int cascadeCountId = Shader.PropertyToID("_CascadeCount");
    private static readonly int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
    private static readonly int cascadeDataId = Shader.PropertyToID("_CascadeData");
    private static readonly int shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");
    private static readonly int shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

    private static readonly Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];
    private static readonly Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
    private static readonly Vector4[] cascadeData = new Vector4[maxCascades];

    private static readonly string[] directionalFilterKeywords = 
    {
		"_DIRECTIONAL_PCF3",
		"_DIRECTIONAL_PCF5",
		"_DIRECTIONAL_PCF7",
	};

    private static readonly string[] cascadeBlendKeywords =
    {
        "_CASCADE_BLEND_SOFT",
        "_CASCADE_BLEND_DITHER"
    };



    private const string bufferName = "Shadows";
    private const int maxShadowedDirectionalLightCount = 4;
    private const int maxCascades = 4;

     
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
    public Vector3 ReserveDirectionalShadows (Light light, int visibleLightIndex) 
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount
            && light.shadows != LightShadows.None && light.shadowStrength > 0
            && this.cullingResults.GetShadowCasterBounds(visibleLightIndex, out _)) // 그림자가 생기는 오브젝트가 있는 지?
        {
			ShadowedDirectionalLights[shadowedDirectionalLightCount] 
            = new ShadowedDirectionalLight 
            {
                visibleLightIndex = visibleLightIndex,
                slopeScaleBias = light.shadowBias,
                nearPlaneOffset = light.shadowNearPlane
            };

            return new Vector3(
                light.shadowStrength, 
                this.shadowSettings.directional.cascadeCount * this.shadowedDirectionalLightCount++,
                light.shadowNormalBias);
		}
        else
        {
            return Vector3.zero;
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

        int tiles = this.shadowedDirectionalLightCount * this.shadowSettings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
		int tileSize = atlasSize / split;

        for (var i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        // Send Matrices to GPU
        buffer.SetGlobalInt(cascadeCountId, this.shadowSettings.directional.cascadeCount);
        buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);

        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        
        var f = 1.0f - this.shadowSettings.directional.cascadeFade;
        buffer.SetGlobalVector(shadowDistanceFadeId,
            new Vector4(1.0f / this.shadowSettings.maxDistance, 
                        1.0f / this.shadowSettings.distanceFade,
                        1.0f / (1.0f - f * f))
        );
        
        this.SetKeywords(directionalFilterKeywords, (int)this.shadowSettings.directional.filter - 1);
        this.SetKeywords(cascadeBlendKeywords, (int)this.shadowSettings.directional.cascadeBlend- 1);
        buffer.SetGlobalVector(shadowAtlasSizeId, new Vector4(atlasSize, 1f / atlasSize));
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


        var cascadeCount = this.shadowSettings.directional.cascadeCount;
        var tileOffset = index * cascadeCount;

        var ratios = this.shadowSettings.directional.CascadeRatios;


        var cullingFactor = Mathf.Max(0.0f, 0.8f - this.shadowSettings.directional.cascadeFade);

        for (var i = 0; i < cascadeCount; i++)
        {
            this.cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                shadowedDirectionalLight.visibleLightIndex, i, cascadeCount, ratios, tileSize, 
                shadowedDirectionalLight.nearPlaneOffset,
			    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
			    out ShadowSplitData splitData);

            splitData.shadowCascadeBlendCullingFactor = cullingFactor;

            shadowDrawingSettings.splitData = splitData;

            // first light
            if (index == 0)
            {
                SetCascadeData(i, splitData.cullingSphere, tileSize);
            }


            var tileIndex = tileOffset + i;
            dirShadowMatrices[tileIndex] 
                = ConvertToAtlasMatrix(
                    projectionMatrix * viewMatrix, 
                    SetTileViewport(tileIndex, split, tileSize), 
                    split
            );

            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);


            buffer.SetGlobalDepthBias(0.0f, shadowedDirectionalLight.slopeScaleBias);
            ExecuteBuffer();
            context.DrawShadows(ref shadowDrawingSettings);
            buffer.SetGlobalDepthBias(0, 0);

        }
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

    void SetCascadeData (int index, Vector4 cullingSphere, float tileSize) 
    {
        float texelSize = 2f * cullingSphere.w / tileSize;
        float filterSize = texelSize * ((float)this.shadowSettings.directional.filter + 1f);

        cullingSphere.w -= filterSize;
		cullingSphere.w *= cullingSphere.w;
        
		cascadeCullingSpheres[index] = cullingSphere;

        cascadeData[index] = new Vector4(1.0f / cullingSphere.w, filterSize * 1.4142136f);
	}

    void SetKeywords(string[] keywords, int enabledIndex) 
    {
		for (int i = 0; i < keywords.Length; i++) {
			if (i == enabledIndex) {
				buffer.EnableShaderKeyword(keywords[i]);
			}
			else {
				buffer.DisableShaderKeyword(keywords[i]);
			}
		}
	}
}