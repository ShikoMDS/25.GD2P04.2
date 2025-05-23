using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PulsingSkyboxShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        // Find properties
        MaterialProperty texture1 = FindProperty("_Texture1", properties);
        MaterialProperty texture2 = FindProperty("_Texture2", properties);
        MaterialProperty texture3 = FindProperty("_Texture3", properties);
        MaterialProperty texture4 = FindProperty("_Texture4", properties);
        MaterialProperty texture5 = FindProperty("_Texture5", properties);
        MaterialProperty texture6 = FindProperty("_Texture6", properties);

        MaterialProperty useTexture1 = FindProperty("_UseTexture1", properties);
        MaterialProperty useTexture2 = FindProperty("_UseTexture2", properties);
        MaterialProperty useTexture3 = FindProperty("_UseTexture3", properties);
        MaterialProperty useTexture4 = FindProperty("_UseTexture4", properties);
        MaterialProperty useTexture5 = FindProperty("_UseTexture5", properties);
        MaterialProperty useTexture6 = FindProperty("_UseTexture6", properties);

        MaterialProperty color = FindProperty("_Color", properties);
        MaterialProperty pulseSpeed = FindProperty("_PulseSpeed", properties);
        MaterialProperty pulseIntensity = FindProperty("_PulseIntensity", properties);
        MaterialProperty baseIntensity = FindProperty("_BaseIntensity", properties);
        MaterialProperty starThreshold = FindProperty("_StarThreshold", properties);
        MaterialProperty starSize = FindProperty("_StarSize", properties);
        MaterialProperty zWrite = FindProperty("_ZWrite", properties);
        MaterialProperty cull = FindProperty("_Cull", properties);

        // Display custom UI
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

        EditorGUILayout.LabelField("Star Appearance", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        materialEditor.ShaderProperty(color, "Star Color");
        materialEditor.ShaderProperty(starThreshold, "Star Threshold");
        materialEditor.ShaderProperty(starSize, "Star Size");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Pulsation Controls", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        materialEditor.ShaderProperty(pulseSpeed, "Pulse Speed");
        materialEditor.ShaderProperty(pulseIntensity, "Pulse Intensity");
        materialEditor.ShaderProperty(baseIntensity, "Base Intensity");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Rendering Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("Box");
        materialEditor.ShaderProperty(zWrite, "Z-Write (Enable for solid objects)");
        materialEditor.ShaderProperty(cull, "Culling Mode");
        EditorGUILayout.EndVertical();

        // Display preset buttons for common configurations
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Regular Object"))
        {
            material.SetFloat("_ZWrite", 1);
            material.SetFloat("_Cull", 2); // Back
            material.SetFloat("_StarThreshold", 0.7f);
            material.SetFloat("_PulseSpeed", 1.0f);
        }

        if (GUILayout.Button("Inner Skybox"))
        {
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_Cull", 1); // Front
            material.SetFloat("_StarThreshold", 0.7f);
            material.SetFloat("_PulseSpeed", 0.5f);
        }

        if (GUILayout.Button("Dense Stars"))
        {
            material.SetFloat("_StarThreshold", 0.5f);
            material.SetFloat("_StarSize", 1.2f);
            material.SetFloat("_PulseIntensity", 0.7f);
        }

        EditorGUILayout.EndHorizontal();
    }
}