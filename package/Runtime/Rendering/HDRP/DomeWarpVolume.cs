#if HAVE_RENDERPIPELINE_HDRP

using UnityEngine;
using UnityEngine.Rendering;
using System;
using pfc.DomeTools;

using UnityEngine.Rendering.HighDefinition;

// Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings)!
[Serializable, VolumeComponentMenu("Post-processing/Custom/Dome Warp Volume")]
public sealed class DomeWarpVolume : CustomPostProcessVolumeComponent, IPostProcessComponent
{

    private Material m_Material;

    public override bool visibleInSceneView => false;
    public bool IsActive() => true; //m_Material != null && intensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings)!
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if(!domemasterFisheye)
            domemasterFisheye = FindAnyObjectByType<DomeWarp>();
    }

    DomeWarp domemasterFisheye;

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if(!domemasterFisheye)
            domemasterFisheye = FindAnyObjectByType<DomeWarp>();
        if(!domemasterFisheye) return;
        if(!domemasterFisheye.enabled) return;

        m_Material = domemasterFisheye.matHDRP;

        if (!m_Material) return;

        // m_Material.SetFloat("_Intensity", intensity.value);
        m_Material.SetTexture("_InputTexture", source);
        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup()
    {
        // CoreUtils.Destroy(m_Material);
    }
}

#endif