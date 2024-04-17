using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;
using UnityEngine.XR.Management.Tests;
using Object = UnityEngine.Object;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX

namespace UnityEditor.XR.Management.Tests.BuildTests
{
    static class BuildTestHelpers
    {
        public const string k_TemporaryTestPath = "Assets/Hidden_XRManagement_Test_Assets";
        public const string k_TempAssetSearchTerm = "TestGeneralAsset";
        public const string k_AssetName = "TestGeneralAsset.asset";

        public static void SetupTemporaryTestAssets(BuildTargetGroup buildTargetGroup, out XRGeneralSettingsPerBuildTarget previousGeneralSettingsInstance)
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out previousGeneralSettingsInstance);
            EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);

            var emptyBuildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            generalSettings.AssignedSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            emptyBuildTargetSettings.SetSettingsForBuildTarget(buildTargetGroup, generalSettings);
            emptyBuildTargetSettings.SettingsForBuildTarget(buildTargetGroup).AssignedSettings.TrySetLoaders(new List<XRLoader>());

            Directory.CreateDirectory(k_TemporaryTestPath);
            AssetDatabase.CreateAsset(emptyBuildTargetSettings, Path.Combine(k_TemporaryTestPath, k_AssetName));

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, emptyBuildTargetSettings, true);
        }

        public static void CleanupTemporaryAssets()
        {
            AssetDatabase.DeleteAsset(Path.Combine(k_TemporaryTestPath, k_AssetName));
            if (Directory.Exists(Path.Combine("./", k_TemporaryTestPath)))
            {
                Directory.Delete(Path.Combine("./", k_TemporaryTestPath));
                File.Delete($"./{k_TemporaryTestPath}.meta");
                AssetDatabase.Refresh();
            }
        }

        // Must match XRGeneralSettingsPerBuildTarget.TryFindSettingsAsset
        public static bool TryFindTemporarySettingsAsset(out XRGeneralSettingsPerBuildTarget buildTargetSettings)
        {
            EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
            {
                var assets = AssetDatabase.FindAssets(k_TempAssetSearchTerm);
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    buildTargetSettings = AssetDatabase.LoadAssetAtPath(path, typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                }
            }
            return buildTargetSettings != null;
        }
    }

#if UNITY_EDITOR_WIN
    [TestFixture(GraphicsDeviceType.Direct3D11, false, GraphicsDeviceType.Direct3D11, null)]
    [TestFixture(GraphicsDeviceType.Direct3D11, false, GraphicsDeviceType.Direct3D12, GraphicsDeviceType.Direct3D11)]
    [TestFixture(GraphicsDeviceType.Direct3D11, true, GraphicsDeviceType.Direct3D12, GraphicsDeviceType.Vulkan)]
    [TestFixture(GraphicsDeviceType.Direct3D11, false, GraphicsDeviceType.Null, GraphicsDeviceType.Vulkan)]
    [TestFixture(GraphicsDeviceType.Direct3D11, false, GraphicsDeviceType.Vulkan, GraphicsDeviceType.Null)]
#elif UNITY_EDITOR_OSX
    [TestFixture(GraphicsDeviceType.Metal, false, GraphicsDeviceType.Metal, null)]
    [TestFixture(GraphicsDeviceType.Metal, false, GraphicsDeviceType.Direct3D12, GraphicsDeviceType.Metal)]
    [TestFixture(GraphicsDeviceType.Metal, true, GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan)]
    [TestFixture(GraphicsDeviceType.Metal, false, GraphicsDeviceType.Null, GraphicsDeviceType.Vulkan)]
    [TestFixture(GraphicsDeviceType.Metal, false, GraphicsDeviceType.Vulkan, GraphicsDeviceType.Null)]
#endif
    class GraphicsAPICompatibilityTests
    {
        XRManagerSettings m_Manager;
        List<XRLoader> m_Loaders = new List<XRLoader>();

        private GraphicsDeviceType m_PlayerSettingsDeviceType;
        private GraphicsDeviceType[]  m_LoadersSupporteDeviceTypes;
        bool m_BuildFails;

        public GraphicsAPICompatibilityTests(GraphicsDeviceType playerSettingsDeviceType, bool fails, GraphicsDeviceType loader0, GraphicsDeviceType? loader1)
        {
            m_BuildFails = fails;
            m_PlayerSettingsDeviceType = playerSettingsDeviceType;
            if (loader1.HasValue)
            {
                m_LoadersSupporteDeviceTypes = new[] {loader0, loader1.Value};
            }
            else {
                m_LoadersSupporteDeviceTypes = new [] {loader0};
            }
        }

        [SetUp]
        public void SetupPlayerSettings()
        {
#if UNITY_EDITOR_WIN
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { m_PlayerSettingsDeviceType });
#elif UNITY_EDITOR_OSX
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, new[] { m_PlayerSettingsDeviceType });
#endif
            m_Manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            m_Manager.automaticLoading = false;

            m_Loaders = new List<XRLoader>();

            for (int i = 0; i < m_LoadersSupporteDeviceTypes.Length; i++)
            {
                DummyLoader dl = ScriptableObject.CreateInstance(typeof(DummyLoader)) as DummyLoader;
                dl.id = i;
                dl.supportedDeviceType = m_LoadersSupporteDeviceTypes[i];
                m_Loaders.Add(dl);
            }
            m_Manager.TrySetLoaders(m_Loaders);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Manager);
            m_Manager = null;

        }

        [Test]
        public void CheckGraphicsAPICompatibilityOnBuild()
        {
            try
            {
                XRGeneralBuildProcessor.VerifyGraphicsAPICompatibility(m_Manager, m_PlayerSettingsDeviceType);
            }
            catch (BuildFailedException)
            {
                Assert.True(m_BuildFails);
                return;
            }

            Assert.False(m_BuildFails);
        }
    }

    [TestFixture(BuildTargetGroup.Standalone)]
    [TestFixture(BuildTargetGroup.Android)]
    [TestFixture(BuildTargetGroup.iOS)]
#if (!UNITY_2021_2_OR_NEWER)
    [TestFixture(BuildTargetGroup.Lumin)]
#endif
    [TestFixture(BuildTargetGroup.PS4)]
    class XRGeneralSettingsBuildTests
    {
        BuildTargetGroup m_BuildTargetGroup;

        XRGeneralSettingsPerBuildTarget m_OldBuildTargetSettings;

        public XRGeneralSettingsBuildTests(BuildTargetGroup group)
        {
            m_BuildTargetGroup = group;
        }

        void CleanupOldSettings() => BuildHelpers.CleanOldSettings<XRGeneralSettings>();

        [SetUp]
        public void SetupPlayerSettings()
        {
            BuildTestHelpers.SetupTemporaryTestAssets(m_BuildTargetGroup, out m_OldBuildTargetSettings);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_OldBuildTargetSettings != null)
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, m_OldBuildTargetSettings, true);

            BuildTestHelpers.CleanupTemporaryAssets();
        }

        [Test]
        public void CheckEmptyXRGeneralAssetWillNotGetIncludedInAssets()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            Assert.False(buildTargetSettings == null);

            var settings = buildTargetSettings.SettingsForBuildTarget(m_BuildTargetGroup);
            Assert.False(settings == null);

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            var settingsIncludedInPreloadedAssets = preloadedAssets.Contains(settings);

            // Use the logic in XRGeneralBuildProcessor.OnPreprocessBuild() to determine if the XR General Settings will
            // be include or not.
            if (!settingsIncludedInPreloadedAssets && settings.AssignedSettings.activeLoaders.Count > 0)
            {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            else
            {
                CleanupOldSettings();
            }

            Assert.False(PlayerSettings.GetPreloadedAssets().Contains(settings));
        }
    }

    [TestFixture]
    class XRGeneralSettingsPerBuildTargetExclusionTests
    {
        static readonly string k_DefaultAssetPathRoot = Path.GetFullPath(Path.Combine(".", "Assets", EditorUtilities.s_DefaultGeneralSettingsPath[0]));

        XRGeneralSettingsPerBuildTarget m_PreviousSettings;

        [SetUp]
        public void SetupPlayerSettings()
        {
            if (EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out _))
            {
                EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);
            }

            AssetDatabase.DeleteAsset("Assets/XR");
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CheckIfSettingsAreNotAddedIfNoSettingsExistInTheProject()
        {
            // Ensure Setup Functioned properly.
            Assert.False(
                EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var buildSettings),
                "Build Settings contains a settings config object when it should not.");

            Assert.False(
                XRGeneralBuildProcessor.TryGetSettingsPerBuildTarget(out buildSettings),
                "Build Settings Configuration Object exists when it shouldn't in the current project");
        }
    }

    /// <summary>
    /// A set of tests that are explicitly designed to ensure if
    /// an <see cref="XRGeneralSettingsPerBuildTarget"/> asset exists
    /// in the current project that it would get processed.
    /// </summary>
    /// <remarks>
    /// Functionally the opposite of the <see cref="XRGeneralSettingsPerBuildTargetBuildExclusionTests"/>
    /// except that these tests will always run since it's possible to create just test assets.
    /// </remarks>
    [TestFixture(BuildTargetGroup.Standalone)]
    [TestFixture(BuildTargetGroup.Android)]
    [TestFixture(BuildTargetGroup.iOS)]
#if (!UNITY_2021_2_OR_NEWER)
    [TestFixture(BuildTargetGroup.Lumin)]
#endif
    [TestFixture(BuildTargetGroup.PS4)]
    class XRGeneralSettingsPerBuildTargetInclusionTests
    {
        const string k_DefaultAssetPath = "Assets/XR";
        const string k_DefaultAssetName = "XRGeneralSettings.asset";

        BuildTargetGroup m_BuildTargetGroup;
        XRGeneralSettingsPerBuildTarget m_OldBuildTargetSettings;

        public XRGeneralSettingsPerBuildTargetInclusionTests(BuildTargetGroup group)
        {
            m_BuildTargetGroup = group;
        }

        [SetUp]
        public void SetupPlayerSettings()
        {
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out m_OldBuildTargetSettings))
            {
                // If not then we get or create the asset and delete it in the teardown
                XRGeneralSettingsPerBuildTarget.GetOrCreate();
                AssetDatabase.Refresh();
            }
            else
            {
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, m_OldBuildTargetSettings, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (m_OldBuildTargetSettings == null)
            {
                EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);

                // Asset did not exists prior to test running so delete.
                AssetDatabase.DeleteAsset(Path.Combine(k_DefaultAssetPath, k_DefaultAssetName));
                if (Directory.Exists(Path.Combine("./", k_DefaultAssetPath)))
                {
                    Directory.Delete(Path.Combine("./", k_DefaultAssetPath), true);
                    File.Delete($"./{k_DefaultAssetPath}.meta");
                    AssetDatabase.Refresh();
                }
            }
        }

        [Test]
        public void EnsureSettingsAreIncludedWhenAssetExists()
        {
            // Ensure Setup Functioned properly.
            Assert.True(XRGeneralBuildProcessor.TryGetSettingsPerBuildTarget(out var originalSettings),
                "XRGeneralSettingsPerBuildTarget should be included at the beginning of the test.");

            // Remove the settings to check if the code functions properly
            EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);

            Assert.True(XRGeneralBuildProcessor.TryGetSettingsPerBuildTarget(out var generalSettings),
                "The asset should exist even if it's been removed from the editor configuration.");
            Assert.True(EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings),
                "The configuration object was not set by the build processor!");
            Assert.NotNull(generalSettings, "Build Settings Configuration Object does not exist when it in the current project.");
        }
    }


    [TestFixture(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX)]
    [TestFixture(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64)]
    [TestFixture(BuildTargetGroup.Android, BuildTarget.Android)]
    [TestFixture(BuildTargetGroup.iOS, BuildTarget.iOS)]
#if (!UNITY_2021_2_OR_NEWER)
    [TestFixture(BuildTargetGroup.Lumin, BuildTarget.Lumin)]
#endif
    [TestFixture(BuildTargetGroup.PS4, BuildTarget.PS4)]
    class XRLoaderSelection
    {
        private readonly BuildTargetGroup m_BuildTargetGroup;
        private readonly BuildTarget m_BuildTarget;

        private DummyLoader m_Loader;
        private string m_ExpectedConfigEntry;

        private XRGeneralSettingsPerBuildTarget m_OldBuildTargetSettings;

        public XRLoaderSelection(BuildTargetGroup group, BuildTarget buildTarget)
        {
            m_BuildTargetGroup = group;
            m_BuildTarget = buildTarget;

            m_Loader = ScriptableObject.CreateInstance(typeof(DummyLoader)) as DummyLoader;
            string dummyLoaderLibName = m_Loader.GetPreInitLibraryName(m_BuildTarget, m_BuildTargetGroup);
            m_ExpectedConfigEntry = XRGeneralBuildProcessor.kPreInitLibraryKey + ":" + dummyLoaderLibName;
        }

        void CleanupOldSettings() => BuildHelpers.CleanOldSettings<XRGeneralSettings>();

        [SetUp]
        public void SetupPlayerSettings()
        {
            BuildTestHelpers.SetupTemporaryTestAssets(m_BuildTargetGroup, out m_OldBuildTargetSettings);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_OldBuildTargetSettings != null)
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, m_OldBuildTargetSettings, true);

            BuildTestHelpers.CleanupTemporaryAssets();
        }

        [Test]
        public void EnsureLoaderIsIncludedInConfigWhenSelected()
        {
            XRGeneralBuildProcessor processor = new XRGeneralBuildProcessor();

            // Set up with DummyLoader
            XRManagerSettings settings = GetXRManagerSettings();
            Assert.True(settings.TryAddLoader(m_Loader));
            processor.OnPreprocessBuildImpl(new GUID(""), m_BuildTargetGroup, m_BuildTarget);

            // Determine that the DummyLoader was committed to Editor settings
            string xrBootSettings = GetXRBootSettings();
            Assert.False(String.IsNullOrEmpty(xrBootSettings));
            string dummyLoaderLibName = m_Loader.GetPreInitLibraryName(m_BuildTarget, m_BuildTargetGroup);
            Assert.True(xrBootSettings.Contains(m_ExpectedConfigEntry));
        }

        [Test]
        public void EnsureXRPreInitKeyIsRemovedWhenNoLoaderSelected()
        {
            XRGeneralBuildProcessor processor = new XRGeneralBuildProcessor();

            // Set up with DummyLoader
            XRManagerSettings settings = GetXRManagerSettings();
            Assert.True(settings.TryAddLoader(m_Loader));
            processor.OnPreprocessBuildImpl(new GUID(""), m_BuildTargetGroup, m_BuildTarget);

            // Determine that the DummyLoader was committed to Editor settings
            string xrBootSettings = GetXRBootSettings();
            Assert.False(String.IsNullOrEmpty(xrBootSettings));
            Assert.True(xrBootSettings.Contains(m_ExpectedConfigEntry));

            // Now try to remove it and make sure it's removed from the config
            Assert.True(settings.TryRemoveLoader(m_Loader));
            processor.OnPreprocessBuildImpl(new GUID(""), m_BuildTargetGroup, m_BuildTarget);

            // Determine that the DummyLoader was removed from the Editor settings
            xrBootSettings = GetXRBootSettings();
            Assert.False(xrBootSettings.Contains(m_ExpectedConfigEntry));
        }

        private string GetXRBootSettings()
        {
            string buildTargetName = BuildPipeline.GetBuildTargetName(m_BuildTarget);
            return UnityEditor
                .EditorUserBuildSettings
                    .GetPlatformSettings(buildTargetName, BootConfig.kXrBootSettingsKey);
        }

        private XRManagerSettings GetXRManagerSettings()
        {
            EditorBuildSettings
                .TryGetConfigObject(
                    XRGeneralSettings.k_SettingsKey,
                    out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            return buildTargetSettings
                    .SettingsForBuildTarget(m_BuildTargetGroup)
                        .AssignedSettings;
        }
    }
}
#endif //UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
