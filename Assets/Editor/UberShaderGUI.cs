using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
///     Custom Material Inspector GUI for UberShader
///     Provides organized property sections with foldouts and contextual help
/// </summary>
public class UberShaderGUI : ShaderGUI
{
    // ============================================================================
    // PRIVATE FIELDS
    // ============================================================================

    private bool showShadowSettings = true;
    private bool showLightSettings = true;

    // ============================================================================
    // MAIN GUI RENDERING
    // ============================================================================

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var target = materialEditor.target as Material;

        var blendMode = FindProperty("_BlendMode", properties);

        // Base Properties Section
        EditorGUILayout.LabelField("Base Properties", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, 0, 6);

        // Shadow Settings Section
        EditorGUILayout.Space();
        showShadowSettings = EditorGUILayout.Foldout(showShadowSettings, "Shadow Settings", EditorStyles.foldoutHeader);
        if (showShadowSettings)
        {
            EditorGUI.indentLevel++;

            var receiveShadows = FindProperty("_ReceiveShadows", properties);
            materialEditor.ShaderProperty(receiveShadows, "Receive Shadows");

            if (receiveShadows.floatValue > 0)
            {
                materialEditor.ShaderProperty(FindProperty("_ShadowStrength", properties), "Shadow Strength");
                materialEditor.ShaderProperty(FindProperty("_ShadowColor", properties), "Shadow Tint Color");
                materialEditor.ShaderProperty(FindProperty("_EnhancedShadows", properties), "Enhanced Shadow Quality");

                EditorGUILayout.HelpBox(
                    "Shadow Strength: 0 = No shadows, 1 = Normal shadows, 2 = Dark shadows\n" +
                    "Shadow Tint: Color that shadows will be tinted with\n" +
                    "Enhanced Quality: Uses 4x sampling for smoother shadows (performance cost)",
                    MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }

        // Light Response Settings Section
        EditorGUILayout.Space();
        showLightSettings =
            EditorGUILayout.Foldout(showLightSettings, "Light Response Settings", EditorStyles.foldoutHeader);
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
                "Max Darkening: Maximum amount the material can darken",
                MessageType.Info);

            EditorGUI.indentLevel--;
        }

        // Detail Maps Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detail Maps", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableMetallic", properties),
            FindPropertyIndex("_AOStrength", properties) + 1);

        // Emission Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableEmission", properties),
            FindPropertyIndex("_UnlitEmission", properties) + 1);

        // Rendering Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(FindProperty("_CullMode", properties), "Cull Mode");

        // Detail Texturing Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detail Texturing", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableDetail", properties),
            FindPropertyIndex("_DetailStrength", properties) + 1);

        // Fresnel Effect Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fresnel Effect", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableFresnel", properties),
            FindPropertyIndex("_FresnelStrength", properties) + 1);

        // Scrolling UV Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scrolling UV", EditorStyles.boldLabel);
        DrawPropertiesInRange(materialEditor, properties, FindPropertyIndex("_EnableUVScroll", properties),
            FindPropertyIndex("_ScrollSpeedY", properties) + 1);

        // Transparent Rendering Options (Conditional)
        if ((int)blendMode.floatValue == 1)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transparent Rendering Options", EditorStyles.boldLabel);

            var useDepthWrite = target.GetInt("_ZWrite") == 1;
            var newDepthWrite = EditorGUILayout.Toggle("Enable Depth Write", useDepthWrite);
            if (newDepthWrite != useDepthWrite) target.SetInt("_ZWrite", newDepthWrite ? 1 : 0);

            var currentQueue = target.renderQueue;
            var newQueue = EditorGUILayout.IntSlider("Render Queue", currentQueue, 2000, 3500);
            if (newQueue != currentQueue) target.renderQueue = newQueue;

            EditorGUILayout.HelpBox(
                "Render Queue Guide:\n" +
                "• 2000-2499: Geometry (Opaque-like)\n" +
                "• 2500-2999: AlphaTest\n" +
                "• 3000+: Transparent",
                MessageType.Info);
        }

        // Map Usage Guide Section
        EditorGUILayout.Space();
        var metallicToggle = FindProperty("_EnableMetallic", properties);
        var roughnessToggle = FindProperty("_EnableRoughness", properties);

        if (metallicToggle.floatValue > 0 || roughnessToggle.floatValue > 0)
        {
            EditorGUILayout.LabelField("Map Usage Guide", EditorStyles.boldLabel);

            if (metallicToggle.floatValue > 0)
                EditorGUILayout.HelpBox("Metallic Map: R channel = Metallic values", MessageType.Info);

            if (roughnessToggle.floatValue > 0)
                EditorGUILayout.HelpBox("Roughness Map: R channel = Roughness values (converted to smoothness)",
                    MessageType.Info);

            if (metallicToggle.floatValue > 0 && roughnessToggle.floatValue > 0)
                EditorGUILayout.HelpBox(
                    "Can use the same texture for both if it has metallic in R and roughness in G/B channels.",
                    MessageType.Info);
        }

        // Performance Warnings Section
        if (target.GetFloat("_EnhancedShadows") > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enhanced Shadows enabled: This uses 4x shadow sampling.", MessageType.Warning);
        }

        // Material Setup (Always executed)
        SetupMaterialBlendMode(target, (int)blendMode.floatValue);
        SetupMaterialKeywords(target);
    }

    // ============================================================================
    // UTILITY METHODS - GUI HELPERS
    // ============================================================================

    /// <summary>
    ///     Draws material properties within a specified range, excluding blend and depth write properties
    /// </summary>
    private void DrawPropertiesInRange(MaterialEditor materialEditor, MaterialProperty[] properties, int startIndex,
        int endIndex)
    {
        for (var i = startIndex; i < endIndex && i < properties.Length; i++)
            if (!properties[i].name.StartsWith("_Src") && !properties[i].name.StartsWith("_Dst") &&
                !properties[i].name.StartsWith("_ZWrite"))
                materialEditor.ShaderProperty(properties[i], properties[i].displayName);
    }

    /// <summary>
    ///     Finds the index of a property by name in the properties array
    /// </summary>
    private int FindPropertyIndex(string propertyName, MaterialProperty[] properties)
    {
        for (var i = 0; i < properties.Length; i++)
            if (properties[i].name == propertyName)
                return i;
        return 0;
    }

    // ============================================================================
    // MATERIAL CONFIGURATION METHODS
    // ============================================================================

    /// <summary>
    ///     Configures material blend mode settings for Opaque, Transparent, and Alpha Test modes
    /// </summary>
    private void SetupMaterialBlendMode(Material material, int blendMode)
    {
        switch (blendMode)
        {
            case 0: // Opaque Mode
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = -1;
                break;

            case 1: // Transparent Mode
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

                if (!material.HasProperty("_ZWrite") || material.GetInt("_ZWrite") == 0) material.SetInt("_ZWrite", 1);

                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");

                if (material.renderQueue == -1 || material.renderQueue == 3000) material.renderQueue = 2450;
                break;

            case 2: // Alpha Test Mode
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)RenderQueue.AlphaTest;
                break;
        }
    }

    /// <summary>
    ///     Manages shader keywords based on material property values
    ///     Enables/disables features like shadows, metallic maps, normal maps, etc.
    /// </summary>
    private void SetupMaterialKeywords(Material material)
    {
        // Shadow Keywords
        if (material.GetFloat("_ReceiveShadows") > 0)
            material.EnableKeyword("_RECEIVE_SHADOWS");
        else
            material.DisableKeyword("_RECEIVE_SHADOWS");

        if (material.GetFloat("_EnhancedShadows") > 0)
            material.EnableKeyword("_ENHANCED_SHADOWS");
        else
            material.DisableKeyword("_ENHANCED_SHADOWS");

        // Surface Map Keywords
        if (material.GetFloat("_EnableMetallic") > 0)
            material.EnableKeyword("_USE_METALLICMAP");
        else
            material.DisableKeyword("_USE_METALLICMAP");

        if (material.GetFloat("_EnableRoughness") > 0)
            material.EnableKeyword("_USE_ROUGHNESSMAP");
        else
            material.DisableKeyword("_USE_ROUGHNESSMAP");

        if (material.GetFloat("_EnableNormal") > 0)
            material.EnableKeyword("_USE_NORMALMAP");
        else
            material.DisableKeyword("_USE_NORMALMAP");

        // Effect Keywords
        if (material.GetFloat("_EnableEmission") > 0)
            material.EnableKeyword("_USE_EMISSION");
        else
            material.DisableKeyword("_USE_EMISSION");

        if (material.GetFloat("_EnableDetail") > 0)
            material.EnableKeyword("_USE_DETAIL");
        else
            material.DisableKeyword("_USE_DETAIL");

        if (material.GetFloat("_EnableFresnel") > 0)
            material.EnableKeyword("_USE_FRESNEL");
        else
            material.DisableKeyword("_USE_FRESNEL");

        if (material.GetFloat("_EnableUVScroll") > 0)
            material.EnableKeyword("_USE_UV_SCROLL");
        else
            material.DisableKeyword("_USE_UV_SCROLL");

        if (material.GetFloat("_UnlitEmission") > 0)
            material.EnableKeyword("_FORCE_UNLIT_EMISSION");
        else
            material.DisableKeyword("_FORCE_UNLIT_EMISSION");
    }
}