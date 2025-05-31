using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

// ============================================================================
// CCTV EFFECT CONTROLLER
// ============================================================================
// Runtime controller for managing CCTV post-processing effects
// Provides keyboard controls, parameter adjustment, and fade transitions

public class CCTVController : MonoBehaviour
{
    // ========================================================================
    // INSPECTOR CONFIGURATION
    // ========================================================================

    [Header("CCTV Controls")] [Space] [Tooltip("Enable/Disable the entire CCTV effect")]
    public bool enableEffect = true;

    [Space] [Tooltip("Overall effect intensity (0 = disabled, 1 = full effect)")] [Range(0f, 1f)]
    public float intensity = 1f;

    [Tooltip("Intensity of horizontal scan lines")] [Range(0f, 1f)]
    public float scanLineIntensity = 0.3f;

    [Tooltip("Amount of film grain noise")] [Range(0f, 1f)]
    public float noiseIntensity = 0.15f;

    [Tooltip("Level of color desaturation (0 = full color, 1 = grayscale)")] [Range(0f, 1f)]
    public float desaturation = 0.8f;

    [Tooltip("Intensity of corner vignetting")] [Range(0f, 1f)]
    public float vignetteIntensity = 0.4f;

    [Tooltip("Show digital timestamp overlay")]
    public bool showTimestamp = true;

    [Tooltip("Enable horizontal scan line effect")]
    public bool showScanLines = true;

    [Header("Keyboard Controls")] [Tooltip("Key to toggle the effect on/off")]
    public KeyCode toggleKey = KeyCode.F1;

    [Tooltip("Enable keyboard controls")] public bool enableKeyboardControls = true;

    [Header("Events")] [Tooltip("Called when the effect is enabled")]
    public UnityEvent OnEffectEnabled;

    [Tooltip("Called when the effect is disabled")]
    public UnityEvent OnEffectDisabled;

    // ========================================================================
    // PRIVATE VARIABLES
    // ========================================================================

    private Volume volume;
    private CCTVEffect cctvEffect;
    private bool previousEnableState;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        InitializeVolumeAndEffect();
    }

    private void Update()
    {
        HandleKeyboardInput();
        UpdateEffectState();
    }

    // ========================================================================
    // INITIALIZATION
    // ========================================================================

    /// <summary>
    ///     Initialize the volume and CCTV effect components
    /// </summary>
    private void InitializeVolumeAndEffect()
    {
        // Find existing volume or create new one
        volume = FindObjectOfType<Volume>();
        if (volume == null)
        {
            var volumeObj = new GameObject("CCTV Volume");
            volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
        }

        // Get or add CCTV effect to volume profile
        if (volume.profile != null && volume.profile.TryGet(out cctvEffect))
        {
            previousEnableState = enableEffect;
            UpdateEffect();
        }
    }

    // ========================================================================
    // INPUT HANDLING
    // ========================================================================

    /// <summary>
    ///     Handle keyboard input for toggling the effect
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (enableKeyboardControls && Input.GetKeyDown(toggleKey)) ToggleEffect();
    }

    // ========================================================================
    // EFFECT STATE MANAGEMENT
    // ========================================================================

    /// <summary>
    ///     Update effect state and trigger events when needed
    /// </summary>
    private void UpdateEffectState()
    {
        if (cctvEffect != null)
        {
            // Check for state changes and trigger events
            if (previousEnableState != enableEffect)
            {
                previousEnableState = enableEffect;
                TriggerStateChangeEvents();
            }

            UpdateEffect();
        }
    }

    /// <summary>
    ///     Trigger appropriate events based on enable state change
    /// </summary>
    private void TriggerStateChangeEvents()
    {
        if (enableEffect)
            OnEffectEnabled?.Invoke();
        else
            OnEffectDisabled?.Invoke();
    }

    /// <summary>
    ///     Apply current parameter values to the CCTV effect
    /// </summary>
    private void UpdateEffect()
    {
        // Use actual intensity only when effect is enabled
        var actualIntensity = enableEffect ? intensity : 0f;

        cctvEffect.intensity.value = actualIntensity;
        cctvEffect.scanLineIntensity.value = scanLineIntensity;
        cctvEffect.noiseIntensity.value = noiseIntensity;
        cctvEffect.desaturation.value = desaturation;
        cctvEffect.vignetteIntensity.value = vignetteIntensity;
        cctvEffect.showTimestamp.value = showTimestamp;
        cctvEffect.showScanLines.value = showScanLines;
    }

    // ========================================================================
    // PUBLIC API
    // ========================================================================

    #region Effect Control Methods

    /// <summary>
    ///     Toggle the CCTV effect on/off
    /// </summary>
    public void ToggleEffect()
    {
        enableEffect = !enableEffect;
    }

    /// <summary>
    ///     Enable the CCTV effect
    /// </summary>
    public void EnableEffect()
    {
        enableEffect = true;
    }

    /// <summary>
    ///     Disable the CCTV effect
    /// </summary>
    public void DisableEffect()
    {
        enableEffect = false;
    }

    /// <summary>
    ///     Set the effect enabled state
    /// </summary>
    /// <param name="enabled">True to enable, false to disable</param>
    public void SetEffectEnabled(bool enabled)
    {
        enableEffect = enabled;
    }

    /// <summary>
    ///     Check if the effect is currently enabled
    /// </summary>
    /// <returns>True if enabled, false if disabled</returns>
    public bool IsEffectEnabled()
    {
        return enableEffect;
    }

    #endregion

    #region Parameter Control Methods

    /// <summary>
    ///     Set the intensity of the effect (0 = off, 1 = full intensity)
    /// </summary>
    /// <param name="value">Intensity value between 0 and 1</param>
    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }

    #endregion

    #region Fade Transition Methods

    /// <summary>
    ///     Fade the effect in over time
    /// </summary>
    /// <param name="duration">Duration of fade in seconds</param>
    public void FadeIn(float duration = 1f)
    {
        StartCoroutine(FadeEffect(0f, intensity, duration));
    }

    /// <summary>
    ///     Fade the effect out over time
    /// </summary>
    /// <param name="duration">Duration of fade in seconds</param>
    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(FadeEffect(intensity, 0f, duration));
    }

    #endregion

    // ========================================================================
    // PRIVATE COROUTINES
    // ========================================================================

    /// <summary>
    ///     Coroutine to smoothly fade effect intensity over time
    /// </summary>
    /// <param name="startIntensity">Starting intensity value</param>
    /// <param name="endIntensity">Target intensity value</param>
    /// <param name="duration">Duration of the fade</param>
    private IEnumerator FadeEffect(float startIntensity, float endIntensity, float duration)
    {
        enableEffect = true; // Ensure effect is enabled during fade
        var startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            var t = (Time.time - startTime) / duration;
            intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            yield return null;
        }

        intensity = endIntensity;

        // Disable effect if we faded to zero
        if (endIntensity <= 0f) enableEffect = false;
    }

    // ========================================================================
    // LEGACY SUPPORT
    // ========================================================================

    #region Deprecated Methods

    [Obsolete("Use ToggleEffect() instead")]
    public void ToggleCCTV()
    {
        ToggleEffect();
    }

    #endregion
}