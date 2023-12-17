using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Rendering One Camera ����
public class CameraRenderer
{
    private static readonly string COMMAND_BUFFER_NAME = "Render_Camera";

    // Unlit shader pass �̸�
    private static readonly ShaderTagId unlitShaderTagId = 
        new ShaderTagId("SRPDefaultUnlit");

    
    private ScriptableRenderContext context;
    private Camera camera;

    // Context �� ���ϴ� ����� �׾Ƶα�� �ϴµ�, �ٷ� �����ϴ� �� �ƴϰ� �׾Ƶ״ٰ� submit �ϴ� ����
    // Context �� �״� ��� ��ü��, ���߿� �ְ� �ʹ�? �׷��� Command Buffer ���?
    private CommandBuffer commandBuffer = new CommandBuffer
    {
        name = COMMAND_BUFFER_NAME,
    };

    private CullingResults cullingResults;


    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        if (!this.Cull())
        {
            return;
        }

        this.Setup();
        this.DrawVisibleGeometry();
        this.Submit();
    }


    private void Setup()
    {
        // Camera property �� ���� �����ϰ� context �� Ŭ���� �ϴ� �� �� ȿ�����̴�?
        this.context.SetupCameraProperties(this.camera);
        this.commandBuffer.ClearRenderTarget(true, true, Color.clear);

        this.commandBuffer.BeginSample(COMMAND_BUFFER_NAME);

        this.ExecuteBuffer();

    }

    private void DrawVisibleGeometry()
    {
        // ī�޶� ����� sorting setting �� �����´�?
        var sortingSettings = new SortingSettings(this.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);

        // � render queue ���� �㰡�ϴ� ��?
        //var filteringSettings = new FilteringSettings(RenderQueueRange.all);  opaque, transparent ���� ���� ��� �׸���

        // opaque �� �׸���
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        // culling result �� �̿�, ī�޶� ��� geometry �� �׸� -> �� ��ɵ� context �� �ѱ�?
        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);

        // ī�޶� ��� ��ī�� �ڽ��� �׸� -> context �� �ѱ�?
        this.context.DrawSkybox(this.camera);

        // skybox �� �׸��� ���� �������� ��ü�� �׸���
        // �������� ��ü�� �׸��� ���� �������� ��� ����
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;

        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    // �����ٸ��� Commands �� submit �ؾ� �Ѵ�
    private void Submit()
    {
        this.commandBuffer.EndSample(COMMAND_BUFFER_NAME);
        this.ExecuteBuffer();
        this.context.Submit();
    }

    private void ExecuteBuffer()
    { 
        // command buffer �� �׾Ƶ� ��ɵ��� context �� �ѱ�
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
