using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CCTVFullscreenFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CCTVSettings
    {
        [Header("CCTV Effect Settings")]
        [Range(0f, 1f)] public float scanlineIntensity = 0.3f;
        [Range(1f, 50f)] public float scanlineCount = 25f;
        [Range(0f, 1f)] public float vignetteIntensity = 0.4f;
        [Range(0f, 1f)] public float noiseIntensity = 0.1f;
        [Range(0f, 1f)] public float distortionAmount = 0.02f;
        [Range(0f, 1f)] public float colorDesaturation = 0.6f;
        [ColorUsage(false)] public Color tintColor = new Color(0.8f, 1f, 0.8f, 1f);

        [Header("Render Settings")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader cctvShader;
    }

    public CCTVSettings settings = new CCTVSettings();
    private CCTVFullscreenPass cctvPass;
    private Material cctvMaterial;

    public override void Create()
    {
        cctvPass = new CCTVFullscreenPass(settings);

        // Create material from shader
        if (settings.cctvShader != null)
        {
            cctvMaterial = new Material(settings.cctvShader);
            cctvPass.SetMaterial(cctvMaterial);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.cctvShader == null)
        {
            Debug.LogWarningFormat("Missing CCTV Shader in {0}", GetType().Name);
            return;
        }

        if (cctvMaterial == null)
        {
            cctvMaterial = new Material(settings.cctvShader);
            cctvPass.SetMaterial(cctvMaterial);
        }

        // Only apply to Game and Scene cameras
        if (renderingData.cameraData.cameraType != CameraType.Game &&
            renderingData.cameraData.cameraType != CameraType.SceneView)
        {
            return;
        }

        cctvPass.Setup(settings);
        renderer.EnqueuePass(cctvPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (cctvMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(cctvMaterial);
            else
                DestroyImmediate(cctvMaterial);
        }
    }

    class CCTVFullscreenPass : ScriptableRenderPass
    {
        private CCTVSettings settings;
        private Material material;
        private RTHandle temporaryColorTexture;

        // Shader property IDs
        private static readonly int ScanlineIntensityID = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int ScanlineCountID = Shader.PropertyToID("_ScanlineCount");
        private static readonly int VignetteIntensityID = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int NoiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int DistortionAmountID = Shader.PropertyToID("_DistortionAmount");
        private static readonly int ColorDesaturationID = Shader.PropertyToID("_ColorDesaturation");
        private static readonly int TintColorID = Shader.PropertyToID("_TintColor");
        private static readonly int TimeID = Shader.PropertyToID("_Time");

        public CCTVFullscreenPass(CCTVSettings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;

            // Configure what this pass reads from
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public void SetMaterial(Material material)
        {
            this.material = material;
        }

        public void Setup(CCTVSettings newSettings)
        {
            this.settings = newSettings;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Get camera target descriptor
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0; // We don't need depth for post-processing

            // Allocate temporary texture
            RenderingUtils.ReAllocateIfNeeded(ref temporaryColorTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_CCTVTempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("CCTV Fullscreen Effect");

            // Get the camera color target
            RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Update shader properties
            material.SetFloat(ScanlineIntensityID, settings.scanlineIntensity);
            material.SetFloat(ScanlineCountID, settings.scanlineCount);
            material.SetFloat(VignetteIntensityID, settings.vignetteIntensity);
            material.SetFloat(NoiseIntensityID, settings.noiseIntensity);
            material.SetFloat(DistortionAmountID, settings.distortionAmount);
            material.SetFloat(ColorDesaturationID, settings.colorDesaturation);
            material.SetColor(TintColorID, settings.tintColor);
            material.SetTexture("_BlitTexture", cameraColorTarget);


            // Perform fullscreen blit
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, temporaryColorTexture, material, 0);
            Blitter.BlitCameraTexture(cmd, temporaryColorTexture, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Cleanup is handled automatically by RTHandle system
        }

        public void Dispose()
        {
            temporaryColorTexture?.Release();
        }
    }
}