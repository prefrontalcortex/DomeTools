using System;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace UnityEditor.XR.OpenXR.Features.MetaQuestSupport
{
    internal class MetaQuestFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
        private const string kLateLatchingSupported = "xr-latelatching-enabled";
        private const string kLateLatchingDebug = "xr-latelatchingdebug-enabled";
        private const string kVulkanExtensionFragmentDensityMap = "xr-vulkan-extension-fragment-density-map-enabled";
        private const string kLowLatencyAudioEnabled = "xr-low-latency-audio-enabled";
        private const string kRequireBackbufferTextures = "xr-require-backbuffer-textures";
        private const string kKeyboardOverlayEnabled = "xr-keyboard-overlay-enabled";
        private const string kPipelineCacheEnabled = "xr-pipeline-cache-enabled";
        private const string kSkipB10G11R11SpecialCasing = "xr-skip-B10G11R11-special-casing";
        private const string kHideMemorylessRenderTexture = "xr-hide-memoryless-render-texture";
        private const string kSkipAudioBufferSizeCheck = "xr-skip-audio-buffer-size-check";
        private const string kUsableCoreMaskEnabled = "xr-usable-core-mask-enabled";

        private MetaQuestFeature GetMetaQuestFeature()
        {
            var featureGuids = AssetDatabase.FindAssets("t:" + typeof(MetaQuestFeature).Name);

            // we should only find one
            if (featureGuids.Length != 1)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(featureGuids[0]);
            return AssetDatabase.LoadAssetAtPath<MetaQuestFeature>(path);
        }

        public override int callbackOrder => 2;

        public override Type featureType => typeof(MetaQuestFeature);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        protected override void OnProcessBootConfigExt(BuildReport report, BootConfigBuilder builder)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            var item = GetMetaQuestFeature();
            if (item == null)
            {
                Debug.Log("Unable to locate the MetaQuestFeature Asset");
                return;
            }

            // Update the boot config
            builder.SetBootConfigBoolean(kLateLatchingSupported, item.lateLatchingMode);
            builder.SetBootConfigBoolean(kLateLatchingDebug, item.lateLatchingDebug);
            builder.SetBootConfigBoolean(kVulkanExtensionFragmentDensityMap, true);
            builder.SetBootConfigBoolean(kLowLatencyAudioEnabled, true);
            builder.SetBootConfigBoolean(kRequireBackbufferTextures, false);
            builder.SetBootConfigBoolean(kKeyboardOverlayEnabled, true);
            builder.SetBootConfigBoolean(kPipelineCacheEnabled, true);
            builder.SetBootConfigBoolean(kSkipB10G11R11SpecialCasing, true);
            builder.SetBootConfigBoolean(kHideMemorylessRenderTexture, true);
            builder.SetBootConfigBoolean(kSkipAudioBufferSizeCheck, true);
            builder.SetBootConfigBoolean(kUsableCoreMaskEnabled, true);
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }
    }
}
