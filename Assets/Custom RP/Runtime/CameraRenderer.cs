using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private static readonly string COMMAND_BUFFER_NAME = "Render_Camera";

    private static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static readonly ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

    
    private ScriptableRenderContext context;
    private Camera camera;


    private CommandBuffer commandBuffer = new CommandBuffer
    {
        name = COMMAND_BUFFER_NAME,
    };

    private CullingResults cullingResults;

    private Lighting lighting = new();


    public void Render(ScriptableRenderContext context, Camera camera,
                       bool useDynamicBatching, bool useGPUInstancing)
    {
        this.context = context;
        this.camera = camera;

        this.PrepareBuffer();
        this.PrepareForSceneWindow();
        if (!this.Cull())
        {
            return;
        }

        this.Setup();
        this.lighting.Setup(context, cullingResults);
        this.DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        this.DrawUnsupportedShaders();
        this.DrawGizmos();
        this.Submit();
    }


    private void Setup()
    {
        // Camera property �� ���� �����ϰ� context �� Ŭ���� �ϴ� �� �� ȿ�����̴�?
        this.context.SetupCameraProperties(this.camera);

        var flags = this.camera.clearFlags; // how to clear background

        this.commandBuffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags <= CameraClearFlags.Color, 
            flags <= CameraClearFlags.Color ? this.camera.backgroundColor.linear
                                            : Color.clear);

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

    // �����ٸ��� Commands �� submit �ؾ� �Ѵ�
    private void Submit()
    {
        this.commandBuffer.EndSample(this.SampleName);
        this.ExecuteBuffer();
        this.context.Submit();
    }

    private void ExecuteBuffer()
    { 
        // command buffer �� �׾Ƶ� ���ɵ��� context �� �ѱ�
        this.context.ExecuteCommandBuffer(this.commandBuffer);
        this.commandBuffer.Clear();
    }

    private bool Cull()
    {
        // culling �� �ʿ��� ������ ī�޶󿡼� �ٷ� ������ �� �ִ�
        if (this.camera.TryGetCullingParameters(out var cullingParameters))
        {
            // ī�޶󿡼� ���� culling parameters -> context �� �־ culling result �� ��´�?
            this.cullingResults = this.context.Cull(ref cullingParameters);
            return true;
        }
        else
        {
            this.cullingResults = default;
            return false;
        }
    }
}
