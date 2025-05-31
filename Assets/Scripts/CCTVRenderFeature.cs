using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CCTVRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CCTVSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material cctvMaterial;
    }

    public CCTVSettings settings = new CCTVSettings();
    private CCTVRenderPass cctvPass;

    public override void Create()
    {
        cctvPass = new CCTVRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.cctvMaterial == null) return;

        // Don't access cameraColorTarget here - pass the renderer instead
        cctvPass.Setup(renderer);
        renderer.EnqueuePass(cctvPass);
    }
}

public class CCTVRenderPass : ScriptableRenderPass
{
    private CCTVRenderFeature.CCTVSettings settings;
    private ScriptableRenderer renderer;
    private RenderTargetHandle tempTexture;
    private static readonly int TempTextureId = Shader.PropertyToID("_CCTVTempTexture");

    public CCTVRenderPass(CCTVRenderFeature.CCTVSettings settings)
    {
        this.settings = settings;
        renderPassEvent = settings.renderPassEvent;
        tempTexture.Init("_CCTVTempTexture");
    }

    public void Setup(ScriptableRenderer renderer)
    {
        this.renderer = renderer;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings.cctvMaterial == null) return;

        var stack = VolumeManager.instance.stack;
        var cctvEffect = stack.GetComponent<CCTVEffect>();

        if (cctvEffect == null || !cctvEffect.IsActive()) return;

        CommandBuffer cmd = CommandBufferPool.Get("CCTV Effect");

        // Get the camera color target here, inside the execute method
        var colorTarget = renderer.cameraColorTarget;

        // Get camera descriptor
        var cameraData = renderingData.cameraData;
        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        // Get temporary render texture
        cmd.GetTemporaryRT(TempTextureId, descriptor);

        // Set shader properties
        settings.cctvMaterial.SetFloat("_Intensity", cctvEffect.intensity.value);
        settings.cctvMaterial.SetFloat("_ScanLineIntensity", cctvEffect.scanLineIntensity.value);
        settings.cctvMaterial.SetFloat("_NoiseIntensity", cctvEffect.noiseIntensity.value);
        settings.cctvMaterial.SetFloat("_Desaturation", cctvEffect.desaturation.value);
        settings.cctvMaterial.SetFloat("_VignetteIntensity", cctvEffect.vignetteIntensity.value);
        //settings.cctvMaterial.SetFloat("_Time", Time.time);
        settings.cctvMaterial.SetFloat("_ShowTimestamp", cctvEffect.showTimestamp.value ? 1f : 0f);
        settings.cctvMaterial.SetFloat("_ShowScanLines", cctvEffect.showScanLines.value ? 1f : 0f);

        // Blit with CCTV effect
        cmd.Blit(colorTarget, TempTextureId, settings.cctvMaterial);
        cmd.Blit(TempTextureId, colorTarget);

        // Cleanup
        cmd.ReleaseTemporaryRT(TempTextureId);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}