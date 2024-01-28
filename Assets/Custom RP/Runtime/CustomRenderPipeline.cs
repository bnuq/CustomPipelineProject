using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class CustomRenderPipeline : RenderPipeline
{
    private readonly CameraRenderer cameraRenderer = new();

    private bool useDynamicBatching = true;
    private bool useGPUInstancing = true;
    private ShadowSettings shadowSettings;

    public CustomRenderPipeline (bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings) 
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;

        // SRP Batcher 사용을 위한 세팅
		GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;

        // final color, convert to linear space
        GraphicsSettings.lightsUseLinearIntensity = true;
	}


    // Create a concrete pipeline
    // Camera[] -> allocate memory every frame
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        
    }

    // Each Frame Unity Invokes 'Render' on the RP instance
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            cameraRenderer.Render(context, camera, this.useDynamicBatching, this.useGPUInstancing, this.shadowSettings);
        }
    }
}
