using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

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
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Meta Quest Pro controller interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Meta Quest Touch Pro Controller Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Allows for mapping input to the Meta Quest Touch Pro Controller interaction profile.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/metaquesttouchprocontrollerprofile.html",
        OpenxrExtensionStrings = "XR_FB_touch_controller_pro",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class MetaQuestTouchProControllerProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.metaquestpro";

        /// <summary>
        /// An Input System device based on the controller interaction profile Meta Touch Controller Pro.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Meta Quest Pro Touch Controller(OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class QuestProTouchController : XRControllerWithRumble
        {
            /// <summary>
            /// A [Vector2Control](xref:UnityEngine.InputSystem.Controls.Vector2Control) that represents the <see cref="MetaQuestTouchProControllerProfile.thumbstick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary2DAxis", "Joystick" }, usage = "Primary2DAxis")]
            public Vector2Control thumbstick { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MetaQuestTouchProControllerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripAxis", "squeeze" }, usage = "Grip")]
            public AxisControl grip { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
            public ButtonControl gripPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.system"/> <see cref="MetaQuestTouchProControllerProfile.menu"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary", "menuButton", "systemButton" }, usage = "MenuButton")]
            public ButtonControl menu { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.buttonA"/> <see cref="MetaQuestTouchProControllerProfile.buttonX"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "A", "X", "buttonA", "buttonX" }, usage = "PrimaryButton")]
            public ButtonControl primaryButton { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.buttonATouch"/> <see cref="MetaQuestTouchProControllerProfile.buttonYTouch"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "ATouched", "XTouched", "ATouch", "XTouch", "buttonATouched", "buttonXTouched" }, usage = "PrimaryTouch")]
            public ButtonControl primaryTouched { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.buttonB"/> <see cref="MetaQuestTouchProControllerProfile.buttonY"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "B", "Y", "buttonB", "buttonY" }, usage = "SecondaryButton")]
            public ButtonControl secondaryButton { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.buttonBTouch"/> <see cref="MetaQuestTouchProControllerProfile.buttonYTouch"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "BTouched", "YTouched", "BTouch", "YTouch", "buttonBTouched", "buttonYTouched" }, usage = "SecondaryTouch")]
            public ButtonControl secondaryTouched { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MetaQuestTouchProControllerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "Trigger")]
            public AxisControl trigger { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "indexButton", "indexTouched", "triggerbutton" }, usage = "TriggerButton")]
            public ButtonControl triggerPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.triggerTouch"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "indexTouch", "indexNearTouched" }, usage = "TriggerTouch")]
            public ButtonControl triggerTouched { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.thumbstickClick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "JoystickOrPadPressed", "thumbstickClick", "joystickClicked" }, usage = "Primary2DAxisClick")]
            public ButtonControl thumbstickClicked { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.thumbstickTouch"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "JoystickOrPadTouched", "thumbstickTouch", "joystickTouched" }, usage = "Primary2DAxisTouch")]
            public ButtonControl thumbstickTouched { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.thumbrest"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "ThumbrestTouch")]
            public ButtonControl thumbrestTouched { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="MetaQuestTouchProControllerProfile.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="MetaQuestTouchProControllerProfile.aim"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, alias = "aimPose", usage = "Pointer")]
            public PoseControl pointer { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 28, usage = "IsTracked")]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 32, usage = "TrackingState")]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position. For the Oculus Touch device, this is both the grip and the pointer position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 36, noisy = true, alias = "gripPosition")]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the Oculus Touch device, this is both the grip and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 48, noisy = true, alias = "gripOrientation")]
            new public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for back compatibility with the XRSDK layouts. This is the pointer position. This value is equivalent to mapping pointerPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 96)]
            public Vector3Control pointerPosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the pointer rotation. This value is equivalent to mapping pointerPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 108, alias = "pointerOrientation")]
            public QuaternionControl pointerRotation { get; private set; }

            /// <summary>
            /// A <see cref="HapticControl"/> that represents the <see cref="MetaQuestTouchProControllerProfile.haptic"/> binding.
            /// </summary>
            [Preserve, InputControl(usage = "Haptic")]
            public HapticControl haptic { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MetaQuestTouchProControllerProfile.thumbrestForce"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "ThumbrestForce")]
            public AxisControl thumbrestForce { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MetaQuestTouchProControllerProfile.stylusForce"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "StylusForce")]
            public AxisControl stylusForce { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MetaQuestTouchProControllerProfile.triggerCurl"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "TriggerCurl")]
            public AxisControl triggerCurl { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MetaQuestTouchProControllerProfile.triggerSlide"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "TriggerSlide")]
            public AxisControl triggerSlide { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.triggerProximity"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "TriggerProximity")]
            public ButtonControl triggerProximity { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MetaQuestTouchProControllerProfile.thumbProximity"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(usage = "ThumbProximity")]
            public ButtonControl thumbProximity { get; private set; }

            /// <summary>
            /// A <see cref="HapticControl"/> that represents the <see cref="MetaQuestTouchProControllerProfile.hapticTrigger"/> binding.
            /// </summary>
            [Preserve, InputControl(usage = "HapticTrigger")]
            public HapticControl hapticTrigger { get; private set; }

            /// <summary>
            /// A <see cref="HapticControl"/> that represents the <see cref="MetaQuestTouchProControllerProfile.hapticThumb"/> binding.
            /// </summary>
            [Preserve, InputControl(usage = "HapticThumb")]
            public HapticControl hapticThumb { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                thumbstick = GetChildControl<Vector2Control>("thumbstick");
                trigger = GetChildControl<AxisControl>("trigger");
                triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
                triggerTouched = GetChildControl<ButtonControl>("triggerTouched");
                grip = GetChildControl<AxisControl>("grip");
                gripPressed = GetChildControl<ButtonControl>("gripPressed");
                menu = GetChildControl<ButtonControl>("menu");
                primaryButton = GetChildControl<ButtonControl>("primaryButton");
                primaryTouched = GetChildControl<ButtonControl>("primaryTouched");
                secondaryButton = GetChildControl<ButtonControl>("secondaryButton");
                secondaryTouched = GetChildControl<ButtonControl>("secondaryTouched");
                thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
                thumbstickTouched = GetChildControl<ButtonControl>("thumbstickTouched");
                thumbrestTouched = GetChildControl<ButtonControl>("thumbrestTouched");

                devicePose = GetChildControl<PoseControl>("devicePose");
                pointer = GetChildControl<PoseControl>("pointer");

                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
                pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
                pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");

                haptic = GetChildControl<HapticControl>("haptic");
                thumbrestForce = GetChildControl<AxisControl>("thumbrestForce");
                stylusForce = GetChildControl<AxisControl>("stylusForce");
                triggerCurl = GetChildControl<AxisControl>("triggerCurl");
                triggerSlide = GetChildControl<AxisControl>("triggerSlide");
                triggerProximity = GetChildControl<ButtonControl>("triggerProximity");
                thumbProximity = GetChildControl<ButtonControl>("thumbProximity");
                hapticTrigger = GetChildControl<HapticControl>("hapticTrigger");
                hapticThumb = GetChildControl<HapticControl>("hapticThumb");
            }
        }

        /// <summary>
        /// The interaction profile string used to reference Meta Quest Pro Touch Controller.
        /// </summary>
        public const string profile = "/interaction_profiles/facebook/touch_controller_pro";

        // Available Bindings
        // Left Hand Only
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/x/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string buttonX = "/input/x/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/x/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string buttonXTouch = "/input/x/touch";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/y/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string buttonY = "/input/y/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/y/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string buttonYTouch = "/input/y/touch";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/menu/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string menu = "/input/menu/click";

        // Right Hand Only
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/a/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string buttonA = "/input/a/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/a/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string buttonATouch = "/input/a/touch";
        /// <summary>
        /// Constant for a boolean interaction binding '..."/input/b/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string buttonB = "/input/b/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/b/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string buttonBTouch = "/input/b/touch";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/system/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string system = "/input/system/click";

        // Both Hands
        /// <summary>
        /// Constant for a float interaction binding '.../input/squeeze/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string squeeze = "/input/squeeze/value";
        /// <summary>
        /// Constant for a float interaction binding '.../input/trigger/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trigger = "/input/trigger/value";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/trigger/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string triggerTouch = "/input/trigger/touch";
        /// <summary>
        /// Constant for a Vector2 interaction binding '...//input/thumbstick' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstick = "/input/thumbstick";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/thumbstick/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstickClick = "/input/thumbstick/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/thumbstick/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstickTouch = "/input/thumbstick/touch";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/thumbrest/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbrest = "/input/thumbrest/touch";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string grip = "/input/grip/pose";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string aim = "/input/aim/pose";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string haptic = "/output/haptic";
        /// <summary>
        /// Constant for a float interaction binding '.../input/thumbrest/force' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbrestForce = "/input/thumbrest/force";
        /// <summary>
        /// Constant for a float interaction binding '.../input/stylus_fb/force' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string stylusForce = "/input/stylus_fb/force";
        /// <summary>
        /// Constant for a float interaction binding '.../input/trigger/curl_fb' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string triggerCurl = "/input/trigger/curl_fb";
        /// <summary>
        /// Constant for a float interaction binding '.../input/trigger/slide_fb' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string triggerSlide = "/input/trigger/slide_fb";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string triggerProximity = "/input/trigger/proximity_fb";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbProximity = "/input/thumb_fb/proximity_fb";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string hapticTrigger = "/output/trigger_haptic_fb";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string hapticThumb = "/output/thumb_haptic_fb";

        private const string kDeviceLocalizedName = "Meta Quest Pro Touch Controller OpenXR";

        protected internal override bool OnInstanceCreate(ulong instance)
        {
            // Requires the pro controller extension
            if (!OpenXRRuntime.IsExtensionEnabled("XR_FB_touch_controller_pro"))
                return false;

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="QuestProTouchController"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(QuestProTouchController),
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="QuestProTouchController"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(QuestProTouchController));
        }

        /// <summary>
        /// Return device layout string that used for registering device for the Input System.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected override string GetDeviceLayoutName()
        {
            return nameof(QuestProTouchController);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "questprotouchcontroller",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "Oculus",
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
                    // Joystick
                    new ActionConfig()
                    {
                        name = "thumbstick",
                        localizedName = "Thumbstick",
                        type = ActionType.Axis2D,
                        usages = new List<string>()
                        {
                            "Primary2DAxis"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstick,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grip
                    new ActionConfig()
                    {
                        name = "grip",
                        localizedName = "Grip",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Grip"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grip Pressed
                    new ActionConfig()
                    {
                        name = "gripPressed",
                        localizedName = "Grip Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "GripButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Menu
                    new ActionConfig()
                    {
                        name = "menu",
                        localizedName = "Menu",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "MenuButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = menu,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = system,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    //A / X Press
                    new ActionConfig()
                    {
                        name = "primaryButton",
                        localizedName = "Primary Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PrimaryButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = buttonX,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = buttonA,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    //A / X Touch
                    new ActionConfig()
                    {
                        name = "primaryTouched",
                        localizedName = "Primary Touched",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PrimaryTouch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = buttonXTouch,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = buttonATouch,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    //B / Y Press
                    new ActionConfig()
                    {
                        name = "secondaryButton",
                        localizedName = "Secondary Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "SecondaryButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = buttonY,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = buttonB,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    //B / Y Touch
                    new ActionConfig()
                    {
                        name = "secondaryTouched",
                        localizedName = "Secondary Touched",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "SecondaryTouch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = buttonYTouch,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = buttonBTouch,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    // Trigger
                    new ActionConfig()
                    {
                        name = "trigger",
                        localizedName = "Trigger",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Trigger"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trigger Pressed
                    new ActionConfig()
                    {
                        name = "triggerPressed",
                        localizedName = "Trigger Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Trigger Touch
                    new ActionConfig()
                    {
                        name = "triggerTouched",
                        localizedName = "Trigger Touched",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerTouch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = triggerTouch,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Thumbstick Clicked
                    new ActionConfig()
                    {
                        name = "thumbstickClicked",
                        localizedName = "Thumbstick Clicked",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "Primary2DAxisClick"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickClick,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Thumbstick Touched
                    new ActionConfig()
                    {
                        name = "thumbstickTouched",
                        localizedName = "Thumbstick Touched",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "Primary2DAxisTouch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickTouch,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Thumbrest Touched
                    new ActionConfig()
                    {
                        name = "thumbrestTouched",
                        localizedName = "Thumbrest Touched",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "ThumbrestTouch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbrest,
                                interactionProfileName = profile,
                            }
                        }
                    },
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
                    // Haptics
                    new ActionConfig()
                    {
                        name = "haptic",
                        localizedName = "Haptic Output",
                        type = ActionType.Vibrate,
                        usages = new List<string>() { "Haptic" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = haptic,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "thumbrestForce",
                        localizedName = "Thumbrest Force",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "ThumbrestForce"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbrestForce,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "stylusForce",
                        localizedName = "Stylus Force",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "StylusForce"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = stylusForce,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "triggerCurl",
                        localizedName = "Trigger Curl",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "TriggerCurl"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = triggerCurl,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "triggerSlide",
                        localizedName = "Trigger Slide",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "TriggerSlide"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = triggerSlide,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Trigger Proximity
                    new ActionConfig()
                    {
                        name = "triggerProximity",
                        localizedName = "Trigger Proximity",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerProximity"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = triggerProximity,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Thumb Proximity
                    new ActionConfig()
                    {
                        name = "thumbProximity",
                        localizedName = "Thumb Proximity",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "ThumbProximity"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbProximity,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    //Haptic Trigger
                    new ActionConfig()
                    {
                        name = "hapticTrigger",
                        localizedName = "Haptic Trigger Output",
                        type = ActionType.Vibrate,
                        usages = new List<string>() { "HapticTrigger" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = hapticTrigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Haptic Thumb
                    new ActionConfig()
                    {
                        name = "hapticThumb",
                        localizedName = "Haptic Thumb Output",
                        type = ActionType.Vibrate,
                        usages = new List<string>() { "HapticThumb" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = hapticThumb,
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
