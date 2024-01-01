using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour 
{
    // GameObject 마다 가지는 Color
	public static int baseColorId = Shader.PropertyToID("_BaseColor");
	
	[SerializeField]
	public Color baseColor = Color.white;


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
        materialPropertyBlock.SetColor(baseColorId, this.baseColor);
        this.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
    }
}