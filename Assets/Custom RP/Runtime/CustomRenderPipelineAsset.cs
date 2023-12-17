using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// RenderPipelineAsset = Unity 엔진이 사용할 pipeline object instance
/// asset 는 handle, settings 를 저장해두는 장소
/// RPAsset 을 구현하면서 그 settings 를 설정하는 것이다
/// Unity 이 엔진이 이 CustomRPAsset 을 참조하게 해 -> 내가 설정한 값을 pipeline 에 넣게 한다
/// </summary>
[CreateAssetMenu(menuName = "Rendering/Custom_Render_Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    // Unity 엔진에 Pipeline 설정, handle 을 넘김, 참조하게 함
    protected override RenderPipeline CreatePipeline()
    {
        // 내가 만든 RenderPipeline 을 리턴하도록 
        return new CustomRenderPipeline();
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
