using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(menuName = "Rendering/Custom_Render_Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    private bool useDynamicBatching = true;
    [SerializeField]
    private bool useGPUInstancing = true;
    [SerializeField]
    private bool useSRPBatcher = true;
    [SerializeField]
	private ShadowSettings shadowSettings = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadowSettings);
    }
}
