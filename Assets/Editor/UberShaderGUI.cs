using UnityEngine;
using UnityEditor;

public class UberShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        Material material = materialEditor.target as Material;

        // Find all properties
        MaterialProperty useNormalMap = FindProperty("_UseNormalMap", props);
        MaterialProperty normalMap = FindProperty("_NormalMap", props);
        MaterialProperty normalStrength = FindProperty("_NormalStrength", props);

        MaterialProperty useMetallic = FindProperty("_UseMetallicWorkflow", props);
        MaterialProperty metallicMap = FindProperty("_MetallicMap", props);
        MaterialProperty metallicColor = FindProperty("_MetallicColor", props);
        MaterialProperty metallic = FindProperty("_Metallic", props);

        MaterialProperty enableEmission = FindProperty("_EnableEmission", props);
        MaterialProperty emissionMap = FindProperty("_EmissionMap", props);
        MaterialProperty emissionColor = FindProperty("_EmissionColor", props);
        MaterialProperty emissionStrength = FindProperty("_EmissionStrength", props);

        MaterialProperty enablePulse = FindProperty("_EnablePulse", props);
        MaterialProperty pulseSpeed = FindProperty("_PulseSpeed", props);
        MaterialProperty pulseIntensity = FindProperty("_PulseIntensity", props);

        MaterialProperty alphaTest = FindProperty("_AlphaTest", props);
        MaterialProperty cutoff = FindProperty("_Cutoff", props);

        // Draw base GUI
        materialEditor.PropertiesDefaultGUI(props);

        // Normal Map
        DrawToggle(materialEditor, material, useNormalMap, "_USENORMALMAP");
        if (useNormalMap.floatValue == 1)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalMap);
            materialEditor.ShaderProperty(normalStrength, "Normal Strength");
        }

        // Metallic
        DrawToggle(materialEditor, material, useMetallic, "_USEMETALLICWORKFLOW");
        if (useMetallic.floatValue == 1)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map"), metallicMap);
            materialEditor.ShaderProperty(metallicColor, "Metallic Tint");
            materialEditor.ShaderProperty(metallic, "Metallic");
        }

        // Emission
        DrawToggle(materialEditor, material, enableEmission, "_ENABLEEMISSION");
        if (enableEmission.floatValue == 1)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), emissionMap);
            materialEditor.ShaderProperty(emissionColor, "Emission Color");
            materialEditor.ShaderProperty(emissionStrength, "Emission Strength");

            DrawToggle(materialEditor, material, enablePulse, "_ENABLEPULSE");
            if (enablePulse.floatValue == 1)
            {
                materialEditor.ShaderProperty(pulseSpeed, "Pulse Speed");
                materialEditor.ShaderProperty(pulseIntensity, "Pulse Intensity");
            }
        }

        // Alpha Test
        DrawToggle(materialEditor, material, alphaTest, "_ALPHATEST_ON");
        if (alphaTest.floatValue == 1)
        {
            materialEditor.ShaderProperty(cutoff, "Alpha Cutoff");
        }
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
