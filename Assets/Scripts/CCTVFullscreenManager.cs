using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CCTVPreset
{
    public string presetName;
    [Range(0f, 1f)] public float scanlineIntensity = 0.3f;
    [Range(1f, 50f)] public float scanlineCount = 25f;
    [Range(0f, 1f)] public float vignetteIntensity = 0.4f;
    [Range(0f, 1f)] public float noiseIntensity = 0.1f;
    [Range(0f, 1f)] public float distortionAmount = 0.02f;
    [Range(0f, 1f)] public float colorDesaturation = 0.6f;
    [ColorUsage(false)] public Color tintColor = new Color(0.8f, 1f, 0.8f, 1f);
}

public class CCTVFullscreenManager : MonoBehaviour
{
    [Header("Render Feature Reference")]
    public UniversalRendererData rendererData;

    [Header("Effect Presets")]
    public CCTVPreset[] presets = new CCTVPreset[]
    {
        new CCTVPreset { presetName = "Classic Green", scanlineIntensity = 0.3f, scanlineCount = 25f, vignetteIntensity = 0.4f, noiseIntensity = 0.1f, distortionAmount = 0.02f, colorDesaturation = 0.6f, tintColor = new Color(0.8f, 1f, 0.8f, 1f) },
        new CCTVPreset { presetName = "High Quality", scanlineIntensity = 0.1f, scanlineCount = 35f, vignetteIntensity = 0.2f, noiseIntensity = 0.05f, distortionAmount = 0.01f, colorDesaturation = 0.3f, tintColor = new Color(0.9f, 0.95f, 1f, 1f) },
        new CCTVPreset { presetName = "Old Security", scanlineIntensity = 0.5f, scanlineCount = 15f, vignetteIntensity = 0.6f, noiseIntensity = 0.2f, distortionAmount = 0.04f, colorDesaturation = 0.8f, tintColor = new Color(1f, 0.9f, 0.7f, 1f) },
        new CCTVPreset { presetName = "Night Vision", scanlineIntensity = 0.2f, scanlineCount = 30f, vignetteIntensity = 0.3f, noiseIntensity = 0.15f, distortionAmount = 0.01f, colorDesaturation = 1f, tintColor = new Color(0.6f, 1f, 0.6f, 1f) },
        new CCTVPreset { presetName = "Thermal", scanlineIntensity = 0.15f, scanlineCount = 40f, vignetteIntensity = 0.25f, noiseIntensity = 0.08f, distortionAmount = 0.005f, colorDesaturation = 1f, tintColor = new Color(1f, 0.4f, 0.2f, 1f) }
    };

    [Header("Current Settings")]
    public int currentPresetIndex = 0;

    [Header("Runtime Controls")]
    public bool enableEffect = true;
    public KeyCode toggleEffectKey = KeyCode.F1;
    public KeyCode nextPresetKey = KeyCode.F2;
    public KeyCode previousPresetKey = KeyCode.F3;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private CCTVFullscreenFeature cctvRenderFeature;

    void Start()
    {
        FindCCTVRenderFeature();
        ApplyCurrentPreset();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(toggleEffectKey))
        {
            ToggleEffect();
        }

        if (Input.GetKeyDown(nextPresetKey))
        {
            NextPreset();
        }

        if (Input.GetKeyDown(previousPresetKey))
        {
            PreviousPreset();
        }
    }

    void FindCCTVRenderFeature()
    {
        if (rendererData == null)
        {
            Debug.LogError("RendererData not assigned in CCTVFullscreenManager!");
            return;
        }

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is CCTVFullscreenFeature)
            {
                cctvRenderFeature = feature as CCTVFullscreenFeature;
                break;
            }
        }

        if (cctvRenderFeature == null)
        {
            Debug.LogError("CCTVFullscreenFeature not found in the assigned RendererData!");
        }
        else
        {
            Debug.Log("CCTVFullscreenFeature found and connected successfully!");
        }
    }

    public void ApplyPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presets.Length)
        {
            Debug.LogWarning($"Invalid preset index: {presetIndex}");
            return;
        }

        if (cctvRenderFeature == null)
        {
            Debug.LogError("CCTVFullscreenFeature not found!");
            return;
        }

        currentPresetIndex = presetIndex;
        CCTVPreset preset = presets[presetIndex];

        // Apply preset settings to the render feature
        cctvRenderFeature.settings.scanlineIntensity = preset.scanlineIntensity;
        cctvRenderFeature.settings.scanlineCount = preset.scanlineCount;
        cctvRenderFeature.settings.vignetteIntensity = preset.vignetteIntensity;
        cctvRenderFeature.settings.noiseIntensity = preset.noiseIntensity;
        cctvRenderFeature.settings.distortionAmount = preset.distortionAmount;
        cctvRenderFeature.settings.colorDesaturation = preset.colorDesaturation;
        cctvRenderFeature.settings.tintColor = preset.tintColor;

        Debug.Log($"Applied CCTV preset: {preset.presetName}");
    }

    public void ApplyCurrentPreset()
    {
        ApplyPreset(currentPresetIndex);
    }

    public void NextPreset()
    {
        currentPresetIndex = (currentPresetIndex + 1) % presets.Length;
        ApplyCurrentPreset();
    }

    public void PreviousPreset()
    {
        currentPresetIndex = (currentPresetIndex - 1 + presets.Length) % presets.Length;
        ApplyCurrentPreset();
    }

    public void ToggleEffect()
    {
        enableEffect = !enableEffect;

        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.SetActive(enableEffect);
        }

        Debug.Log($"CCTV Effect: {(enableEffect ? "Enabled" : "Disabled")}");
    }

    public void SetEffectEnabled(bool enabled)
    {
        enableEffect = enabled;

        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.SetActive(enableEffect);
        }
    }

    // Runtime adjustment methods for fine-tuning
    public void SetScanlineIntensity(float value)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.scanlineIntensity = Mathf.Clamp01(value);
        }
    }

    public void SetScanlineCount(float value)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.scanlineCount = Mathf.Clamp(value, 1f, 50f);
        }
    }

    public void SetVignetteIntensity(float value)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.vignetteIntensity = Mathf.Clamp01(value);
        }
    }

    public void SetNoiseIntensity(float value)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.noiseIntensity = Mathf.Clamp01(value);
        }
    }

    public void SetDistortionAmount(float value)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.distortionAmount = Mathf.Clamp01(value);
        }
    }

    public void SetColorDesaturation(float value)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.colorDesaturation = Mathf.Clamp01(value);
        }
    }

    public void SetTintColor(Color color)
    {
        if (cctvRenderFeature != null)
        {
            cctvRenderFeature.settings.tintColor = color;
        }
    }

    // Get current preset info
    public string GetCurrentPresetName()
    {
        if (currentPresetIndex >= 0 && currentPresetIndex < presets.Length)
        {
            return presets[currentPresetIndex].presetName;
        }
        return "Unknown";
    }

    public CCTVPreset GetCurrentPreset()
    {
        if (currentPresetIndex >= 0 && currentPresetIndex < presets.Length)
        {
            return presets[currentPresetIndex];
        }
        return null;
    }

    // UI Display
    void OnGUI()
    {
        if (!showDebugInfo || !enableEffect) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;

        // Background for better readability
        GUIStyle backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = Texture2D.blackTexture;

        GUI.Box(new Rect(5, 5, 450, 80), "", backgroundStyle);

        GUI.Label(new Rect(10, 10, 300, 25), $"CCTV: {GetCurrentPresetName()}", style);
        GUI.Label(new Rect(10, 30, 400, 20), $"Controls: {toggleEffectKey}=Toggle | {nextPresetKey}=Next | {previousPresetKey}=Previous", style);

        CCTVPreset current = GetCurrentPreset();
        if (current != null)
        {
            GUI.Label(new Rect(10, 50, 400, 20), $"Scanlines: {current.scanlineIntensity:F2} | Vignette: {current.vignetteIntensity:F2} | Noise: {current.noiseIntensity:F2}", style);
        }
    }

    // Editor helper methods
#if UNITY_EDITOR
    [ContextMenu("Find Render Feature")]
    void EditorFindRenderFeature()
    {
        FindCCTVRenderFeature();
    }

    [ContextMenu("Apply Current Preset")]
    void EditorApplyCurrentPreset()
    {
        FindCCTVRenderFeature();
        ApplyCurrentPreset();
    }

    [ContextMenu("Test Toggle Effect")]
    void EditorTestToggle()
    {
        FindCCTVRenderFeature();
        ToggleEffect();
    }

    [ContextMenu("Cycle Through All Presets")]
    void EditorCyclePesets()
    {
        StartCoroutine(CyclePresets());
    }

    IEnumerator CyclePresets()
    {
        for (int i = 0; i < presets.Length; i++)
        {
            ApplyPreset(i);
            yield return new WaitForSeconds(2f);
        }
    }
#endif
}