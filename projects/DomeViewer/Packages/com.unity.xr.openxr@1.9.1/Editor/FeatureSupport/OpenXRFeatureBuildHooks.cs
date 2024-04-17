using System;
using System.Collections.Generic;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#if XR_MGMT_4_4_0_OR_NEWER
using Unity.XR.Management.AndroidManifest.Editor;
#endif

namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// Inherit from this class to get callbacks to hook into the build process when your OpenXR Extension is enabled.
    /// </summary>
    public abstract class OpenXRFeatureBuildHooks : IPostGenerateGradleAndroidProject, IPostprocessBuildWithReport, IPreprocessBuildWithReport
#if XR_MGMT_4_4_0_OR_NEWER
        , IAndroidManifestRequirementProvider
#endif
    {
        private OpenXRFeature _ext;
        private readonly BootConfigBuilder _bootConfigBuilder = new BootConfigBuilder();

        private bool IsExtensionEnabled(BuildTarget target, BuildTargetGroup group)
        {
            if (!BuildHelperUtils.HasLoader(group, typeof(OpenXRLoaderBase)))
                return false;

            if (OpenXRSettings.ActiveBuildTargetInstance == null || OpenXRSettings.ActiveBuildTargetInstance.features == null)
                return false;

            if (_ext == null || _ext.GetType() != featureType)
            {
                foreach (var ext in OpenXRSettings.ActiveBuildTargetInstance.features)
                {
                    if (featureType == ext.GetType())
                    {
                        _ext = ext;
                    }
                }
            }

            return _ext != null && _ext.enabled;
        }

        /// <summary>
        /// Returns the current callback order for build processing.
        /// </summary>
        /// <value>Int value denoting the callback order.</value>
        public abstract int callbackOrder { get; }

        /// <summary>
        /// Post process build step for checking if a feature is enabled. If so will call to the feature to run their build pre processing.
        /// </summary>
        /// <param name="report">Build report.</param>
        public virtual void OnPreprocessBuild(BuildReport report)
        {
            if (!IsExtensionEnabled(report.summary.platform, report.summary.platformGroup))
                return;

            _bootConfigBuilder.ReadBootConfig(report);

            OnProcessBootConfigExt(report, _bootConfigBuilder);

            OnPreprocessBuildExt(report);

            _bootConfigBuilder.WriteBootConfig(report);
        }

        /// <summary>
        /// Post process build step for checking if a feature is enabled for android builds. If so will call to the feature to run their build post processing for android builds.
        /// </summary>
        /// <param name="path">Path to gradle project.</param>
        public virtual void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!IsExtensionEnabled(BuildTarget.Android, BuildTargetGroup.Android))
                return;

            OnPostGenerateGradleAndroidProjectExt(path);
        }

        /// <summary>
        /// Post-process build step for any necessary clean-up. This will also call to the feature to run their build post processing.
        /// </summary>
        /// <param name="report">Build report.</param>
        public virtual void OnPostprocessBuild(BuildReport report)
        {
            if (!IsExtensionEnabled(report.summary.platform, report.summary.platformGroup))
                return;

            OnPostprocessBuildExt(report);

            _bootConfigBuilder.ClearAndWriteBootConfig(report);
        }

        /// <summary>
        /// System.Type of the class that implements OpenXRFeature.
        /// </summary>
        public abstract Type featureType { get; }

        /// <summary>
        /// Called during the build process when the feature is enabled. Implement this function to receive a callback before the build starts.
        /// </summary>
        /// <param name="report">Report that contains information about the build, such as its target platform and output path.</param>
        protected abstract void OnPreprocessBuildExt(BuildReport report);

        /// <summary>
        /// Called during build process when extension is enabled. Implement this function to receive a callback after the Android Gradle project is generated and before building begins. Function is not called for Internal builds.
        /// </summary>
        /// <param name="path">The path to the root of the Gradle project. Note: When exporting the project, this parameter holds the path to the folder specified for export.</param>
        protected abstract void OnPostGenerateGradleAndroidProjectExt(string path);

        /// <summary>
        /// Called during the build process when extension is enabled. Implement this function to receive a callback after the build is complete.
        /// </summary>
        /// <param name="report">BuildReport that contains information about the build, such as the target platform and output path.</param>
        protected abstract void OnPostprocessBuildExt(BuildReport report);

        /// <summary>
        /// Called during the build process when extension is enabled. Implement this function to add Boot Config Settings.
        /// </summary>
        /// <param name="report">BuildReport that contains information about the build, such as the target platform and output path.</param>
        /// <param name="builder">This is the Boot Config interface tha can be used to write boot configs</param>
        protected virtual void OnProcessBootConfigExt(BuildReport report, BootConfigBuilder builder)
        {
        }

#if XR_MGMT_4_4_0_OR_NEWER
        /// <summary>
        /// Called during build process when collecting requirements for Android Manifest. Implement this function to add, override or remove Android manifest entries.
        /// </summary>
        public virtual ManifestRequirement ProvideManifestRequirement()
        {
            return null;
        }

#endif
    }
}
