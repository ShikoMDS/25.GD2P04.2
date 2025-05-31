using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/CCTV Effect")]
public class CCTVEffect : VolumeComponent, IPostProcessComponent
{
    [Header("CCTV Settings")]
    public FloatParameter intensity = new FloatParameter(1f);
    public FloatParameter scanLineIntensity = new FloatParameter(0.3f);
    public FloatParameter noiseIntensity = new FloatParameter(0.15f);
    public FloatParameter desaturation = new FloatParameter(0.8f);
    public FloatParameter vignetteIntensity = new FloatParameter(0.4f);
    public BoolParameter showTimestamp = new BoolParameter(true);
    public BoolParameter showScanLines = new BoolParameter(true);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => false;
}