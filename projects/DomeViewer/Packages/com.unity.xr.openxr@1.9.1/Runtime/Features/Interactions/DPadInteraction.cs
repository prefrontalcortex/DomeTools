using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of DPad feature in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "D-Pad Binding",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Add DPad feature support and if enabled, extra dpad paths will be added to any controller profiles with a thumbstick or trackpad.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/dpadinteraction.html",
        OpenxrExtensionStrings = "XR_KHR_binding_modification XR_EXT_dpad_binding",
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class DPadInteraction : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.dpadinteraction";

        /// <summary>
        /// A flag to mark this DPad feature is additive.
        /// </summary>
        internal override bool IsAdditive => true;

        /// <summary>
        ///  a number in the half-open range (0, 1] representing the force value threshold at or above which ≥ a dpad input will transition from inactive to active.
        /// </summary>
        public float forceThresholdLeft = 0.5f;
        /// <summary>
        /// a number in the half-open range (0, 1] representing the force value threshold strictly below which less than a dpad input will transition from active to inactive.
        /// </summary>
        public float forceThresholdReleaseLeft = 0.4f;
        /// <summary>
        /// the radius in the input value space, of a logically circular region in the center of the input, in the range (0, 1).
        /// </summary>
        public float centerRegionLeft = 0.5f;
        /// <summary>
        /// indicates the angle in radians of each direction region and is a value in the half-open range (0, π].
        /// </summary>
        public float wedgeAngleLeft = (float)(0.5f * Math.PI);
        /// <summary>
        /// indicates that the implementation will latch the first region that is activated and continue to indicate that the binding for that region is true until the user releases the input underlying the virtual dpad.
        /// </summary>
        public bool isStickyLeft = false;

        /// <summary>
        ///  a number in the half-open range (0, 1] representing the force value threshold at or above which ≥ a dpad input will transition from inactive to active.
        /// </summary>
        public float forceThresholdRight = 0.5f;
        /// <summary>
        /// a number in the half-open range (0, 1] representing the force value threshold strictly below which less than a dpad input will transition from active to inactive.
        /// </summary>
        public float forceThresholdReleaseRight = 0.4f;
        /// <summary>
        /// the radius in the input value space, of a logically circular region in the center of the input, in the range (0, 1).
        /// </summary>
        public float centerRegionRight = 0.5f;
        /// <summary>
        /// indicates the angle in radians of each direction region and is a value in the half-open range [0, π].
        /// </summary>
        public float wedgeAngleRight = (float)(0.5f * Math.PI);
        /// <summary>
        /// indicates that the implementation will latch the first region that is activated and continue to indicate that the binding for that region is true until the user releases the input underlying the virtual dpad.
        /// </summary>
        public bool isStickyRight = false;

#if UNITY_EDITOR
        internal class DpadControlEditorWindow : EditorWindow
        {
            private Object feature;
            private Editor featureEditor;

            public static EditorWindow Create(Object feature)
            {
                var window = EditorWindow.GetWindow<DpadControlEditorWindow>(true, "Dpad Interaction Binding Setting", true);
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

        /// <summary>
        /// A  dpad-like interaction feature that allows the application to bind one or more digital actions to a trackpad or thumbstick as though it were a dpad. <a href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XrInteractionProfileDpadBindingEXT"></a>
        /// </summary>
        [Preserve, InputControlLayout(displayName = "D-Pad Binding (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class DPad : XRController
        {
            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.thumbstickDpadUp"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl thumbstickDpadUp { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.thumbstickDpadDown"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl thumbstickDpadDown { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.thumbstickDpadLeft"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl thumbstickDpadLeft { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.thumbstickDpadRight"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl thumbstickDpadRight { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.trackpadDpadUp"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl trackpadDpadUp { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.trackpadDpadDown"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl trackpadDpadDown { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteractionP.trackpadDpadLeft"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl trackpadDpadLeft { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.trackpadDpadRight"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl trackpadDpadRight { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="DPadInteraction.trackpadDpadCenter"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl()]
            public ButtonControl trackpadDpadCenter { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                thumbstickDpadUp = GetChildControl<ButtonControl>("thumbstickDpadUp");
                thumbstickDpadDown = GetChildControl<ButtonControl>("thumbstickDpadDown");
                thumbstickDpadLeft = GetChildControl<ButtonControl>("thumbstickDpadLeft");
                thumbstickDpadRight = GetChildControl<ButtonControl>("thumbstickDpadRight");
                trackpadDpadUp = GetChildControl<ButtonControl>("trackpadDpadUp");
                trackpadDpadDown = GetChildControl<ButtonControl>("trackpadDpadDown");
                trackpadDpadLeft = GetChildControl<ButtonControl>("trackpadDpadLeft");
                trackpadDpadRight = GetChildControl<ButtonControl>("trackpadDpadRight");
                trackpadDpadCenter = GetChildControl<ButtonControl>("trackpadDpadCenter");
            }
        }

        /// <summary>
        /// Constant for a boolean interaction binding '.../thumbstick/dpad_up' OpenXR Input Binding.
        /// </summary>
        public const string thumbstickDpadUp = "/input/thumbstick/dpad_up";
        /// <summary>
        /// Constant for a boolean interaction binding '.../thumbstick/dpad_down' OpenXR Input Binding.
        /// </summary>
        public const string thumbstickDpadDown = "/input/thumbstick/dpad_down";
        /// <summary>
        /// Constant for a boolean interaction binding '.../thumbstick/dpad_left' OpenXR Input Binding.
        /// </summary>
        public const string thumbstickDpadLeft = "/input/thumbstick/dpad_left";
        /// <summary>
        /// Constant for a boolean interaction binding '.../thumbstick/dpad_right' OpenXR Input Binding.
        /// </summary>
        public const string thumbstickDpadRight = "/input/thumbstick/dpad_right";
        /// <summary>
        /// Constant for a boolean interaction binding '.../trackpad/dpad_up' OpenXR Input Binding.
        /// </summary>
        public const string trackpadDpadUp = "/input/trackpad/dpad_up";
        /// <summary>
        /// Constant for a boolean interaction binding '.../trackpad/dpad_down' OpenXR Input Binding.
        /// </summary>
        public const string trackpadDpadDown = "/input/trackpad/dpad_down";
        /// <summary>
        /// Constant for a boolean interaction binding '.../trackpad/dpad_left' OpenXR Input Binding.
        /// </summary>
        public const string trackpadDpadLeft = "/input/trackpad/dpad_left";
        /// <summary>
        /// Constant for a boolean interaction binding '.../trackpad/dpad_right' OpenXR Input Binding.
        /// </summary>
        public const string trackpadDpadRight = "/input/trackpad/dpad_right";
        /// <summary>
        /// Constant for a boolean interaction binding '.../trackpad/dpad_center' OpenXR Input Binding.
        /// </summary>
        public const string trackpadDpadCenter = "/input/trackpad/dpad_center";

        /// <summary>
        /// A unique string for dpad feature
        /// </summary>
        public const string profile = "/interaction_profiles/unity/dpad";

        private const string kDeviceLocalizedName = "DPad Interaction OpenXR";

        /// <summary>
        /// The OpenXR Extension strings. This is used by OpenXR to check if this extension is available or enabled.
        /// /// </summary>
        public string[] extensionStrings = { "XR_KHR_binding_modification", "XR_EXT_dpad_binding" };

#if UNITY_EDITOR
        protected internal override void GetValidationChecks(List<OpenXRFeature.ValidationRule> results, BuildTargetGroup target)
        {
            results.Add( new ValidationRule(this){
                message = "Additive Interaction feature requires a valid controller profile with thumbstick or trackpad selected within Interaction Profiles.",
                error = true,
                errorEnteringPlaymode = true,
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(target);
                    if (null == settings)
                        return false;

                    bool dpadFeatureEnabled = false;
                    bool otherNonAdditiveInteractionFeatureEnabled = false;
                    foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                    {
                        if (feature.enabled)
                        {
                            if (feature is DPadInteraction)
                                dpadFeatureEnabled = true;
                            else if (!(feature as OpenXRInteractionFeature).IsAdditive && !(feature is EyeGazeInteraction))
                                otherNonAdditiveInteractionFeatureEnabled = true;
                        }
                    }
                    return dpadFeatureEnabled && otherNonAdditiveInteractionFeatureEnabled;
                },
                fixIt = () => SettingsService.OpenProjectSettings("Project/XR Plug-in Management/OpenXR"),
                fixItAutomatic = false,
                fixItMessage = "Open Project Settings to select one or more non Additive interaction profiles."
            });
        }
#endif

        /// <inheritdoc/>
        protected internal override bool OnInstanceCreate(ulong instance)
        {
            // Requires dpad related exts
            foreach (var ext in extensionStrings)
            {
                if (!OpenXRRuntime.IsExtensionEnabled(ext))
                    return false;
            }
            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="DPad"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(DPad),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="DPad"/> layout with the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(DPad));
        }

        /// <summary>
        /// Return device layout string for registering Dpad in InputSystem.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return nameof(DPad);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "dpadinteraction",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left),
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right),
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    new ActionConfig()
                    {
                        name = "thumbstickDpadUp",
                        localizedName = " Thumbstick Dpad Up",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickDpadUp,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "thumbstickDpadDown",
                        localizedName = "Thumbstick Dpad Down",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickDpadDown,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "thumbstickDpadLeft",
                        localizedName = "Thumbstick Dpad Left",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickDpadLeft,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "thumbstickDpadRight",
                        localizedName = "Thumbstick Dpad Right",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickDpadRight,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "trackpadDpadUp",
                        localizedName = "Trackpad Dpad Up",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadDpadUp,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "trackpadDpadDown",
                        localizedName = "Trackpad Dpad Down",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadDpadDown,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "trackpadDpadLeft",
                        localizedName = "Trackpad Dpad Left",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadDpadLeft,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "trackpadDpadRight",
                        localizedName = "Trackpad Dpad Right",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadDpadRight,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "trackpadDpadCenter",
                        localizedName = "Trackpad Dpad Center",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadDpadCenter,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    }
                }
            };

            AddActionMap(actionMap);
        }

        //Process additive actions: add additional supported additive actions to the existing controller profiles
        internal override void AddAdditiveActions(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
        {
            foreach (var actionMap in actionMaps)
            {
                //valid userPath is user/hand/left or user/hand/right
                var validUserPath = actionMap.deviceInfos.Where(d => d.userPath != null && ((String.CompareOrdinal(d.userPath, OpenXRInteractionFeature.UserPaths.leftHand) == 0) ||
                    (String.CompareOrdinal(d.userPath, OpenXRInteractionFeature.UserPaths.rightHand) == 0)));
                if (!validUserPath.Any())
                    return;
                //check if interaction profile has thumbstick and/or trackpad
                bool hasTrackPad = false;
                bool hasThumbstick = false;
                foreach (var action in actionMap.actions)
                {
                    if (!hasTrackPad)
                    {
                        var withTrackpad = action.bindings.FirstOrDefault(b => b.interactionPath.Contains("trackpad"));
                        if (withTrackpad != null)
                            hasTrackPad = true;
                    }
                    if (!hasThumbstick)
                    {
                        var withThumbstick = action.bindings.FirstOrDefault(b => b.interactionPath.Contains("thumbstick"));
                        if (withThumbstick != null)
                            hasThumbstick = true;
                    }
                }

                foreach (var additiveAction in additiveMap.actions.Where(a => a.isAdditive))
                {
                    if ((hasTrackPad && additiveAction.name.StartsWith("trackpad")) || (hasThumbstick && additiveAction.name.StartsWith("thumbstick")))
                        actionMap.actions.Add(additiveAction);
                }
            }
        }
    }
}
