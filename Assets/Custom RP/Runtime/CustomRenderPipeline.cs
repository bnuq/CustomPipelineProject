using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// CustomRenderPipeline Asset 을 통해서 리턴하려는 RenderPipeline Instance 자체
/// 
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    // URP -> Scriptable Renderer 와 동일한 역할?
    // 원래 ScriptableRenderContext 하나를 ScriptableRenderer 가 그려주나?
    private CameraRenderer cameraRenderer = new();


    // Create a concrete pipeline
    // Camera[] 이 allocate memory every frame
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        
    }

    // 더 나은 버전
    // Each Frame Unity Invokes 'Render' on the RP instance
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        // Scriptable Render Context -> 엔진에 렌더링하는 데 필요한 context 정보? connection?
        
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
