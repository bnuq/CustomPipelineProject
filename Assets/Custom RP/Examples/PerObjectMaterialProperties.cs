using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour 
{
    // GameObject 마다 가지는 Color
	private static int baseColorId = Shader.PropertyToID("_BaseColor");
    private static int cutoffId = Shader.PropertyToID("_Cutoff");
    private static int metallicId = Shader.PropertyToID("_Metallic");
	private static int smoothnessId = Shader.PropertyToID("_Smoothness");

	
	[SerializeField]
	private Color baseColor = Color.white;
    [SerializeField, Range(0f, 1f)]
	private float cutoff = 0.5f;

    [SerializeField, Range(0f, 1f)]
	private float metallic = 0.0f;

    [SerializeField, Range(0f, 1f)]
	private float smoothness = 0.5f;


    // Material 에 색 값을 넘기는 역할, 클래스에 하나만 있으면 된다?
    // 재활용하면 되니까?
    private MaterialPropertyBlock materialPropertyBlock;


    private void Awake()
    {
        this.OnValidate();
    }

    // Unity Editor 에서 컴포넌트가 로드되거나 값이 바뀔 때 호출됨
    private void OnValidate()
    {
        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new();
        }
        
        // Material 에 다른 색을 넘기기
        // 항상 MPB 를 통해서 넘겨
        materialPropertyBlock.SetColor(baseColorId, this.baseColor);
        materialPropertyBlock.SetFloat(cutoffId, this.cutoff);
        
        materialPropertyBlock.SetFloat(metallicId, this.metallic);
        materialPropertyBlock.SetFloat(smoothnessId, this.smoothness);

        this.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
    }
}