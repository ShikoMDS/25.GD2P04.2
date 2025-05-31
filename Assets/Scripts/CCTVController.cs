using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CCTVController : MonoBehaviour
{
    [Header("CCTV Controls")]
    [Range(0f, 1f)] public float intensity = 1f;
    [Range(0f, 1f)] public float scanLineIntensity = 0.3f;
    [Range(0f, 1f)] public float noiseIntensity = 0.15f;
    [Range(0f, 1f)] public float desaturation = 0.8f;
    [Range(0f, 1f)] public float vignetteIntensity = 0.4f;
    public bool showTimestamp = true;
    public bool showScanLines = true;

    private Volume volume;
    private CCTVEffect cctvEffect;

    void Start()
    {
        // Find or create volume
        volume = FindObjectOfType<Volume>();
        if (volume == null)
        {
            GameObject volumeObj = new GameObject("CCTV Volume");
            volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
        }

        // Get or add CCTV effect to volume profile
        if (volume.profile != null && volume.profile.TryGet<CCTVEffect>(out cctvEffect))
        {
            UpdateEffect();
        }
    }

    void Update()
    {
        if (cctvEffect != null)
        {
            UpdateEffect();
        }
    }

    void UpdateEffect()
    {
        cctvEffect.intensity.value = intensity;
        cctvEffect.scanLineIntensity.value = scanLineIntensity;
        cctvEffect.noiseIntensity.value = noiseIntensity;
        cctvEffect.desaturation.value = desaturation;
        cctvEffect.vignetteIntensity.value = vignetteIntensity;
        cctvEffect.showTimestamp.value = showTimestamp;
        cctvEffect.showScanLines.value = showScanLines;
    }

    [System.Obsolete("Use ToggleEffect() instead")]
    public void ToggleCCTV()
    {
        ToggleEffect();
    }

    public void ToggleEffect()
    {
        intensity = intensity > 0 ? 0 : 1;
    }

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }
}