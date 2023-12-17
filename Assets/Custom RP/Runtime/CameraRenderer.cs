using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Rendering One Camera 목적
public class CameraRenderer
{
    private static readonly string COMMAND_BUFFER_NAME = "Render_Camera";

    // Unlit shader pass 이름
    private static readonly ShaderTagId unlitShaderTagId = 
        new ShaderTagId("SRPDefaultUnlit");

    
    private ScriptableRenderContext context;
    private Camera camera;

    // Context 에 원하는 명령을 쌓아두기는 하는데, 바로 실행하는 건 아니고 쌓아뒀다가 submit 하는 형태
    // Context 에 쌓는 명령 자체를, 나중에 넣고 싶다? 그래서 Command Buffer 사용?
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
        // Camera property 를 먼저 세팅하고 context 를 클리어 하는 게 더 효율적이다?
        this.context.SetupCameraProperties(this.camera);
        this.commandBuffer.ClearRenderTarget(true, true, Color.clear);

        this.commandBuffer.BeginSample(COMMAND_BUFFER_NAME);

        this.ExecuteBuffer();

    }

    private void DrawVisibleGeometry()
    {
        // 카메라에 적용된 sorting setting 을 가져온다?
        var sortingSettings = new SortingSettings(this.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);

        // 어떤 render queue 들을 허가하는 지?
        //var filteringSettings = new FilteringSettings(RenderQueueRange.all);  opaque, transparent 구별 없이 모두 그리기

        // opaque 만 그리기
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        // culling result 를 이용, 카메라에 담긴 geometry 만 그림 -> 이 명령도 context 에 넘김?
        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);

        // 카메라에 담긴 스카이 박스를 그림 -> context 에 넘김?
        this.context.DrawSkybox(this.camera);

        // skybox 를 그리고 나서 불투명한 객체들 그리기
        // 불투명한 객체를 그리기 위한 세팅으로 모두 변경
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;

        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    // 스케줄링한 Commands 를 submit 해야 한다
    private void Submit()
    {
        this.commandBuffer.EndSample(COMMAND_BUFFER_NAME);
        this.ExecuteBuffer();
        this.context.Submit();
    }

    private void ExecuteBuffer()
    { 
        // command buffer 에 쌓아둔 명령들을 context 로 넘김
        this.context.ExecuteCommandBuffer(this.commandBuffer);
        this.commandBuffer.Clear();
    }

    private bool Cull()
    {
        // culling 에 필요한 정보를 카메라에서 바로 가져올 수 있다
        if (this.camera.TryGetCullingParameters(out var cullingParameters))
        {
            // 카메라에서 얻은 culling parameters -> context 에 넣어서 culling result 를 얻는다?
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
