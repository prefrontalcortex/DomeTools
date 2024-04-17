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
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Palm Pose feature in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Palm Pose",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Add Palm pose feature and if enabled, extra palm pose path /input/palm_ext/pose will be added to regular interaction profile.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/palmposeinteraction.html",
        OpenxrExtensionStrings = extensionString,
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class PalmPoseInteraction : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.palmpose";

        /// <summary>
        /// A flag to mark this Palm Pose feature is additive.
        /// </summary>
        internal override bool IsAdditive => true;

        /// <summary>
        /// Palm Pose interaction feature supports an input patch for the palm pose.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Palm Pose (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class PalmPose : XRController
        {
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="PalmPoseInteraction.palmPose"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0)]
            public PoseControl palmPose { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping palmPose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 0)]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping palmPose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 4)]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position. This value is equivalent to mapping palmPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 8, noisy = true, alias = "palmPosition")]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. This value is equivalent to mapping palmPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 20, noisy = true, alias = "palmRotation")]
            new public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the palm pose position. This value is equivalent to mapping palmPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 8, noisy = true)]
            public Vector3Control palmPosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the palm pose orientation. This value is equivalent to mapping palmPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 20, noisy = true)]
            public QuaternionControl palmRotation { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                palmPose = GetChildControl<PoseControl>("palmPose");
            }
        }

        /// <summary>
        /// Constant for a pose interaction binding '.../palm_ext/pose' OpenXR Input Binding.
        /// </summary>
        public const string palmPose = "/input/palm_ext/pose";

        /// <summary>
        /// A unique string for palm pose feature
        /// </summary>
        public const string profile = "/interaction_profiles/ext/palmpose";

        private const string kDeviceLocalizedName = "Palm Pose Interaction OpenXR";
        /// <summary>
        /// The OpenXR Extension string. This is used by OpenXR to check if this extension is available or enabled.
        /// /// </summary>
        public const string extensionString = "XR_EXT_palm_pose";

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

                    bool palmPoseFeatureEnabled = false;
                    bool otherNonAdditiveInteractionFeatureEnabled = false;
                    foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
                    {
                        if (feature.enabled)
                        {
                            if (feature is PalmPoseInteraction)
                                palmPoseFeatureEnabled = true;
                            else if (!(feature as OpenXRInteractionFeature).IsAdditive && !(feature is EyeGazeInteraction))
                                otherNonAdditiveInteractionFeatureEnabled = true;
                        }
                    }
                    return palmPoseFeatureEnabled && otherNonAdditiveInteractionFeatureEnabled;
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
            // Requires the palmPose extension
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="PalmPose"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(PalmPose),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="PalmPose"/> layout with the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(PalmPose));
        }

        /// <summary>
        /// Return device layout string that used for registering device for the Input System.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return nameof(PalmPose);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "palmposeinteraction",
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
                    new ActionConfig()
                    {
                        name = "palmpose",
                        localizedName = "Palm Pose",
                        type = ActionType.Pose,
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = palmPose,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    }
                }
            };

            AddActionMap(actionMap);
        }

        //Process additive actions: add additional supported additive actions to existing controller profiles
        internal override void AddAdditiveActions(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
        {
            foreach (var actionMap in actionMaps)
            {
                //valid userPath is user/hand/left or user/hand/right
                var validUserPath = actionMap.deviceInfos.Where(d => d.userPath != null && ((String.CompareOrdinal(d.userPath, OpenXRInteractionFeature.UserPaths.leftHand) == 0) ||
                    (String.CompareOrdinal(d.userPath, OpenXRInteractionFeature.UserPaths.rightHand) == 0)));
                if (!validUserPath.Any())
                    return;

                foreach (var additiveAction in additiveMap.actions.Where(a => a.isAdditive))
                {
                    actionMap.actions.Add(additiveAction);
                }
            }
        }
    }
}
