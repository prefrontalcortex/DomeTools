using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

namespace Unity.XR.Management.AndroidManifest.Editor
{
    /// <summary>
    /// Class that receives the build event when building an Android Gradle project,
    /// so the manifest element processing can be executed.
    /// </summary>
    internal class AndroidManifestBuildEventReceiver : IPostGenerateGradleAndroidProject
    {

        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject(string gradleProjectPath)
        {
            var processor = CreateManifestProcessor(gradleProjectPath);

            var manifestProviders = GetManifestProviders();
            processor.ProcessManifestRequirements(manifestProviders);
        }

        private AndroidManifestProcessor CreateManifestProcessor(string gradleProjectPath)
        {
#if UNITY_2021_1_OR_NEWER
            var xrManagementPackagePath = EditorUtilities.GetPackagePath("com.unity.xr.management");
            return new AndroidManifestProcessor(gradleProjectPath, xrManagementPackagePath, GetXRManagerSettings());
#else
            return new AndroidManifestProcessor(gradleProjectPath, GetXRManagerSettings());
#endif
        }

        /// <summary>
        /// Finds all implementation of <see cref="IAndroidManifestRequirementProvider"/> in the assemblies,
        /// and creates instances for each type into a single collection.
        /// </summary>
        /// <returns><see cref="System.Collections.Generic.List{T}"/> collection of <see cref="IAndroidManifestRequirementProvider"/> instances. All contained objects are unique.</returns>
        private List<IAndroidManifestRequirementProvider> GetManifestProviders()
        {
            return TypeCache
                .GetTypesDerivedFrom<IAndroidManifestRequirementProvider>()
                .Where(type => !type.IsInterface && !type.IsAbstract && !type.IsNestedPrivate)
                .Select(providerType => Activator.CreateInstance(providerType)) // Instantiate providers
                .OfType<IAndroidManifestRequirementProvider>()
                .Distinct()
                .ToList();
        }

        private XRManagerSettings GetXRManagerSettings()
        {
            if (XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android))
            {
                return XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android).AssignedSettings;
            }

            return null;
        }
    }
}
