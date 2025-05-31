using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EmissionRT : MonoBehaviour
{
    public Light pointLight;
    public Renderer targetRenderer;
    [Range(0, 4)] public int materialIndex = 1;  // Choose the emissive material index

    [Header("Shader Property Names")]
    public string strengthProperty = "_EmissiveStrength";
    public string pulseSpeedProperty = "_PulseSpeed";
    public string colorProperty = "_EmissiveColor";

    void Update()
    {
        if (pointLight == null || targetRenderer == null) return;

        Material[] materials = Application.isPlaying
            ? targetRenderer.materials
            : targetRenderer.sharedMaterials;

        if (materialIndex >= materials.Length) return;

        Material mat = materials[materialIndex];
        if (mat == null || !mat.HasProperty(strengthProperty)) return;

        float pulseSpeed = mat.GetFloat(pulseSpeedProperty);
        float strength = mat.GetFloat(strengthProperty) / 4;
        Color color = mat.GetColor(colorProperty);

        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed);

        pointLight.intensity = strength * pulse;
        pointLight.color = color;
    }
}
