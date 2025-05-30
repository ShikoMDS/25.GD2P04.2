using UnityEngine;
using UnityEditor;

public class UberShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material target = materialEditor.target as Material;

        // Find blend mode property
        MaterialProperty blendMode = FindProperty("_BlendMode", properties);

        // Draw default inspector
        base.OnGUI(materialEditor, properties);

        // Add custom transparent options
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

        // Set render states based on blend mode
        SetupMaterialBlendMode(target, (int)blendMode.floatValue);
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
                    material.SetInt("_ZWrite", 0); // Default to off for backwards compatibility
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
}