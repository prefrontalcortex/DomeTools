using System;
using System.Collections.Generic;
#if !UNITY_2023_2_OR_NEWER
using System.Reflection;
#endif // !UNITY_2023_2_OR_NEWER
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEditor.XR.Hands.ProjectValidation;
using UnityEngine;

namespace UnityEngine.XR.Hands.Samples.GestureSample.Editor
{
    /// <summary>
    /// Unity Editor class which registers Project Validation rules for the Gestures sample,
    /// checking that other required samples and packages are installed.
    /// </summary>
    static class HandsSampleProjectValidation
    {
        const string k_SampleDisplayName = "Gestures Sample";
        const string k_Category = "XR Hands";
        const string k_HandVisualizerSampleName = "HandVisualizer";
        const string k_ProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";
        const string k_HandsPackageDisplayName = "XR Hands";
        const string k_HandsPackageName = "com.unity.xr.hands";
        const string k_InternalTestProjectName = "XRHandsSample";
        static readonly PackageVersion s_MinimumPackageVersion = new PackageVersion("1.2.1");
        static readonly PackageVersion s_RecommendedPackageVersion = new PackageVersion("1.5.0");

        static readonly BuildTargetGroup[] s_SupportedBuildTargets =
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android
        };

        static readonly List<BuildValidationRule> s_BuildValidationRules = new List<BuildValidationRule>
        {
            new BuildValidationRule
            {
                IsRuleEnabled = () => s_HandsPackageAddRequest == null || s_HandsPackageAddRequest.IsCompleted,
                Message = $"[{k_SampleDisplayName}] XR Hands ({k_HandsPackageName}) package must be at version {s_RecommendedPackageVersion} or higher to use the latest sample features.",
                Category = k_Category,
                CheckPredicate = () => PackageVersionUtility.GetPackageVersion(k_HandsPackageName) >= s_RecommendedPackageVersion,
                FixIt = () =>
                {
                    if (s_HandsPackageAddRequest == null || s_HandsPackageAddRequest.IsCompleted)
                        InstallOrUpdateHands();
                },
                FixItAutomatic = true,
                Error = false,
            },
            new BuildValidationRule
            {
                IsRuleEnabled = () => PackageVersionUtility.GetPackageVersion(k_HandsPackageName) >= s_MinimumPackageVersion,
                Message = $"[{k_SampleDisplayName}] {k_HandVisualizerSampleName} sample from XR Hands ({k_HandsPackageName}) package must be imported or updated to use this sample.",
                Category = k_Category,
                CheckPredicate = () => ProjectValidationUtility.SampleImportMeetsMinimumVersion(
                    k_HandsPackageDisplayName, k_HandVisualizerSampleName, PackageVersionUtility.GetPackageVersion(k_HandsPackageName)),
                FixIt = () =>
                {
                    if (TryFindSample(k_HandsPackageName, string.Empty, k_HandVisualizerSampleName, out var sample))
                    {
                        sample.Import(Sample.ImportOptions.OverridePreviousImports);
                    }
                },
                FixItAutomatic = true,
                Error = !ProjectValidationUtility.HasSampleImported(k_HandsPackageDisplayName, k_HandVisualizerSampleName),
            }
        };

        static AddRequest s_HandsPackageAddRequest;

        [InitializeOnLoadMethod]
        static void RegisterProjectValidationRules()
        {
            // Don't register validation rules for the internal test project.
            if (IsInternalTestProject())
                return;

            foreach (var buildTargetGroup in s_SupportedBuildTargets)
            {
                BuildValidator.AddRules(buildTargetGroup, s_BuildValidationRules);
            }

            // Delay evaluating conditions for issues to give time for Package Manager and UPM cache to fully initialize.
            EditorApplication.delayCall += ShowWindowIfIssuesExist;
        }

        static bool IsInternalTestProject()
        {
#if UNITY_2023_2_OR_NEWER
            var xrHandsPackage = UnityEditor.PackageManager.PackageInfo.FindForPackageName(k_HandsPackageName);
#else
            var xrHandsPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
#endif
            if (xrHandsPackage == null)
                return false;

            bool isLocalImport = xrHandsPackage.source == PackageSource.Local;

            bool isXRHandsSample = Application.dataPath.Contains(k_InternalTestProjectName);

            return isLocalImport && isXRHandsSample;
        }

        static void ShowWindowIfIssuesExist()
        {
            foreach (var validation in s_BuildValidationRules)
            {
                if (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke())
                {
                    ShowWindow();
                    return;
                }
            }
        }

        internal static void ShowWindow()
        {
            // Delay opening the window since sometimes other settings in the player settings provider redirect to the
            // project validation window causing serialized objects to be nullified.
            EditorApplication.delayCall += () =>
            {
                SettingsService.OpenProjectSettings(k_ProjectValidationSettingsPath);
            };
        }

        static bool TryFindSample(string packageName, string packageVersion, string sampleDisplayName, out Sample sample)
        {
            sample = default;

            if (!PackageVersionUtility.IsPackageInstalled(packageName))
                return false;

            IEnumerable<Sample> packageSamples;
            try
            {
                packageSamples = Sample.FindByPackage(packageName, packageVersion);
            }
            catch (Exception e)
            {
                Debug.LogError($"Couldn't find samples of the {ToString(packageName, packageVersion)} package; aborting project validation rule. Exception: {e}");
                return false;
            }

            if (packageSamples == null)
            {
                Debug.LogWarning($"Couldn't find samples of the {ToString(packageName, packageVersion)} package; aborting project validation rule.");
                return false;
            }

            foreach (var packageSample in packageSamples)
            {
                if (packageSample.displayName == sampleDisplayName)
                {
                    sample = packageSample;
                    return true;
                }
            }

            Debug.LogWarning($"Couldn't find {sampleDisplayName} sample in the {ToString(packageName, packageVersion)} package; aborting project validation rule.");
            return false;
        }

        static string ToString(string packageName, string packageVersion)
        {
            return string.IsNullOrEmpty(packageVersion) ? packageName : $"{packageName}@{packageVersion}";
        }

        static void InstallOrUpdateHands()
        {
            // Set a 3-second timeout for request to avoid editor lockup
            var currentTime = DateTime.Now;
            var endTime = currentTime + TimeSpan.FromSeconds(3);

            var request = Client.Search(k_HandsPackageName);
            if (request.Status == StatusCode.InProgress)
            {
                Debug.Log($"Searching for ({k_HandsPackageName}) in Unity Package Registry.");
                while (request.Status == StatusCode.InProgress && currentTime < endTime)
                    currentTime = DateTime.Now;
            }

            var addRequest = k_HandsPackageName;
            if (request.Status == StatusCode.Success && request.Result.Length > 0)
            {
                var versions = request.Result[0].versions;
#if UNITY_2022_2_OR_NEWER
                var recommendedVersion = new PackageVersion(versions.recommended);
#else
                var recommendedVersion = new PackageVersion(versions.verified);
#endif
                var latestCompatible = new PackageVersion(versions.latestCompatible);
                if (recommendedVersion < s_RecommendedPackageVersion && s_RecommendedPackageVersion <= latestCompatible)
                    addRequest = $"{k_HandsPackageName}@{s_RecommendedPackageVersion}";
            }

            s_HandsPackageAddRequest = Client.Add(addRequest);
            if (s_HandsPackageAddRequest.Error != null)
            {
                Debug.LogError($"Package installation error: {s_HandsPackageAddRequest.Error}: {s_HandsPackageAddRequest.Error.message}");
            }
        }
    }
}
