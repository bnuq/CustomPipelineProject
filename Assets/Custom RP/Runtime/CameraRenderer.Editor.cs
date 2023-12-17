using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

// Editor-Only code for CameraRenderer
// 대응하지 않는 Shader Pass 에 대한 설정을 하는 코드만 남긴다
public partial class CameraRenderer
{
    // method partial 을 위해서 선언을 미리 함
    // UNITY_EDITOR 가 아니더라도 호출할 수 있게
    private partial void DrawUnsupportedShaders();

    private partial void DrawGizmos();

    private partial void PrepareForSceneWindow();

    private partial void PrepareBuffer();


// Unity Editor 에서만 유효한 코드
#if UNITY_EDITOR
    private string SampleName { get; set; }

    private static readonly ShaderTagId[] legacyShaderTagIds = 
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    private Material errorMaterial = null;

    private partial void DrawUnsupportedShaders()
    {
        if (this.errorMaterial == null)
        {
            this.errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(this.camera))
        {
            overrideMaterial = this.errorMaterial,
        };
        // draw settings 에 여러 shader passes 를 추가
        for (var i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;

        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    // Gizmo 를 그리는 것도 RenderPipeline 에서 지정해주어야 한다
    private partial void DrawGizmos()
    {
        if (UnityEditor.Handles.ShouldRenderGizmos())  // Editor 에서 Gizmo 를 그려야 하는 상황?
        {
            // gizmo 를 그리는 것도 context 에 명령을 내려서 진행한다??
            this.context.DrawGizmos(this.camera, GizmoSubset.PreImageEffects);
            this.context.DrawGizmos(this.camera, GizmoSubset.PostImageEffects);
        }
    }

    // scene view camera 에서 UI 가 뜨게 설정?
    private partial void PrepareForSceneWindow()
    {
        if (this.camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(this.camera);
        }
    }


    // Command Buffer 에 서로 다른 이름이 들어가면서, Frame Debugger 에 다른 Sample 이름으로 올라간다?
    private partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor_Only");
        this.commandBuffer.name = this.SampleName = this.camera.name;
        Profiler.EndSample();
    }
#else
    private const string SampleName = COMMAND_BUFFER_NAME;
#endif
}
