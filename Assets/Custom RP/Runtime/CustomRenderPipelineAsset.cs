using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// RenderPipelineAsset = Unity ������ ����� pipeline object instance
/// asset �� handle, settings �� �����صδ� ���
/// RPAsset �� �����ϸ鼭 �� settings �� �����ϴ� ���̴�
/// Unity �� ������ �� CustomRPAsset �� �����ϰ� �� -> ���� ������ ���� pipeline �� �ְ� �Ѵ�
/// </summary>
[CreateAssetMenu(menuName = "Rendering/Custom_Render_Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    // Unity ������ Pipeline ����, handle �� �ѱ�, �����ϰ� ��
    protected override RenderPipeline CreatePipeline()
    {
        // ���� ���� RenderPipeline �� �����ϵ��� 
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
