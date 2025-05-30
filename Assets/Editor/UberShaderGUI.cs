using UnityEngine;
using UnityEditor;

public class UberShaderGUI : ShaderGUI
{
    private bool showShadowSettings = true;
    private bool showLightSettings = true;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material target = materialEditor.target as Material;

        // Find blend mode property
        MaterialProperty blendMode = FindProperty("_BlendMode", properties);

        // Draw default inspector for base properties
        EditorGUILayout.LabelField("Base Properties", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, 0, 6);

        // Shadow Settings Section
        EditorGUILayout.Space();
        showShadowSettings = EditorGUILayout.Foldout(showShadowSettings, "Shadow Settings", EditorStyles.foldoutHeader);
        if (showShadowSettings)
        {
            EditorGUI.indentLevel++;

            MaterialProperty receiveShadows = FindProperty("_ReceiveShadows", properties);
            materialEditor.ShaderProperty(receiveShadows, "Receive Shadows");

            if (receiveShadows.floatValue > 0)
            {
                materialEditor.ShaderProperty(FindProperty("_ShadowStrength", properties), "Shadow Strength");
                materialEditor.ShaderProperty(FindProperty("_ShadowColor", properties), "Shadow Tint Color");
                materialEditor.ShaderProperty(FindProperty("_EnhancedShadows", properties), "Enhanced Shadow Quality");

                EditorGUILayout.HelpBox(
                    "Shadow Strength: 0 = No shadows, 1 = Normal shadows, 2 = Very dark shadows\n" +
                    "Shadow Tint: Color that shadows will be tinted with\n" +
                    "Enhanced Quality: Uses 4x sampling for smoother shadows (performance cost)",
                    MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }
        
        // Light Response Settings Section
        EditorGUILayout.Space();
        showLightSettings = EditorGUILayout.Foldout(showLightSettings, "Light Response Settings", EditorStyles.foldoutHeader);
        if (showLightSettings)
        {
            EditorGUI.indentLevel++;

            materialEditor.ShaderProperty(FindProperty("_LightSensitivity", properties), "Light Sensitivity");
            materialEditor.ShaderProperty(FindProperty("_AmbientDarkening", properties), "Ambient Darkening");
            materialEditor.ShaderProperty(FindProperty("_DarkeningThreshold", properties), "Darkening Threshold");
            materialEditor.ShaderProperty(FindProperty("_MaxDarkening", properties), "Max Darkening");

            EditorGUILayout.HelpBox(
                "Light Sensitivity: How responsive the material is to light changes\n" +
                "Ambient Darkening: Reduces ambient lighting in dark areas\n" +
                "Darkening Threshold: Light level below which darkening starts\n" +
                "Max Darkening: Maximum amount the material can darken (0-1)",
                MessageType.Info);

            EditorGUI.indentLevel--;
        }

        // Draw remaining properties
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detail Maps", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableMetallic", properties),
                             FindPropertyIndex("_AOStrength", properties) + 1);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableEmission", properties),
                             FindPropertyIndex("_UnlitEmission", properties) + 1);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(FindProperty("_CullMode", properties), "Cull Mode");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detail Texturing", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableDetail", properties),
                             FindPropertyIndex("_DetailStrength", properties) + 1);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fresnel Effect", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableFresnel", properties),
                             FindPropertyIndex("_FresnelStrength", properties) + 1);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scrolling UV", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableUVScroll", properties),
                             FindPropertyIndex("_ScrollSpeedY", properties) + 1);

        // Transparency options
        if ((int)blendMode.floatValue == 1) // If transparent mode
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transparent Rendering Options", EditorStyles.boldLabel);

            bool useDepthWrite = target.GetInt("_ZWrite") == 1;
            bool newDepthWrite = EditorGUILayout.Toggle("Enable Depth Write", useDepthWrite);
            if (newDepthWrite != useDepthWrite)
            {
                target.SetInt("_ZWrite", newDepthWrite ? 1 : 0);
            }

            // Custom render queue slider
            int currentQueue = target.renderQueue;
            int newQueue = EditorGUILayout.IntSlider("Render Queue", currentQueue, 2000, 3500);
            if (newQueue != currentQueue)
            {
                target.renderQueue = newQueue;
            }

            EditorGUILayout.HelpBox(
                "Render Queue Guide:\n" +
                "• 2000-2499: Geometry (Opaque-like)\n" +
                "• 2500-2999: AlphaTest\n" +
                "• 3000+: Transparent",
                MessageType.Info);
        }

        // Map usage guidance
        EditorGUILayout.Space();
        MaterialProperty metallicToggle = FindProperty("_EnableMetallic", properties);
        MaterialProperty roughnessToggle = FindProperty("_EnableRoughness", properties);

        if (metallicToggle.floatValue > 0 || roughnessToggle.floatValue > 0)
        {
            EditorGUILayout.LabelField("Map Usage Guide", EditorStyles.boldLabel);

            if (metallicToggle.floatValue > 0)
            {
                EditorGUILayout.HelpBox("Metallic Map: R channel = Metallic values", MessageType.Info);
            }

            if (roughnessToggle.floatValue > 0)
            {
                EditorGUILayout.HelpBox("Roughness Map: R channel = Roughness values (converted to smoothness)", MessageType.Info);
            }

            if (metallicToggle.floatValue > 0 && roughnessToggle.floatValue > 0)
            {
                EditorGUILayout.HelpBox("Tip: You can use the same texture for both if it has metallic in R and roughness in G/B channels.", MessageType.Info);
            }
        }

        // Performance warning
        if (target.GetFloat("_EnhancedShadows") > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enhanced Shadows enabled: This uses 4x shadow sampling which may impact performance on mobile devices.", MessageType.Warning);
        }

        // Set render states based on blend mode
        SetupMaterialBlendMode(target, (int)blendMode.floatValue);

        // Handle keyword updates
        SetupMaterialKeywords(target);
    }

    private void DrawPropertiesInRange(MaterialEditor materialEditor, MaterialProperty[] properties, int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex && i < properties.Length; i++)
        {
            if (!properties[i].name.StartsWith("_Src") && !properties[i].name.StartsWith("_Dst") && !properties[i].name.StartsWith("_ZWrite"))
            {
                materialEditor.ShaderProperty(properties[i], properties[i].displayName);
            }
        }
    }

    private int FindPropertyIndex(string propertyName, MaterialProperty[] properties)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i].name == propertyName)
                return i;
        }
        return 0;
    }

    private void SetupMaterialBlendMode(Material material, int blendMode)
    {
        switch (blendMode)
        {
            case 0: // Opaque
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = -1;
                break;

            case 1: // Transparent
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                // Don't automatically set ZWrite to 0 - let user control it
                if (!material.HasProperty("_ZWrite") || material.GetInt("_ZWrite") == 0)
                {
                    material.SetInt("_ZWrite", 1); // Default to off for backwards compatibility
                }

                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");

                // Set to early transparent queue by default (renders more like opaque)
                if (material.renderQueue == -1 || material.renderQueue == 3000)
                {
                    material.renderQueue = 2450; // Between opaque and alpha test
                }
                break;

            case 2: // Cutout
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
        }
    }

    private void SetupMaterialKeywords(Material material)
    {
        // Handle shadow keywords
        if (material.GetFloat("_ReceiveShadows") > 0)
        {
            material.EnableKeyword("_RECEIVE_SHADOWS");
        }
        else
        {
            material.DisableKeyword("_RECEIVE_SHADOWS");
        }

        if (material.GetFloat("_EnhancedShadows") > 0)
        {
            material.EnableKeyword("_ENHANCED_SHADOWS");
        }
        else
        {
            material.DisableKeyword("_ENHANCED_SHADOWS");
        }

        // Handle metallic map keyword
        if (material.GetFloat("_EnableMetallic") > 0)
        {
            material.EnableKeyword("_USE_METALLICMAP");
        }
        else
        {
            material.DisableKeyword("_USE_METALLICMAP");
        }

        // Handle roughness map keyword
        if (material.GetFloat("_EnableRoughness") > 0)
        {
            material.EnableKeyword("_USE_ROUGHNESSMAP");
        }
        else
        {
            material.DisableKeyword("_USE_ROUGHNESSMAP");
        }

        // Handle other existing keywords
        if (material.GetFloat("_EnableNormal") > 0)
        {
            material.EnableKeyword("_USE_NORMALMAP");
        }
        else
        {
            material.DisableKeyword("_USE_NORMALMAP");
        }

        if (material.GetFloat("_EnableEmission") > 0)
        {
            material.EnableKeyword("_USE_EMISSION");
        }
        else
        {
            material.DisableKeyword("_USE_EMISSION");
        }

        if (material.GetFloat("_EnableDetail") > 0)
        {
            material.EnableKeyword("_USE_DETAIL");
        }
        else
        {
            material.DisableKeyword("_USE_DETAIL");
        }

        if (material.GetFloat("_EnableFresnel") > 0)
        {
            material.EnableKeyword("_USE_FRESNEL");
        }
        else
        {
            material.DisableKeyword("_USE_FRESNEL");
        }

        if (material.GetFloat("_EnableUVScroll") > 0)
        {
            material.EnableKeyword("_USE_UV_SCROLL");
        }
        else
        {
            material.DisableKeyword("_USE_UV_SCROLL");
        }

        if (material.GetFloat("_UnlitEmission") > 0)
        {
            material.EnableKeyword("_FORCE_UNLIT_EMISSION");
        }
        else
        {
            material.DisableKeyword("_FORCE_UNLIT_EMISSION");
        }
    }
}