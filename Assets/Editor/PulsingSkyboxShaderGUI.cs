using UnityEditor;
using UnityEngine;

public class PulsingSkyboxShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var material = materialEditor.target as Material;

        // ========================================
        // PROPERTY RETRIEVAL
        // ========================================
        // Get references to all shader properties for UI display

        // Texture properties
        var texture1 = FindProperty("_Texture1", properties);
        var texture2 = FindProperty("_Texture2", properties);
        var texture3 = FindProperty("_Texture3", properties);
        var texture4 = FindProperty("_Texture4", properties);
        var texture5 = FindProperty("_Texture5", properties);
        var texture6 = FindProperty("_Texture6", properties);

        // Texture toggle properties
        var useTexture1 = FindProperty("_UseTexture1", properties);
        var useTexture2 = FindProperty("_UseTexture2", properties);
        var useTexture3 = FindProperty("_UseTexture3", properties);
        var useTexture4 = FindProperty("_UseTexture4", properties);
        var useTexture5 = FindProperty("_UseTexture5", properties);
        var useTexture6 = FindProperty("_UseTexture6", properties);

        // Visual and animation properties
        var color = FindProperty("_Color", properties);
        var pulseSpeed = FindProperty("_PulseSpeed", properties);
        var pulseIntensity = FindProperty("_PulseIntensity", properties);
        var baseIntensity = FindProperty("_BaseIntensity", properties);
        var starThreshold = FindProperty("_StarThreshold", properties);
        var starSize = FindProperty("_StarSize", properties);

        // Rendering properties
        var zWrite = FindProperty("_ZWrite", properties);
        var cull = FindProperty("_Cull", properties);

        // ========================================
        // TEXTURE CONFIGURATION SECTION
        // ========================================
        // UI for selecting and enabling star field textures

        EditorGUILayout.LabelField("Star Field Textures", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");

        EditorGUILayout.BeginHorizontal();
        materialEditor.ShaderProperty(useTexture1, "Use");
        materialEditor.ShaderProperty(texture1, "Texture 1");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        materialEditor.ShaderProperty(useTexture2, "Use");
        materialEditor.ShaderProperty(texture2, "Texture 2");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        materialEditor.ShaderProperty(useTexture3, "Use");
        materialEditor.ShaderProperty(texture3, "Texture 3");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        materialEditor.ShaderProperty(useTexture4, "Use");
        materialEditor.ShaderProperty(texture4, "Texture 4");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        materialEditor.ShaderProperty(useTexture5, "Use");
        materialEditor.ShaderProperty(texture5, "Texture 5");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        materialEditor.ShaderProperty(useTexture6, "Use");
        materialEditor.ShaderProperty(texture6, "Texture 6");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // ========================================
        // STAR APPEARANCE SECTION
        // ========================================
        // Controls for star visual properties

        EditorGUILayout.LabelField("Star Appearance", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        materialEditor.ShaderProperty(color, "Star Color");
        materialEditor.ShaderProperty(starThreshold, "Star Threshold");
        materialEditor.ShaderProperty(starSize, "Star Size");
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // ========================================
        // ANIMATION CONTROLS SECTION
        // ========================================
        // Controls for pulsation and animation behavior

        EditorGUILayout.LabelField("Pulsation Controls", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        materialEditor.ShaderProperty(pulseSpeed, "Pulse Speed");
        materialEditor.ShaderProperty(pulseIntensity, "Pulse Intensity");
        materialEditor.ShaderProperty(baseIntensity, "Base Intensity");
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // ========================================
        // RENDERING OPTIONS SECTION
        // ========================================
        // Advanced rendering settings for different use cases

        EditorGUILayout.LabelField("Rendering Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        materialEditor.ShaderProperty(zWrite, "Z-Write (Enable for solid objects)");
        materialEditor.ShaderProperty(cull, "Culling Mode");
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // ========================================
        // PRESET CONFIGURATIONS SECTION
        // ========================================
        // Quick setup buttons for common shader configurations

        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        // Regular object preset - standard 3D object rendering
        if (GUILayout.Button("Regular Object"))
        {
            material.SetFloat("_ZWrite", 1);
            material.SetFloat("_Cull", 2); // Back
            material.SetFloat("_StarThreshold", 0.7f);
            material.SetFloat("_PulseSpeed", 1.0f);
        }

        // Inner skybox preset - for skybox rendering inside geometry
        if (GUILayout.Button("Inner Skybox"))
        {
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_Cull", 1); // Front
            material.SetFloat("_StarThreshold", 0.7f);
            material.SetFloat("_PulseSpeed", 0.5f);
        }

        // Dense stars preset - more visible stars with enhanced effects
        if (GUILayout.Button("Dense Stars"))
        {
            material.SetFloat("_StarThreshold", 0.5f);
            material.SetFloat("_StarSize", 1.2f);
            material.SetFloat("_PulseIntensity", 0.7f);
        }

        EditorGUILayout.EndHorizontal();
    }
}