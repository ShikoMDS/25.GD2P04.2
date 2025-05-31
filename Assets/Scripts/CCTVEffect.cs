using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// ============================================================================
// CCTV POST-PROCESSING EFFECT COMPONENT
// ============================================================================
// Volume component for Unity's URP post-processing stack
// Provides CCTV-style visual effects including scan lines, noise, and timestamp overlay

[Serializable]
[VolumeComponentMenu("Custom/CCTV Effect")]
public class CCTVEffect : VolumeComponent, IPostProcessComponent
{
    // ========================================================================
    // EFFECT PARAMETERS
    // ========================================================================

    [Header("CCTV Settings")] [Tooltip("Overall intensity of the CCTV effect (0 = disabled, 1 = full effect)")]
    public FloatParameter intensity = new(1f);

    [Tooltip("Intensity of horizontal scan lines across the image")]
    public FloatParameter scanLineIntensity = new(0.3f);

    [Tooltip("Amount of film grain noise to add")]
    public FloatParameter noiseIntensity = new(0.15f);

    [Tooltip("Level of color desaturation (0 = full color, 1 = grayscale)")]
    public FloatParameter desaturation = new(0.8f);

    [Tooltip("Intensity of corner vignetting effect")]
    public FloatParameter vignetteIntensity = new(0.4f);

    [Tooltip("Show digital timestamp overlay in top-left corner")]
    public BoolParameter showTimestamp = new(true);

    [Tooltip("Enable horizontal scan line effect")]
    public BoolParameter showScanLines = new(true);

    // ========================================================================
    // IPOSTPROCESSCOMPONENT IMPLEMENTATION
    // ========================================================================

    /// <summary>
    ///     Determines if this effect should be processed
    /// </summary>
    public bool IsActive()
    {
        return intensity.value > 0f;
    }

    /// <summary>
    ///     Indicates this effect is not compatible with tile-based rendering
    /// </summary>
    public bool IsTileCompatible()
    {
        return false;
    }
}