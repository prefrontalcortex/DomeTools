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

#if USE_INPUT_SYSTEM_POSE_CONTROL
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of hand common poses profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Hand Interaction Poses",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Add hand common interaction poses feature, if enabled, four additional commonly used poses will be supported.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/handcommonposesinteraction.html",
        OpenxrExtensionStrings = extensionString,
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class HandCommonPosesInteraction : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.handinteractionposes";

        /// <summary>
        /// A flag to mark this hand interaction feature is potentially additive.
        /// </summary>
        internal override bool IsAdditive => true;

        /// <summary>
        ///  An interaction feature that supports commonly used hand poses for hand interactions across motion controller and hand tracking devices.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Hand Interaction Poses (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true)]
        public class HandInteractionPoses : OpenXRDevice
        {
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteractionPoses.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteractionPoses.aim"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
            public PoseControl pointer { get; private set; }
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteractionPoses.poke"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0)]
            public PoseControl pokePose { get; private set; }
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteractionPoses.pinch"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0)]
            public PoseControl pinchPose { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                devicePose = GetChildControl<PoseControl>("devicePose");
                pointer = GetChildControl<PoseControl>("pointer");
                pokePose = GetChildControl<PoseControl>("pokePose");
                pinchPose = GetChildControl<PoseControl>("pinchPose");
            }
        }

        /// <summary>
        /// The interaction profile string used to reference Hand Common Poses feature.
        /// </summary>
        public const string profile = "/interaction_profiles/unity/hand_interaction_poses";

        // Available Bindings
        // Both Hands
        /// <summary>
        /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string grip = "/input/grip/pose";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string aim = "/input/aim/pose";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/poke_ext/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string poke = "/input/poke_ext/pose";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/pinch_ext/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string pinch = "/input/pinch_ext/pose";

        private const string kDeviceLocalizedName = "Hand Interaction Poses OpenXR";

        /// <summary>
        /// The OpenXR Extension string. This is used by OpenXR to check if this extension is available or enabled.
        /// /// </summary>
        public const string extensionString = "XR_EXT_hand_interaction";

#if UNITY_EDITOR
        protected internal override void GetValidationChecks(List<OpenXRFeature.ValidationRule> results, BuildTargetGroup target)
        {
            results.Add( new ValidationRule(this){
                message = "Additive Interaction feature requires a valid controller or hand interaction profile selected within Interaction Profiles.",
                error = true,
                errorEnteringPlaymode = true,
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(target);
                    if (null == settings)
                        return false;

                    bool handCommonPosesFeatureEnabled = false;
                    bool otherNonAdditiveInteractionFeatureEnabled = false;
                    foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                    {
                        if (feature.enabled)
                        {
                            if (feature is HandCommonPosesInteraction)
                                handCommonPosesFeatureEnabled = true;
                            else if (!(feature as OpenXRInteractionFeature).IsAdditive && !(feature is EyeGazeInteraction))
                                otherNonAdditiveInteractionFeatureEnabled = true;
                        }
                    }
                    return handCommonPosesFeatureEnabled && otherNonAdditiveInteractionFeatureEnabled;
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
            // Requires hand tracking extension
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="HandInteractionPoses"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(HandInteractionPoses),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="HandInteractionPoses"/> layout with the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(HandInteractionPoses));
        }

        /// <summary>
        /// Return Interaction profile type. Hand common poses profile is Device type.
        /// </summary>
        /// <returns>Interaction profile type.</returns>
        protected override InteractionProfileType GetInteractionProfileType()
        {
            return typeof(HandInteractionPoses).IsSubclassOf(typeof(XRController)) ? InteractionProfileType.XRController : InteractionProfileType.Device;
        }

        /// <summary>
        /// Return device layout string that used to register device in InputSystem.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return nameof(HandInteractionPoses);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "handinteractionposes",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left),
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right),
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = grip,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    // Pointer Pose
                    new ActionConfig()
                    {
                        name = "pointer",
                        localizedName = "Pointer Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pointer"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = aim,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    //Poke Pose
                    new ActionConfig()
                    {
                        name = "PokePose",
                        localizedName = "Poke Pose",
                        type = ActionType.Pose,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = poke,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    //Pinch Pose
                    new ActionConfig()
                    {
                        name = "PinchPose",
                        localizedName = "Pinch Pose",
                        type = ActionType.Pose,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pinch,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    }
                }
            };
            AddActionMap(actionMap);
        }

        //Process additive actions: add additional supported additive actions to existing controller or hand interaction profiles
        internal override void AddAdditiveActions(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
        {
            foreach (var actionMap in actionMaps)
            {
                //valid userPath is user/hand/left or user/hand/right
                var validUserPath = actionMap.deviceInfos.Where(d => d.userPath != null && ((String.CompareOrdinal(d.userPath, OpenXRInteractionFeature.UserPaths.leftHand) == 0) ||
                    (String.CompareOrdinal(d.userPath, OpenXRInteractionFeature.UserPaths.rightHand) == 0)));

                if (validUserPath.Any())
                {
                    foreach (var additiveAction in additiveMap.actions.Where(a => a.isAdditive))
                    {
                        bool duplicateFound = false;
                        var poseActions = actionMap.actions.Where(m => m.type == ActionType.Pose).Distinct().ToList();
                        foreach (var poseAction in poseActions)
                        {
                            if ((poseAction.bindings.Where(b => b.interactionPath != null && (String.CompareOrdinal(b.interactionPath, additiveAction.bindings[0].interactionPath) == 0))).Any())
                            {
                                poseAction.isAdditive = true;
                                duplicateFound = true;
                            }
                        }
                        if (!duplicateFound)
                            actionMap.actions.Add(additiveAction);
                    }
                }
            }
        }
    }
}
