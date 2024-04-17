using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;

#if UNITY_EDITOR
using System.IO;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR.Features.Interactions;
#endif

[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Features.OculusQuestSupport")]
[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Features.MetaQuestSupport.Editor")]
namespace UnityEngine.XR.OpenXR.Features.MetaQuestSupport
{
    /// <summary>
    /// Enables the Meta mobile OpenXR Loader for Android, and modifies the AndroidManifest to be compatible with Quest.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Meta Quest Support",
        Desc = "Necessary to deploy a Meta Quest compatible app.",
        Company = "Unity",
        DocumentationLink = "https://developer.oculus.com/downloads/package/oculus-openxr-mobile-sdk/",
        OpenxrExtensionStrings = "XR_OCULUS_android_initialize_loader",
        Version = "1.0.0",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        CustomRuntimeLoaderBuildTargets = new[] { BuildTarget.Android },
        FeatureId = featureId
    )]
#endif
    public class MetaQuestFeature : OpenXRFeature
    {
        [Serializable]
        internal struct TargetDevice
        {
            public string visibleName;
            public string manifestName;
            public bool enabled;
            [NonSerialized] public bool active;
        }
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.metaquest";

        /// <summary>
        /// The name of the ambient occlusion render feature script.
        /// Used for validation regarding ambient occlusion on meta quest devices.
        /// </summary>
        private const string ambientOcclusionScriptName = "ScreenSpaceAmbientOcclusion";

#if UNITY_EDITOR
        /// <summary>
        /// Adds devices to the supported devices list in the Android manifest.
        /// </summary>
        [SerializeField]
        internal List<TargetDevice> targetDevices;

        /// <summary>
        /// Forces the removal of Internet permissions added to the Android Manifest.
        /// </summary>
        [SerializeField, Tooltip("Forces the removal of Internet permissions added to the Android Manifest.")]
        internal bool forceRemoveInternetPermission = false;

        [SerializeField]
        internal bool symmetricProjection;

        /// <summary>
        /// Uses a PNG in the Assets folder as the system splash screen image. If set, the OS will display the system splash screen as a high quality compositor layer as soon as the app is starting to launch until the app submits the first frame.
        /// </summary>
        [SerializeField, Tooltip("Uses a PNG in the Assets folder as the system splash screen image. If set, the OS will display the system splash screen as a high quality compositor layer as soon as the app is starting to launch until the app submits the first frame.")]
        public Texture2D systemSplashScreen;

        /// <summary>
        /// Caches validation rules for each build target group requested by <see cref="GetValidationChecks="/>.
        /// </summary>
        private Dictionary<BuildTargetGroup, ValidationRule[]> validationRules = new Dictionary<BuildTargetGroup, ValidationRule[]>();

        /// Holding the Late Latching mode here for the editor (so we get undo/redo functionality)
        /// </summary>
        [SerializeField]
        internal bool lateLatchingMode;

        /// <summary>
        /// Holding the Late Latching mode here for the editor (so we get undo/redo functionality)
        /// </summary>
        [SerializeField]
        internal bool lateLatchingDebug;

        /// <summary>
        /// Forces the removal of Internet permissions added to the Android Manifest.
        /// </summary>
        public bool ForceRemoveInternetPermission
        {
            get => forceRemoveInternetPermission;
            set => forceRemoveInternetPermission = value;
        }

        public new void OnEnable()
        {
            // add known devices
            AddTargetDevice("quest", "Quest", true);
            AddTargetDevice("quest2", "Quest 2", true);
            AddTargetDevice("cambria", "Quest Pro", true);

            symmetricProjection = false;
        }

        /// <summary>
        /// Adds additional target devices to the devices list in the MetaQuestFeatureEditor. Added target devices will
        /// be serialized into the settings asset and will persist across editor sessions, but will only be visible to users
        /// and the manifest if they've been added in the active editor session.
        /// </summary>
        /// <param name="manifestName">Target device name that will be added to AndroidManifest</param>
        /// <param name="visibleName">Device name that will be displayed in feature configuration UI</param>
        /// <param name="enabledByDefault">Target device should be enabled by default or not</param>
        public void AddTargetDevice(string manifestName, string visibleName, bool enabledByDefault)
        {
            if (targetDevices == null)
                targetDevices = new List<TargetDevice>();

            // don't add devices that already exist, but do mark them active for this session
            for (int i = 0; i < targetDevices.Count; ++i)
            {
                var dev = targetDevices[i];

                if (dev.manifestName == manifestName)
                {
                    dev.active = true;
                    targetDevices[i] = dev;
                    return;
                }
            }

            TargetDevice targetDevice = new TargetDevice { manifestName = manifestName, visibleName = visibleName, enabled = enabledByDefault, active = true };
            targetDevices.Add(targetDevice);
        }

        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            if (!validationRules.ContainsKey(targetGroup))
                validationRules.Add(targetGroup, CreateValidationRules(targetGroup));

            rules.AddRange(validationRules[targetGroup]);
        }

        private ValidationRule[] CreateValidationRules(BuildTargetGroup targetGroup) =>

            new ValidationRule[]
            {
                    new ValidationRule(this)
                    {
                        message = "Only the Oculus Touch Interaction Profile and Meta Quest Pro Touch Interaction Profile are supported right now.",
                        checkPredicate = () =>
                        {
                            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                            if (null == settings)
                                return false;

                            bool touchFeatureEnabled = false;
                            bool otherInteractionFeatureEnabled = false;
                            foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                            {
                                if (feature.enabled)
                                {
                                    if ((feature is OculusTouchControllerProfile) || (feature is MetaQuestTouchProControllerProfile))
                                        touchFeatureEnabled = true;
                                    else
                                        otherInteractionFeatureEnabled = true;
                                }
                            }
                            return touchFeatureEnabled && !otherInteractionFeatureEnabled;
                        },
                        error = true,
                        fixIt = () => { SettingsService.OpenProjectSettings("Project/XR Plug-in Management/OpenXR"); },
                        fixItAutomatic = false,
                        fixItMessage = "Open Project Settings to select Oculus Touch or Meta Quest Pro Touch interaction profiles or select both."
                    },

                    new ValidationRule(this)
                    {
                        message = "No Quest target devices selected.",
                        checkPredicate = () =>
                        {
                            foreach (var device in targetDevices)
                            {
                                if (device.enabled)
                                    return true;
                            }

                            return false;
                        },
                        fixIt = () =>
                        {
                            var window = MetaQuestFeatureEditorWindow.Create(this);
                            window.ShowPopup();
                        },
                        error = true,
                        fixItAutomatic = false,
                    },

                    new ValidationRule(this)
                    {
                        message = "Using the Screen Space Ambient Occlusion render feature results in significant performance overhead when the application is running natively on device. Disabling or removing that render feature is recommended.",
                        helpText = "Only removing the Screen Space Ambient Occlusion render feature from all UniversalRenderer assets that may be used will make this warning go away, but just disabling the render feature will still prevent the performance overhead.",
                        checkPredicate = () =>
                        {

                            // Checks the dependencies of all configured render pipeline assets.
                            foreach(var renderPipeline in GraphicsSettings.allConfiguredRenderPipelines)
                            {
                                var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(renderPipeline));
                                foreach(var dependency in dependencies)
                                {
                                    if (dependency.Contains(ambientOcclusionScriptName))
                                        return false;
                                }
                            }

                            return true;
                        },
                        fixItAutomatic = false,
                    },

                    new ValidationRule(this)
                    {
                        message = "System Splash Screen must be a PNG texture asset.",
                        checkPredicate = () =>
                        {
                            if (systemSplashScreen == null)
                                return true;

                            string splashScreenAssetPath = AssetDatabase.GetAssetPath(systemSplashScreen);
                            if (Path.GetExtension(splashScreenAssetPath).ToLower() != ".png")
                                return false;

                            return true;
                        },
                        fixIt = () =>
                        {
                            var window = MetaQuestFeatureEditorWindow.Create(this);
                            window.ShowPopup();
                        },
                        error = true,
                        fixItAutomatic = false,
                    },

#if UNITY_ANDROID
                    new ValidationRule(this)
                    {
                        message = "Symmetric Projection is only supported on Vulkan graphics API",
                        checkPredicate = () =>
                        {
                            if (symmetricProjection)
                            {
                                if (!PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
                                {
                                    GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                                    if (apis.Length >= 1 && apis[0] == GraphicsDeviceType.Vulkan)
                                    {
                                        return true;
                                    }
                                    return false;
                                }
                            }
                            return true;
                        },
                        fixIt = () =>
                        {
                            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
                        },
                        error = true,
                        fixItAutomatic = true,
                        fixItMessage = "Set Vulkan as Graphics API"
                    },

                    new ValidationRule(this)
                    {
                        message = "Symmetric Projection is not supported on Quest 1",
                        checkPredicate = () =>
                        {
                            if (symmetricProjection)
                            {
                                foreach (var device in targetDevices)
                                {
                                    if (device.enabled && device.manifestName == "quest")
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        },
                        fixIt = () =>
                        {
                            var window = MetaQuestFeatureEditorWindow.Create(this);
                            window.ShowPopup();
                        },
                        error = true,
                        fixItAutomatic = false,
                    }
#endif
            };

        internal class MetaQuestFeatureEditorWindow : EditorWindow
        {
            private Object feature;
            private Editor featureEditor;

            public static EditorWindow Create(Object feature)
            {
                var window = EditorWindow.GetWindow<MetaQuestFeatureEditorWindow>(true, "Meta Quest Feature Configuration", true);
                window.feature = feature;
                window.featureEditor = Editor.CreateEditor(feature);
                return window;
            }

            private void OnGUI()
            {
                featureEditor.OnInspectorGUI();
            }
        }
#endif
    }
}
