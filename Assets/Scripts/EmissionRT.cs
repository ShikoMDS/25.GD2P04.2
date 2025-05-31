using UnityEngine;

// ============================================================================
// EMISSION REALTIME LIGHT CONTROLLER
// ============================================================================
// Synchronizes a point light with material emission properties in real-time
// Useful for creating realistic lighting that matches emissive materials

[ExecuteAlways]
public class EmissionRT : MonoBehaviour
{
    // ========================================================================
    // COMPONENT REFERENCES
    // ========================================================================

    [Header("Light & Material Setup")] [Tooltip("Point light to synchronize with material emission")]
    public Light pointLight;

    [Tooltip("Renderer containing the emissive material to read from")]
    public Renderer targetRenderer;

    [Tooltip("Index of the emissive material in the renderer's materials array")] [Range(0, 4)]
    public int materialIndex = 1;

    // ========================================================================
    // SHADER PROPERTY CONFIGURATION
    // ========================================================================

    [Header("Shader Property Names")] [Tooltip("Name of the emission strength property in the shader")]
    public string strengthProperty = "_EmissiveStrength";

    [Tooltip("Name of the pulse speed property in the shader")]
    public string pulseSpeedProperty = "_PulseSpeed";

    [Tooltip("Name of the emissive color property in the shader")]
    public string colorProperty = "_EmissiveColor";

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    /// <summary>
    ///     Update light properties to match material emission every frame
    ///     Runs in both play mode and edit mode due to [ExecuteAlways]
    /// </summary>
    private void Update()
    {
        // Validate required components
        if (!ValidateComponents()) return;

        // Get material and validate shader properties
        var emissiveMaterial = GetEmissiveMaterial();
        if (emissiveMaterial == null) return;

        // Read emission properties from material
        var emissionData = ReadEmissionProperties(emissiveMaterial);

        // Apply properties to point light
        UpdateLightFromEmission(emissionData);
    }

    // ========================================================================
    // VALIDATION METHODS
    // ========================================================================

    /// <summary>
    ///     Validate that all required components are assigned and valid
    /// </summary>
    /// <returns>True if all components are valid, false otherwise</returns>
    private bool ValidateComponents()
    {
        if (pointLight == null || targetRenderer == null) return false;

        return true;
    }

    /// <summary>
    ///     Get the emissive material from the target renderer
    /// </summary>
    /// <returns>The emissive material, or null if invalid</returns>
    private Material GetEmissiveMaterial()
    {
        // Get appropriate materials array based on play/edit mode
        var materials = Application.isPlaying
            ? targetRenderer.materials // Runtime materials (instances)
            : targetRenderer.sharedMaterials; // Shared materials (assets)

        // Validate material index
        if (materialIndex >= materials.Length) return null;

        var material = materials[materialIndex];

        // Ensure material exists and has required emission strength property
        if (material == null || !material.HasProperty(strengthProperty)) return null;

        return material;
    }

    // ========================================================================
    // EMISSION DATA HANDLING
    // ========================================================================

    /// <summary>
    ///     Container for emission properties read from material
    /// </summary>
    private struct EmissionData
    {
        public float strength;
        public float pulseSpeed;
        public Color color;
        public float pulseFactor;
    }

    /// <summary>
    ///     Read emission properties from the material shader
    /// </summary>
    /// <param name="material">Material to read properties from</param>
    /// <returns>Emission data structure</returns>
    private EmissionData ReadEmissionProperties(Material material)
    {
        var data = new EmissionData();

        // Read base emission properties
        data.strength = material.GetFloat(strengthProperty) / 4f; // Scale down for light intensity
        data.pulseSpeed = material.GetFloat(pulseSpeedProperty);
        data.color = material.GetColor(colorProperty);

        // Calculate pulsing factor using sine wave
        data.pulseFactor = 1f + Mathf.Sin(Time.time * data.pulseSpeed);

        return data;
    }

    /// <summary>
    ///     Apply emission data to the point light
    /// </summary>
    /// <param name="emissionData">Emission properties to apply</param>
    private void UpdateLightFromEmission(EmissionData emissionData)
    {
        // Apply pulsing intensity based on material emission
        pointLight.intensity = emissionData.strength * emissionData.pulseFactor;

        // Match light color to emission color
        pointLight.color = emissionData.color;
    }

    // ========================================================================
    // EDITOR UTILITIES
    // ========================================================================

#if UNITY_EDITOR
    /// <summary>
    ///     Validate setup in the editor and provide helpful feedback
    /// </summary>
    private void OnValidate()
    {
        // Clamp material index to valid range
        if (targetRenderer != null)
        {
            var maxIndex = targetRenderer.sharedMaterials.Length - 1;
            materialIndex = Mathf.Clamp(materialIndex, 0, maxIndex);
        }
    }
#endif

    // ========================================================================
    // DEBUG UTILITIES
    // ========================================================================

#if UNITY_EDITOR
    /// <summary>
    ///     Display helpful information in the scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (pointLight != null && targetRenderer != null)
        {
            // Draw connection line between light and target renderer
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointLight.transform.position, targetRenderer.bounds.center);

            // Draw light range
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(pointLight.transform.position, pointLight.range);
        }
    }
#endif
}