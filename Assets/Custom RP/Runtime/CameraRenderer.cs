using UnityEngine;
using UnityEngine.Rendering;


public partial class CameraRenderer
{
    private static readonly string COMMAND_BUFFER_NAME = "Render_Camera";

    private static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static readonly ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

    private readonly CommandBuffer commandBuffer = new CommandBuffer
    {
        name = COMMAND_BUFFER_NAME,
    };

    private readonly Lighting lighting = new();
    

    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults cullingResults;


    public void Render(ScriptableRenderContext context, Camera camera, 
                       bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        // Editor Only
        this.PrepareBuffer();
        this.PrepareForSceneWindow();

        if (!this.Cull(shadowSettings.maxDistance))
        {
            return;
        }
        commandBuffer.BeginSample(SampleName);
		ExecuteBuffer();
        this.lighting.Setup(context, cullingResults, shadowSettings);
        commandBuffer.EndSample(SampleName);
        this.Setup();
        this.DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        
        // Editor Only
        this.DrawUnsupportedShaders();
        this.DrawGizmos();
        
        lighting.Cleanup();
        this.Submit();
    }


    private bool Cull(float maxShadowDistance)
    {
        if (this.camera.TryGetCullingParameters(out var cullingParameters))
        {
            cullingParameters.shadowDistance 
            = Mathf.Min(maxShadowDistance, camera.farClipPlane);

            this.cullingResults = this.context.Cull(ref cullingParameters);
            return true;
        }
        else
        {
            this.cullingResults = default;
            return false;
        }
    }

    private void Setup()
    {
        this.context.SetupCameraProperties(this.camera);

        var flags = this.camera.clearFlags; // how to clear background
        this.commandBuffer.ClearRenderTarget
        (
            flags <= CameraClearFlags.Depth, 
            flags <= CameraClearFlags.Color, 
            flags <= CameraClearFlags.Color ? this.camera.backgroundColor.linear
                                            : Color.clear
        );

        this.commandBuffer.BeginSample(this.SampleName);
        this.ExecuteBuffer();
    }

    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(this.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = PerObjectData.Lightmaps 
                            | PerObjectData.LightProbe
                            | PerObjectData.LightProbeProxyVolume,
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);

        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);


        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);
        this.context.DrawSkybox(this.camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private void Submit()
    {
        this.commandBuffer.EndSample(this.SampleName);
        this.ExecuteBuffer();
        this.context.Submit();
    }

    private void ExecuteBuffer()
    { 
        this.context.ExecuteCommandBuffer(this.commandBuffer);
        this.commandBuffer.Clear();
    }
}
