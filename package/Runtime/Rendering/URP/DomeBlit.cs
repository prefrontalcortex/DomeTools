#if HAVE_RENDERPIPELINE_UNIVERSAL || HAVE_RENDERPIPELINE_LWRP
#define HAVE_RENDERPIPELINE
#endif

using UnityEngine;
#if HAVE_RENDERPIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#elif HAVE_RENDERPIPELINE_LWRP
using UnityEngine.Rendering.LWRP;
#endif

#if HAVE_RENDERPIPELINE
namespace pfc.DomeTools.Rendering
{
    public class DomeBlit : ScriptableRendererFeature
    {
        [System.Serializable]
        public class BlitSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

            public Material blitMaterial = null;
            public int blitMaterialPassIndex = -1;
            public Target destination = Target.Color;
            public string textureId = "_BlitPassTexture";
        }

        public enum Target
        {
            Color,
            Texture,
            JustRender
        }

        public BlitSettings settings = new BlitSettings();
        RenderTargetHandle m_RenderTextureHandle;

        DomeBlitPass domeBlitPass;

        public override void Create()
        {
            var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
            domeBlitPass = new DomeBlitPass(settings.Event, settings.blitMaterial, settings.blitMaterialPassIndex, name);
            if (settings.destination != Target.JustRender)
                m_RenderTextureHandle.Init(settings.textureId);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTarget;
            var dest = (settings.destination == Target.Color) ? RenderTargetHandle.CameraTarget : m_RenderTextureHandle;

            if (settings.blitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            domeBlitPass.Setup(src, dest);
            renderer.EnqueuePass(domeBlitPass);
        }
    }
}
#endif