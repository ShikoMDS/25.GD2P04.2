using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// ============================================================================
// CCTV RENDER FEATURE
// ============================================================================
// URP Scriptable Renderer Feature for applying CCTV post-processing effects
// Integrates with Unity's Volume system for parameter control

public class CCTVRenderFeature : ScriptableRendererFeature
{
    // ========================================================================
    // SETTINGS CLASS
    // ========================================================================

    [Serializable]
    public class CCTVSettings
    {
        [Tooltip("When in the rendering pipeline to apply the CCTV effect")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Tooltip("Material containing the CCTV shader")]
        public Material cctvMaterial;
    }

    // ========================================================================
    // PUBLIC FIELDS
    // ========================================================================

    [Header("CCTV Render Settings")] public CCTVSettings settings = new();

    // ========================================================================
    // PRIVATE FIELDS
    // ========================================================================

    private CCTVRenderPass cctvPass;

    // ========================================================================
    // SCRIPTABLE RENDERER FEATURE OVERRIDES
    // ========================================================================

    /// <summary>
    ///     Create and initialize the CCTV render pass
    ///     Called once when the feature is created
    /// </summary>
    public override void Create()
    {
        cctvPass = new CCTVRenderPass(settings);
    }

    /// <summary>
    ///     Add the CCTV render pass to the rendering queue
    ///     Called every frame for each camera
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Skip if no material is assigned
        if (settings.cctvMaterial == null) return;

        // Setup the pass with current renderer and enqueue
        cctvPass.Setup(renderer);
        renderer.EnqueuePass(cctvPass);
    }
}

// ============================================================================
// CCTV RENDER PASS
// ============================================================================
// Executes the actual CCTV effect rendering

public class CCTVRenderPass : ScriptableRenderPass
{
    // ========================================================================
    // PRIVATE FIELDS
    // ========================================================================

    private readonly CCTVRenderFeature.CCTVSettings settings;
    private ScriptableRenderer renderer;

    // Temporary render texture for effect processing
    [Obsolete("Using legacy RenderTargetHandle - consider updating to RTHandle")]
    private readonly RenderTargetHandle tempTexture;

    private static readonly int TempTextureId = Shader.PropertyToID("_CCTVTempTexture");

    // ========================================================================
    // CONSTRUCTOR
    // ========================================================================

    /// <summary>
    ///     Initialize the CCTV render pass with settings
    /// </summary>
    /// <param name="settings">Configuration settings for the render pass</param>
    public CCTVRenderPass(CCTVRenderFeature.CCTVSettings settings)
    {
        this.settings = settings;
        renderPassEvent = settings.renderPassEvent;
        tempTexture.Init("_CCTVTempTexture");
    }

    // ========================================================================
    // PUBLIC METHODS
    // ========================================================================

    /// <summary>
    ///     Setup the render pass with the current renderer
    /// </summary>
    /// <param name="renderer">The scriptable renderer executing this pass</param>
    public void Setup(ScriptableRenderer renderer)
    {
        this.renderer = renderer;
    }

    // ========================================================================
    // RENDER PASS EXECUTION
    // ========================================================================

    /// <summary>
    ///     Execute the CCTV effect render pass
    /// </summary>
    [Obsolete("Using legacy rendering API - consider updating to newer URP version")]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Validate material exists
        if (settings.cctvMaterial == null) return;

        // Get CCTV effect from volume stack
        var stack = VolumeManager.instance.stack;
        var cctvEffect = stack.GetComponent<CCTVEffect>();

        // Skip if effect is not active
        if (cctvEffect == null || !cctvEffect.IsActive()) return;

        // ====================================================================
        // COMMAND BUFFER SETUP
        // ====================================================================

        var cmd = CommandBufferPool.Get("CCTV Effect");

        // Get camera color target (must be done inside Execute method)
        var colorTarget = renderer.cameraColorTarget;

        // Setup render texture descriptor
        var cameraData = renderingData.cameraData;
        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0; // No depth needed for post-processing

        // Allocate temporary render texture
        cmd.GetTemporaryRT(TempTextureId, descriptor);

        // ====================================================================
        // SHADER PARAMETER SETUP
        // ====================================================================

        // Apply volume component parameters to shader
        settings.cctvMaterial.SetFloat("_Intensity", cctvEffect.intensity.value);
        settings.cctvMaterial.SetFloat("_ScanLineIntensity", cctvEffect.scanLineIntensity.value);
        settings.cctvMaterial.SetFloat("_NoiseIntensity", cctvEffect.noiseIntensity.value);
        settings.cctvMaterial.SetFloat("_Desaturation", cctvEffect.desaturation.value);
        settings.cctvMaterial.SetFloat("_VignetteIntensity", cctvEffect.vignetteIntensity.value);

        // Time parameter handled by shader globals - no need to set manually
        // settings.cctvMaterial.SetFloat("_Time", Time.time);

        // Convert boolean parameters to float
        settings.cctvMaterial.SetFloat("_ShowTimestamp", cctvEffect.showTimestamp.value ? 1f : 0f);
        settings.cctvMaterial.SetFloat("_ShowScanLines", cctvEffect.showScanLines.value ? 1f : 0f);

        // ====================================================================
        // EFFECT RENDERING
        // ====================================================================

        // Apply CCTV effect: Source -> Temp (with effect) -> Destination
        cmd.Blit(colorTarget, TempTextureId, settings.cctvMaterial);
        cmd.Blit(TempTextureId, colorTarget);

        // ====================================================================
        // CLEANUP
        // ====================================================================

        // Release temporary resources
        cmd.ReleaseTemporaryRT(TempTextureId);

        // Execute commands and return buffer to pool
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}