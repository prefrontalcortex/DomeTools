using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Unity.XR.Management.AndroidManifest.Editor
{
    /// <summary>
    /// Class that retrieves Android manifest entries required by classes that implement the IAndroidManifestEntryProvider interface.
    /// </summary>
    internal class AndroidManifestProcessor
    {
        private static readonly string k_androidManifestFileName = "AndroidManifest.xml";
#if UNITY_2021_1_OR_NEWER
        private static readonly string k_xrLibraryDirectoryName = "xrmanifest.androidlib";
        private static readonly string k_xrLibraryManifestRelativePath = string.Join(Path.DirectorySeparatorChar.ToString(), k_xrLibraryDirectoryName, k_androidManifestFileName);
#endif

        private readonly string m_unityLibraryManifestFilePath;
#if UNITY_2021_1_OR_NEWER
        private readonly string m_xrPackageManifestTemplateFilePath;
        private readonly string m_xrLibraryManifestFilePath;
#endif
        private readonly XRManagerSettings m_xrSettings;

        internal AndroidManifestProcessor(string gradleProjectPath, XRManagerSettings settings)
        {
            m_unityLibraryManifestFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), gradleProjectPath, "src", "main", k_androidManifestFileName);
            m_xrSettings = settings;
        }


#if UNITY_2021_1_OR_NEWER
        internal AndroidManifestProcessor(
            string gradleProjectPath,
            string xrManagementPackagePath,
            XRManagerSettings settings)
        {
            m_xrPackageManifestTemplateFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), xrManagementPackagePath, k_xrLibraryManifestRelativePath);
            m_xrLibraryManifestFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), gradleProjectPath, k_xrLibraryManifestRelativePath);
            m_unityLibraryManifestFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), gradleProjectPath, "src", "main", k_androidManifestFileName);
            m_xrSettings = settings;
        }
#endif

        internal void ProcessManifestRequirements(List<IAndroidManifestRequirementProvider> manifestProviders)
        {
            var activeLoaders = GetActiveLoaderList();

            // Get manifest entries from providers
            var manifestRequirements = manifestProviders
                .Select(provider => provider.ProvideManifestRequirement())
                .OfType<ManifestRequirement>()
                .Distinct()
                // Requirements can apply to different platforms, so we filter out those whose loaders aren't currently active
                .Where(requirement => requirement.SupportedXRLoaders.Any(loaderType => activeLoaders.Contains(loaderType)))
                .ToList();

            var mergedRequiredElements =
                MergeElements(
                    manifestRequirements
                    .SelectMany(requirement => requirement.OverrideElements));
            var elementsToBeRemoved = manifestRequirements
                .SelectMany(requirement => requirement.RemoveElements)
                .OfType<ManifestElement>();

#if UNITY_2021_1_OR_NEWER
            // The intent-filter elements are not merged by default,
            // so we separate them from the XR manifest to add them later.
            // Otherwise, the application won't load correctly.
            var newRequiredElements = manifestRequirements
                .SelectMany(requirement => requirement.NewElements)
                .Where(element => !element.ElementPath.Contains("intent-filter"));
            var newIntentElements = manifestRequirements
                .SelectMany(requirement => requirement.NewElements)
                .Where(element => element.ElementPath.Contains("intent-filter"));

            {
                var xrLibraryManifest = new AndroidManifestDocument(m_xrPackageManifestTemplateFilePath);
                var unityLibraryManifest = new AndroidManifestDocument(m_unityLibraryManifestFilePath);

                xrLibraryManifest.CreateElements(newRequiredElements);
                xrLibraryManifest.OverrideElements(mergedRequiredElements);
                unityLibraryManifest.CreateElements(newIntentElements, false); // Add the intents in the unity library manifest
                unityLibraryManifest.RemoveElements(elementsToBeRemoved);

                // Write updated manifests
                xrLibraryManifest.SaveAs(m_xrLibraryManifestFilePath);
                unityLibraryManifest.Save();
            }
#else
            var newRequiredElements = manifestRequirements
                .SelectMany(requirement => requirement.NewElements);

            { 
                var manifest = new AndroidManifestDocument(m_unityLibraryManifestFilePath);

                manifest.CreateElements(newRequiredElements);
                manifest.OverrideElements(mergedRequiredElements);
                manifest.RemoveElements(elementsToBeRemoved);

                // Write manifest into project's library path
                manifest.Save();
            }
#endif
        }

        /// <summary>
        /// Merges the elements of given <see cref="IEnumerable{T}"/> of type <see cref="ManifestElement"/> based on their <see cref="ManifestElement.ElementPath"/>.
        /// Their key-value pair attributes are deduped and merged into a single element.
        /// </summary>
        /// <param name="source"><see cref="IEnumerable{T}"/> of type <see cref="ManifestElement"/> containing all elements to be merged.</param>
        /// <returns>Filtered <see cref="IEnumerable{T}"/> of type <see cref="ManifestElement"/> with unique elements.</returns>
        private IEnumerable<ManifestElement> MergeElements(IEnumerable<ManifestElement> source)
        {
            return source
                .GroupBy(
                    (requirement) => requirement.ElementPath,
                    (requirement) => requirement,
                    (elementPath, groupedRequirements) => {
                        var mergedAttributes = groupedRequirements
                            .SelectMany(requirement => requirement.Attributes)
                            .Distinct()
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                        return new ManifestElement
                        {
                            ElementPath = elementPath,
                            Attributes = mergedAttributes
                        };
                    });
        }

        private List<Type> GetActiveLoaderList()
        {       
            if (!m_xrSettings)
            {
                // No loaders active, don't throw error
                Debug.LogWarning("No XR Manager settings found, manifest entries will not be updated.");
                return new List<Type>();
            }

            return m_xrSettings.activeLoaders
                .Select(loader => loader.GetType())
                .ToList();
        }
    }
}
