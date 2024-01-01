using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// CustomRenderPipeline Asset �� ���ؼ� �����Ϸ��� RenderPipeline Instance ��ü
/// 
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    // URP -> Scriptable Renderer �� ������ ����?
    // ���� ScriptableRenderContext �ϳ��� ScriptableRenderer �� �׷��ֳ�?
    private CameraRenderer cameraRenderer = new();


    public CustomRenderPipeline () 
    {
        // SRP Batcher 사용을 위한 세팅
		GraphicsSettings.useScriptableRenderPipelineBatching = true;
	}


    // Create a concrete pipeline
    // Camera[] �� allocate memory every frame
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        
    }

    // �� ���� ����
    // Each Frame Unity Invokes 'Render' on the RP instance
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        // Scriptable Render Context -> ������ �������ϴ� �� �ʿ��� context ����? connection?
        
        foreach (var camera in cameras)
        {
            cameraRenderer.Render(context, camera);
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
