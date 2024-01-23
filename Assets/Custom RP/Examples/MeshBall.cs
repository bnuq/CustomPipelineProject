using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBall : MonoBehaviour
{
    private static int baseColorId = Shader.PropertyToID("_BaseColor");
    private static int metallicId = Shader.PropertyToID("_Metallic");
    private static int smoothnessId = Shader.PropertyToID("_Smoothness");


    [SerializeField]
    private Mesh mesh = default;

    [SerializeField]
    private Material material = default;

    [SerializeField]
    private float alpha = 1.0f;


    private Matrix4x4[] matrices = new Matrix4x4[1023];
	private Vector4[] baseColors = new Vector4[1023];
	private MaterialPropertyBlock block;


    private float[] metallic = new float[1023];
    private float[] smoothness = new float[1023];



    private void Awake () 
    {
		for (int i = 0; i < matrices.Length; i++) 
        {
			matrices[i] = Matrix4x4.TRS
            (
				Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one
			);
			baseColors[i] = new Vector4(Random.value, Random.value, Random.value, alpha);

            metallic[i] = Random.value < 0.25f ? 1f : 0f;
			smoothness[i] = Random.Range(0.05f, 0.95f);
		}
	}

    void Update () 
    {
		if (block == null)
        {
			block = new MaterialPropertyBlock();

            // property 에 들어갈 값들, 배열을 연결?
			block.SetVectorArray(baseColorId, baseColors);

            block.SetFloatArray(metallicId, metallic);
            block.SetFloatArray(smoothnessId, smoothness);
		}

        // GPU Instancing 으로 그리기
		Graphics.DrawMeshInstanced(mesh, 0, material, matrices, 1023, block);
	}
}
