using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

// Editor-Only code for CameraRenderer
// �������� �ʴ� Shader Pass �� ���� ������ �ϴ� �ڵ常 �����
public partial class CameraRenderer
{
    // method partial �� ���ؼ� ������ �̸� ��
    // UNITY_EDITOR �� �ƴϴ��� ȣ���� �� �ְ�
    private partial void DrawUnsupportedShaders();

    private partial void DrawGizmos();

    private partial void PrepareForSceneWindow();

    private partial void PrepareBuffer();


// Unity Editor ������ ��ȿ�� �ڵ�
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
        // draw settings �� ���� shader passes �� �߰�
        for (var i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;

        this.context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    // Gizmo �� �׸��� �͵� RenderPipeline ���� �������־�� �Ѵ�
    private partial void DrawGizmos()
    {
        if (UnityEditor.Handles.ShouldRenderGizmos())  // Editor ���� Gizmo �� �׷��� �ϴ� ��Ȳ?
        {
            // gizmo �� �׸��� �͵� context �� ����� ������ �����Ѵ�??
            this.context.DrawGizmos(this.camera, GizmoSubset.PreImageEffects);
            this.context.DrawGizmos(this.camera, GizmoSubset.PostImageEffects);
        }
    }

    // scene view camera ���� UI �� �߰� ����?
    private partial void PrepareForSceneWindow()
    {
        if (this.camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(this.camera);
        }
    }


    // Command Buffer �� ���� �ٸ� �̸��� ���鼭, Frame Debugger �� �ٸ� Sample �̸����� �ö󰣴�?
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
