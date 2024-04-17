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
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of New Hand interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Hand Interaction Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Add hand interaction profile for hand tracking input device.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/handinteractionprofile.html",
        OpenxrExtensionStrings = extensionString,
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class HandInteractionProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.handinteraction";

        /// <summary>
        /// A new interaction profile for hand tracking input device to provide actions through the OpenXR action system.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Hand Interaction (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class HandInteraction : XRController
        {
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteraction.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteraction.aim"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
            public PoseControl pointer { get; private set; }
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteraction.poke"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, usage = "Poke")]
            public PoseControl pokePose { get; private set; }
            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="HandInteraction.pinch"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, usage = "Pinch")]
            public PoseControl pinchPose { get; private set; }
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="HandInteraction.pinchValue"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "PinchValue")]
            public AxisControl pinchValue { get; private set; }
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="HandInteraction.pinchReady"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "PinchReady")]
            public ButtonControl pinchReady { get; private set; }
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="HandInteraction.pointerActivateValue"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "PointerActivateValue")]
            public AxisControl pointerActivateValue { get; private set; }
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="HandInteraction.pointerActivateReady"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "PointerActivateReady")]
            public ButtonControl pointerActivateReady { get; private set; }
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="HandInteraction.graspValue"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "GraspValue")]
            public AxisControl graspValue { get; private set; }
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="HandInteraction.graspReady"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "GraspReady")]
            public ButtonControl graspReady { get; private set; }
            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping gripPose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 2)]
            new public ButtonControl isTracked { get; private set; }
            /// <summary>
            /// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping gripPose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 4)]
            new public IntegerControl trackingState { get; private set; }
            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position. This value is equivalent to mapping gripPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 8, noisy = true, alias = "gripPosition")]
            new public Vector3Control devicePosition { get; private set; }
            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. This value is equivalent to mapping gripPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 20, noisy = true, alias = "gripRotation")]
            new public QuaternionControl deviceRotation { get; private set; }
            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the aim position. This value is equivalent to mapping aimPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 68, noisy = true)]
            public Vector3Control pointerPosition { get; private set; }
            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the aim orientation. This value is equivalent to mapping aimPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 80, noisy = true)]
            public QuaternionControl pointerRotation { get; private set; }
            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the poke position. This value is equivalent to mapping pokePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 128, noisy = true)]
            public Vector3Control pokePosition { get; private set; }
            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the poke orientation. This value is equivalent to mapping pokePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 140, noisy = true)]
            public QuaternionControl pokeRotation { get; private set; }
            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the pinch position. This value is equivalent to mapping pinchPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 188, noisy = true)]
            public Vector3Control pinchPosition { get; private set; }
            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the pinch orientation. This value is equivalent to mapping pinchPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 200, noisy = true)]
            public QuaternionControl pinchRotation { get; private set; }

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
                pinchValue = GetChildControl<AxisControl>("pinchValue");
                pinchReady = GetChildControl<ButtonControl>("pinchReady");
                pointerActivateValue = GetChildControl<AxisControl>("pointerActivateValue");
                pointerActivateReady = GetChildControl<ButtonControl>("pointerActivateReady");
                graspValue = GetChildControl<AxisControl>("graspValue");
                graspReady = GetChildControl<ButtonControl>("graspReady");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
                pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
                pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
                pokePosition = GetChildControl<Vector3Control>("pokePosition");
                pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
                pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
                pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");
            }
        }

        /// <summary>
        /// The interaction profile string used to reference Hand Interaction Profile.
        /// </summary>
        public const string profile = "/interaction_profiles/ext/hand_interaction_ext";

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
        /// <summary>
        /// Constant for a float interaction binding '.../input/pinch_ext/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string pinchValue = "/input/pinch_ext/value";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/pinch_ext/ready_ext' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string pinchReady = "/input/pinch_ext/ready_ext";
        /// <summary>
        /// Constant for a float interaction binding '.../input/aim_activate_ext/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string pointerActivateValue = "/input/aim_activate_ext/value";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/aim_activate_ext/ready_ext' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string pointerActivateReady = "/input/aim_activate_ext/ready_ext";
        /// <summary>
        /// Constant for a float interaction binding '.../input/grasp_ext/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string graspValue = "/input/grasp_ext/value";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/grasp_ext/ready_ext' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string graspReady = "/input/grasp_ext/ready_ext";

        private const string kDeviceLocalizedName = "Hand Interaction OpenXR";

        /// <summary>
        /// The OpenXR Extension string. This is used by OpenXR to check if this extension is available or enabled.
        /// /// </summary>
        public const string extensionString = "XR_EXT_hand_interaction";

        /// <inheritdoc/>
        protected internal override bool OnInstanceCreate(ulong instance)
        {
            // Requires hand tracking extension
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="HandInteraction"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(HandInteraction),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="HandInteraction"/> layout with the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(HandInteraction));
        }

        /// <summary>
        /// Return device layout string that used for registering device in InputSystem.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return nameof(HandInteraction);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "handinteraction",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left),
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right),
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Grip Pose",
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
                        }
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
                        }
                    },
                    //Poke Pose
                    new ActionConfig()
                    {
                        name = "PokePose",
                        localizedName = "Poke Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Poke"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = poke,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Pinch Pose
                    new ActionConfig()
                    {
                        name = "PinchPose",
                        localizedName = "Pinch Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pinch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pinch,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Pinch Value
                    new ActionConfig()
                    {
                        name = "PinchValue",
                        localizedName = "Pinch Value",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "PinchValue"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pinchValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Pinch Ready
                    new ActionConfig()
                    {
                        name = "PinchReady",
                        localizedName = "Pinch Ready",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PinchReady"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pinchReady,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Pointer Activate Value
                    new ActionConfig()
                    {
                        name = "PointerActivateValue",
                        localizedName = "Pointer Activate Value",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "PointerActivateValue"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pointerActivateValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Pointer Activate Ready
                    new ActionConfig()
                    {
                        name = "PointerActivateReady",
                        localizedName = "Pointer Activate Ready",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PointerActivateReady"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pointerActivateReady,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grasp Value
                    new ActionConfig()
                    {
                        name = "GraspValue",
                        localizedName = "Grasp Value",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "GraspValue"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = graspValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Grasp Ready
                    new ActionConfig()
                    {
                        name = "GraspReady",
                        localizedName = "Grasp Ready",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "GraspReady"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = graspReady,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
    }
}
