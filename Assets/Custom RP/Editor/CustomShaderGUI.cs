using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    enum ShadowMode
    {
        On, Clip, Dither, Off
    }

    ShadowMode Shadows
    {
        set
        {
            if (SetProperty("_Shadows", (float)value))
            {
                SetKeyword("_SHADOWS_CLIP", value == ShadowMode.Clip);
                SetKeyword("_SHADOWS_DITHER", value == ShadowMode.Dither);
            }
        }
    }

    private MaterialEditor editor;
	private Object[] materials;
	private MaterialProperty[] properties;


	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties) 
    {
		EditorGUI.BeginChangeCheck();

		base.OnGUI(materialEditor, properties);

		this.editor = materialEditor;
		this.materials = materialEditor.targets;
		this.properties = properties;

		BakedEmission();


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

        if (EditorGUI.EndChangeCheck())
        {
            SetShadowCasterPass();
			CopyLightMappingProperties();
        }
    }

    void BakedEmission()
    {
        EditorGUI.BeginChangeCheck();
        editor.LightmapEmissionProperty();
        if (EditorGUI.EndChangeCheck())
        {
            foreach (Material m in editor.targets)
            {
                m.globalIlluminationFlags &=
                    ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
    }

    void CopyLightMappingProperties()
    {
        MaterialProperty mainTex = FindProperty("_MainTex", properties, false);
        MaterialProperty baseMap = FindProperty("_BaseMap", properties, false);
        if (mainTex != null && baseMap != null)
        {
            mainTex.textureValue = baseMap.textureValue;
            mainTex.textureScaleAndOffset = baseMap.textureScaleAndOffset;
        }
        MaterialProperty color = FindProperty("_Color", properties, false);
        MaterialProperty baseColor =
            FindProperty("_BaseColor", properties, false);
        if (color != null && baseColor != null)
        {
            color.colorValue = baseColor.colorValue;
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

    void SetShadowCasterPass()
    {
        MaterialProperty shadows = FindProperty("_Shadows", properties, false);
        if (shadows == null || shadows.hasMixedValue)
        {
            return;
        }

        bool enabled = shadows.floatValue < (float)ShadowMode.Off;
        foreach (Material m in materials)
        {
            m.SetShaderPassEnabled("ShadowCaster", enabled);
        }
    }
}