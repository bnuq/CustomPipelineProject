using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
	private MaterialEditor editor;
	private Object[] materials;
	private MaterialProperty[] properties;


	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties) 
    {
		base.OnGUI(materialEditor, properties);

		this.editor = materialEditor;
		this.materials = materialEditor.targets;
		this.properties = properties;

        if (PresetButton("Opaque")) 
		{
			Clipping = false;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.Geometry;
		}
		if (PresetButton("Clip")) 
		{
			Clipping = true;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.AlphaTest;
		}
		if (PresetButton("Fade")) 
		{
			Clipping = false;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.SrcAlpha;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
		if (HasPremultiplyAlpha && PresetButton("Transparent")) 
		{
			Clipping = false;
			PremultiplyAlpha = true;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}


	bool PresetButton (string name) 
	{
		if (GUILayout.Button(name)) 
		{
			editor.RegisterPropertyChangeUndo(name);
			return true;
		}
		return false;
	}

    bool Clipping 
	{
		set => SetProperty("_Clipping", "_CLIPPING", value);
	}
	bool PremultiplyAlpha
	{
		set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
	}
	BlendMode SrcBlend
	{
		set => SetProperty("_SrcBlend", (float)value);
	}
	BlendMode DstBlend 
	{
		set => SetProperty("_DstBlend", (float)value);
	}
	bool ZWrite 
	{
		set => SetProperty("_ZWrite", value ? 1f : 0f);
	}
    RenderQueue RenderQueue 
	{
		set 
		{
			foreach (var m in materials.Cast<Material>()) 
			{
				m.renderQueue = (int)value;
			}
		}
	}
    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");
	bool HasProperty (string name) => FindProperty(name, properties, false) != null;
    
	
    void SetProperty (string propertyName, string keyword, bool value)
	{
		if (SetProperty(propertyName, value ? 1f : 0f))
		{
            SetKeyword(keyword, value);
		}
	}

    bool SetProperty (string propertyName, float value)
	{
		// propertyIsMandatory = false -> property 못 찾아도 error 발생하지 않음
		MaterialProperty property = FindProperty(propertyName, properties, false); 
		
		if (property != null) 
		{
			property.floatValue = value;
			return true;
		}
		else
		{
			return false;
		}
	}

    void SetKeyword (string keyword, bool enabled)
	{
		if (enabled) 
		{
			foreach (var m in materials.Cast<Material>()) 
			{
				// Enables a local shader keyword
				// 셰이더에서 사용되는 키워드를 정의해 주는 거네
				// 그 지정을 Material 단에서 진행한다
				m.EnableKeyword(keyword);
			}
		}
		else 
		{
			foreach (var m in materials.Cast<Material>()) 
			{
				// 셰이더 키워드 정의 취소도 Material 에서 진행
				m.DisableKeyword(keyword);
			}
		}
	}
}