using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR
{
    internal class UWPCoreWindowBuildHooks : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // After MixedRealityBuildProcessor, if it's in the project.
        public int callbackOrder => 2;

        private static readonly Dictionary<string, string> BootVars = new Dictionary<string, string>()
        {
            {"force-primary-window-holographic", "1"},
            {"vr-enabled", "1"},
            {"xrsdk-windowsmr-library", "UnityOpenXR.dll"},
            {"early-boot-windows-holographic", "1"},
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WSAPlayer)
                return;

            if (!BuildHelperUtils.HasLoader(BuildTargetGroup.WSA, typeof(OpenXRLoaderBase)))
                return;

            var bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            var initXRManagerOnStart = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.WSA).InitManagerOnStart;
            BootVars["vr-enabled"] = initXRManagerOnStart ? "1" : "0";

            // MixedRealityBuildProcessor may skip setting `force-primary-window-holographic` in certain cases:
            //
            //     When AppRemoting is enabled, skip the flag to force primary corewindow to be holographic (it won't be).
            //     If this flag exist, Unity might hit a bug that it skips rendering into the CoreWindow on the desktop.
            var skipPrimaryWindowHolographic = bootConfig.TryGetValue("xrsdk-windowsmr-library", out var unused1) &&
                (bootConfig.TryGetValue("vr-enabled", out var vrEnabled) && vrEnabled == "1") &&
                (bootConfig.TryGetValue("early-boot-windows-holographic", out var earlyBoot) && earlyBoot == "1") &&
                (!bootConfig.TryGetValue("force-primary-window-holographic", out var forceHolographic) || forceHolographic == "0");

            foreach (var entry in BootVars)
            {
                if (entry.Key == "force-primary-window-holographic" && skipPrimaryWindowHolographic)
                    continue;

                bootConfig.SetValueForKey(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WSAPlayer)
                return;

            if (!BuildHelperUtils.HasLoader(BuildTargetGroup.WSA, typeof(OpenXRLoaderBase)))
                return;

            // Clean up boot settings after build
            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (KeyValuePair<string, string> entry in BootVars)
            {
                bootConfig.ClearEntryForKeyAndValue(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();
        }
    }
}
