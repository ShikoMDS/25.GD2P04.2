using UnityEngine;
using UnityEditor;

public class UberShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        Material material = materialEditor.target as Material;

        // Find all properties
        MaterialProperty baseMap = FindProperty("_BaseMap", props);
        MaterialProperty baseColor = FindProperty("_BaseColor", props);

        MaterialProperty useNormalMap = FindProperty("_UseNormalMap", props);
        MaterialProperty normalMap = FindProperty("_NormalMap", props);
        MaterialProperty normalStrength = FindProperty("_NormalStrength", props);

        MaterialProperty useMetallic = FindProperty("_UseMetallicWorkflow", props);
        MaterialProperty metallicMap = FindProperty("_MetallicMap", props);
        MaterialProperty metallicColor = FindProperty("_MetallicColor", props);
        MaterialProperty metallic = FindProperty("_Metallic", props);

        MaterialProperty roughnessMap = FindProperty("_RoughnessMap", props);
        MaterialProperty roughness = FindProperty("_Roughness", props);
        MaterialProperty smoothness = FindProperty("_Smoothness", props);

        MaterialProperty enableEmission = FindProperty("_EnableEmission", props);
        MaterialProperty emissionMap = FindProperty("_EmissionMap", props);
        MaterialProperty emissionColor = FindProperty("_EmissionColor", props);
        MaterialProperty emissionStrength = FindProperty("_EmissionStrength", props);

        MaterialProperty enablePulse = FindProperty("_EnablePulse", props);
        MaterialProperty pulseSpeed = FindProperty("_PulseSpeed", props);
        MaterialProperty pulseIntensity = FindProperty("_PulseIntensity", props);

        MaterialProperty alphaTest = FindProperty("_AlphaTest", props);
        MaterialProperty cutoff = FindProperty("_Cutoff", props);

        // Header styles
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Base Properties", headerStyle);
        EditorGUI.indentLevel++;
        materialEditor.TexturePropertySingleLine(new GUIContent("Base Map"), baseMap, baseColor);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Normal Mapping", headerStyle);
        EditorGUI.indentLevel++;
        DrawToggle(materialEditor, material, useNormalMap, "_USENORMALMAP");
        if (useNormalMap.floatValue == 1)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalMap);
            materialEditor.ShaderProperty(normalStrength, "Normal Strength");
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("PBR Properties", headerStyle);
        EditorGUI.indentLevel++;
        DrawToggle(materialEditor, material, useMetallic, "_USEMETALLICWORKFLOW");
        if (useMetallic.floatValue == 1)
        {
            EditorGUILayout.LabelField("Metallic", EditorStyles.miniBoldLabel);
            materialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map"), metallicMap);
            materialEditor.ShaderProperty(metallicColor, "Metallic Tint");
            materialEditor.ShaderProperty(metallic, "Metallic");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Roughness", EditorStyles.miniBoldLabel);
            materialEditor.TexturePropertySingleLine(new GUIContent("Roughness Map"), roughnessMap);
            materialEditor.ShaderProperty(roughness, "Roughness");

            EditorGUILayout.HelpBox("Tip: Use a packed texture with Metallic in R channel and Roughness in G channel for efficiency.", MessageType.Info);
        }
        else
        {
            materialEditor.ShaderProperty(smoothness, "Smoothness");
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Emission", headerStyle);
        EditorGUI.indentLevel++;
        DrawToggle(materialEditor, material, enableEmission, "_ENABLEEMISSION");
        if (enableEmission.floatValue == 1)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), emissionMap, emissionColor);
            materialEditor.ShaderProperty(emissionStrength, "Emission Strength");

            EditorGUILayout.Space(5);
            DrawToggle(materialEditor, material, enablePulse, "_ENABLEPULSE");
            if (enablePulse.floatValue == 1)
            {
                materialEditor.ShaderProperty(pulseSpeed, "Pulse Speed");
                materialEditor.ShaderProperty(pulseIntensity, "Pulse Intensity");
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Transparency", headerStyle);
        EditorGUI.indentLevel++;
        DrawToggle(materialEditor, material, alphaTest, "_ALPHATEST_ON");
        if (alphaTest.floatValue == 1)
        {
            materialEditor.ShaderProperty(cutoff, "Alpha Cutoff");
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Material Usage Tips", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• Diffuse Map: Use _BaseMap for your main texture\n" +
            "• Metallic Map: Red channel for metallic values (0=dielectric, 1=metal)\n" +
            "• Roughness Map: Single channel texture (0=mirror, 1=rough)\n" +
            "• For packed textures: Metallic(R), Roughness(G), AO(B)",
            MessageType.Info);
    }

    private void DrawToggle(MaterialEditor editor, Material mat, MaterialProperty prop, string keyword)
    {
        EditorGUI.BeginChangeCheck();
        editor.ShaderProperty(prop, prop.displayName);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (Material m in prop.targets)
            {
                if (prop.floatValue == 1)
                    m.EnableKeyword(keyword);
                else
                    m.DisableKeyword(keyword);
            }
        }
    }
}