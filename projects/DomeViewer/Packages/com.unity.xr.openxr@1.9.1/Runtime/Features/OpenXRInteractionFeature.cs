using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
#endif

namespace UnityEngine.XR.OpenXR.Features
{
    /// <summary>
    /// A Unity OpenXR Interaction feature.
    /// This class can be inherited from to add a custom action mapping for OpenXR.
    /// </summary>
    [Serializable]
    public abstract class OpenXRInteractionFeature : OpenXRFeature
    {
        /// <summary>
        /// Temporary static list used for action map creation
        /// </summary>
        private static List<ActionMapConfig> m_CreatedActionMaps = null;
        private static Dictionary<InteractionProfileType, Dictionary<string, bool>> m_InteractionProfileEnabledMaps = new Dictionary<InteractionProfileType, Dictionary<string, bool>>();

        /// <summary>
        /// Flag that indicates this feature or profile is additive and its binding paths will be added to other non-additive profiles if enabled.
        /// </summary>
        internal virtual bool IsAdditive => false;

        /// <summary>
        /// The underlying type of an OpenXR action. This enumeration contains all supported control types within OpenXR. This is used when declaring actions in OpenXR with XrAction/>.
        /// </summary>
        [Serializable]
        protected internal enum ActionType
        {
            /// <summary>A binary (on/off) action type. Represented by ButtonControl in the Input System or Boolean in XR.InputDevice.</summary>
            Binary,
            /// <summary>A single Axis float action type. Represented by an AxisControl in the InputSystem or a float in XR.InputDevice.</summary>
            Axis1D,
            /// <summary>A two-dimensional float action type. Represented by a Vector2Control in the InputSystem or Vector2 in XR.InputDevice.</summary>
            Axis2D,
            /// <summary>A position and rotation in three-dimensional space. Represented by a PoseControl in the InputSystem, and a series of controls (boolean to represent if it's being tracked or not, unsigned integer for which fields are available, Vector3 for position, Quaternion for rotation) in XR.InputDevice.</summary>
            Pose,
            /// <summary>This control represents an output motor. Usable as sequential channels (first declared is channel 0, second is 1, etc...) in both the Input System and XR.InputDevice haptic APIs.</summary>
            Vibrate,
            /// <summary>A value representing the total number of ActionTypes available. This can be used to check if an ActionType value is a valid ActionType.</summary>
            Count
        }

        /// <summary>
        /// Information sent to OpenXR about specific, physical control on an input device. Used to identify what an action is bound to (that is, which physical control will trigger that action).
        /// </summary>
        [Serializable]
        protected internal class ActionBinding
        {
            /// <summary>OpenXR interaction profile name</summary>
            public string interactionProfileName;

            /// <summary>OpenXR path for the interaction</summary>
            public string interactionPath;

            /// <summary>Optional OpenXR user paths <seealso cref="UserPaths"/></summary>
            public List<string> userPaths;
        }

        /// <summary>
        /// Declares an abstract input source bound to multiple physical input controls. XrActions are commonly contained within an ActionMapConfig as a grouped series of abstract, remappable inputs.
        /// </summary>
        [Serializable]
        protected internal class ActionConfig
        {
            /// <summary>The name of the action, reported into the InputSystem as the name of the control that represents the input data for this action. This name can only contain a-z lower case letters.</summary>
            public string name;

            /// <summary>The type of data this action will report. <seealso cref="ActionType"/></summary>
            public ActionType type;

            /// <summary>Human readable name for the action</summary>
            public string localizedName;

            /// <summary>The underlying physical input controls to use as the value for this action</summary>
            public List<ActionBinding> bindings;

            /// <summary>These will be tagged onto <see cref="UnityEngine.XR.InputDevice"/> features. See <seealso cref="UnityEngine.XR.InputDevice.TryGetFeatureValue"/></summary>
            public List<string> usages;

            /// <summary>Tag to determine if certain action is additive and could be added to the existing profiles</summary>
            public bool isAdditive;
        }

        /// <summary>
        /// Information sent to the OpenXR runtime to identify how to map a <see cref="UnityEngine.XR.InputDevice"/> or <see cref="UnityEngine.InputSystem.InputDevice"/> to an underlying OpenXR user path.
        /// </summary>
        protected internal class DeviceConfig
        {
            /// <summary>The <see cref="InputDeviceCharacteristics"/> for the <see cref="UnityEngine.XR.InputDevice"/> that will represent this ActionMapConfig. See <seealso cref="UnityEngine.XR.InputDevice.characteristics"/></summary>
            public InputDeviceCharacteristics characteristics;

            /// <summary>OpenXR user path that this device maps to. <seealso cref="UserPaths"/></summary>
            public string userPath;
        }

        /// <summary>
        /// Defines a mapping of between the Unity input system and OpenXR.
        /// </summary>
        [Serializable]
        protected internal class ActionMapConfig
        {
            /// <summary>
            /// Name of the action map
            /// </summary>
            public string name;

            /// <summary>
            /// Human readable name of the OpenXR user path that this device maps to.
            /// </summary>
            public string localizedName;

            /// <summary>
            /// List of devices to configure
            /// </summary>
            public List<DeviceConfig> deviceInfos;

            /// <summary>
            /// List of actions to configure
            /// </summary>
            public List<ActionConfig> actions;

            /// <summary>
            /// OpenXR interaction profile to use for this action map
            /// </summary>
            public string desiredInteractionProfile;

            /// <summary>
            /// Name of the manufacturer providing this action map
            /// </summary>
            public string manufacturer;

            /// <summary>
            /// Serial number of the device
            /// </summary>
            public string serialNumber;
        }

        /// <summary>
        /// Common OpenXR user path definitions.
        /// See the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#semantic-path-user) for more information.
        /// </summary>
        public static class UserPaths
        {
            /// <summary>
            /// Path for user left hand
            /// </summary>
            public const string leftHand = "/user/hand/left";

            /// <summary>
            /// Path for user right hand
            /// </summary>
            public const string rightHand = "/user/hand/right";

            /// <summary>
            /// Path for user head
            /// </summary>
            public const string head = "/user/head";

            /// <summary>
            /// Path for user gamepad
            /// </summary>
            public const string gamepad = "/user/gamepad";

            /// <summary>
            /// Path for user treadmill
            /// </summary>
            public const string treadmill = "/user/treadmill";
        }

        /// <summary>
        /// Flags used to indicate Interaction profile type
        /// </summary>
        public enum InteractionProfileType
        {
            /// <summary>
            /// Interaction profile derived from InputDevice class
            /// </summary>
            Device,
            /// <summary>
            /// Interaction profile derived from XRController class
            /// </summary>
            XRController
        }

        /// <summary>
        /// Register a device layout with the Unity Input System.
        /// Called whenever this interaction profile is enabled in the Editor.
        /// </summary>
        protected virtual void RegisterDeviceLayout()
        {
        }

        /// <summary>
        /// Remove a device layout from the Unity Input System.
        /// Called whenever this interaction profile is disabled in the Editor.
        /// </summary>
        protected virtual void UnregisterDeviceLayout()
        {
        }

        /// <summary>
        /// Register action maps for this device with the OpenXR Runtime.
        /// Called at runtime before Start.
        /// </summary>
        protected virtual void RegisterActionMapsWithRuntime()
        {
        }

        /// <inheritdoc/>
        protected internal override bool OnInstanceCreate(ulong xrSession)
        {
            RegisterDeviceLayout();
            return true;
        }

        /// <summary>
        /// Return interaction profile type. Default type is XRController. Override this if interactionProfile is not derived from XRController class.
        /// </summary>
        /// <returns>Interaction profile type.</returns>
        protected virtual InteractionProfileType GetInteractionProfileType() => InteractionProfileType.XRController;

        /// <summary>
        /// Return device layout name string used for register layouts in inputSystem.
        /// </summary>
        /// <returns>Device layout string.</returns>
        protected virtual string GetDeviceLayoutName() => "";

        /// <summary>
        /// Request the feature create its action maps
        /// </summary>
        /// <param name="configs">Target list for the action maps</param>
        internal void CreateActionMaps(List<ActionMapConfig> configs)
        {
            m_CreatedActionMaps = configs;
            RegisterActionMapsWithRuntime();
            m_CreatedActionMaps = null;
        }

        /// <summary>
        /// Add an action map to the Unity Input System.
        ///
        /// This method must be called from within the RegisterActionMapsWithRuntime method.
        /// </summary>
        /// <param name="map">Action map to add</param>
        protected void AddActionMap(ActionMapConfig map)
        {
            if (null == map)
                throw new ArgumentNullException("map");

            if (null == m_CreatedActionMaps)
                throw new InvalidOperationException("ActionMap must be added from within the RegisterActionMapsWithRuntime method");

            m_CreatedActionMaps.Add(map);
        }

        internal virtual void AddAdditiveActions(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
        {
        }

        /// <summary>
        /// Handle enabled state change
        /// </summary>
        protected internal override void OnEnabledChange()
        {
            base.OnEnabledChange();
#if UNITY_EDITOR && INPUT_SYSTEM_BINDING_VALIDATOR
            var packageSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (null == packageSettings)
                return;

            foreach (var feature in packageSettings.GetFeatures<OpenXRInteractionFeature>())
            {
                var profileType = ((OpenXRInteractionFeature) feature).GetInteractionProfileType();
                string deviceLayoutName = ((OpenXRInteractionFeature) feature).GetDeviceLayoutName();
                deviceLayoutName = "<" + deviceLayoutName + ">";
                if (m_InteractionProfileEnabledMaps.ContainsKey(profileType) && m_InteractionProfileEnabledMaps[profileType].ContainsKey(deviceLayoutName))
                    m_InteractionProfileEnabledMaps[profileType][deviceLayoutName] = feature.enabled;
            }
#endif
        }

        internal static void RegisterLayouts()
        {
#if UNITY_EDITOR
            var packageSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (null == packageSettings)
                return;
#if INPUT_SYSTEM_BINDING_VALIDATOR
            m_InteractionProfileEnabledMaps.Clear();
            foreach (var feature in packageSettings.GetFeatures<OpenXRInteractionFeature>())
            {
                //Register all the available profiles
                ((OpenXRInteractionFeature) feature).RegisterDeviceLayout();

                var profileType = ((OpenXRInteractionFeature) feature).GetInteractionProfileType();
                string deviceLayoutName = ((OpenXRInteractionFeature) feature).GetDeviceLayoutName();
                if (String.IsNullOrEmpty(deviceLayoutName))
                {
                    Debug.LogWarningFormat("No GetDeviceLayoutName() override detected in {0}. Binding path validator for this interaction profile is not as effective. To fix, add GetDeviceLayoutName and GetInteractionProfileType override in this profile.", feature.nameUi);
                    continue;
                }
                deviceLayoutName = "<" + deviceLayoutName + ">";
                if (!m_InteractionProfileEnabledMaps.ContainsKey(profileType))
                    m_InteractionProfileEnabledMaps[profileType] = new Dictionary<string, bool>();

                m_InteractionProfileEnabledMaps[profileType].Add(deviceLayoutName, feature.enabled);
            }
            InputSystem.InputSystem.customBindingPathValidators -= PathValidator;
            InputSystem.InputSystem.customBindingPathValidators += PathValidator;
#else //#if INPUT_SYSTEM_BINDING_VALIDATOR
            foreach (var feature in packageSettings.GetFeatures<OpenXRInteractionFeature>())
            {
                //Register all the available profiles
                ((OpenXRInteractionFeature)feature).RegisterDeviceLayout();
            }
#endif //#if INPUT_SYSTEM_BINDING_VALIDATOR
#else
            foreach (var feature in OpenXRSettings.Instance.GetFeatures<OpenXRInteractionFeature>())
                if (feature.enabled)
                    ((OpenXRInteractionFeature)feature).RegisterDeviceLayout();
#endif //#if UNITY_EDITOR
        }

#if UNITY_EDITOR && INPUT_SYSTEM_BINDING_VALIDATOR
        internal static Action PathValidator(string bindingPath)
        {
            //case1: OpenXR plugin not enabled in XR management
            if (!OpenXRLoaderEnabledForSelectedBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup))
                return null;

            string warningText = null;
            //case2: current bindingPath maps to XRController.
            if (bindingPath.StartsWith("<XRController>"))
            {
                if (!m_InteractionProfileEnabledMaps.ContainsKey(InteractionProfileType.XRController))
                    return null;
                bool controllerProfileEnabled = false;
                foreach (var profile in m_InteractionProfileEnabledMaps[InteractionProfileType.XRController])
                {
                    if (profile.Value)
                        controllerProfileEnabled = true;
                }
                if (controllerProfileEnabled)
                    return null;
                warningText = "This binding will be inactive because there are no enabled OpenXR interaction profiles.";
            }
            else
            {
                //case3: current bindingPath maps to specific OpenXR interaction profile
                //Only check for bindings that belongs to OpenXRInteractionFeature
                bool checkXRInteractionBinding = false;
                bool profileEnabled = false;
                foreach (var map in m_InteractionProfileEnabledMaps)
                {
                    foreach (var profile in map.Value)
                    {
                        if (bindingPath.StartsWith(profile.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            checkXRInteractionBinding = true;
                            profileEnabled = profile.Value;
                            break;
                        }
                    }
                    if (checkXRInteractionBinding)
                        break;
                }
                if (!checkXRInteractionBinding || profileEnabled)
                    return null;

                warningText = "This binding will be inactive because it refers to a disabled OpenXR interaction profile.";
            }
            // Draw the warning information in the Binding Properties panel
            return () =>
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(EditorGUIUtility.IconContent("Warning@2x"), new GUIStyle(EditorStyles.label));
                GUILayout.Label(warningText, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();

                if(GUILayout.Button("Manage Interaction Profiles"))
                     SettingsService.OpenProjectSettings("Project/XR Plug-in Management/OpenXR");
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            };
        }
#endif

#if UNITY_EDITOR
            internal static bool OpenXRLoaderEnabledForSelectedBuildTarget(BuildTargetGroup targetGroup)
            {
                var settings =XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup)?.AssignedSettings;
                if (!settings)
                    return false;
                bool loaderFound = false;
                foreach (var activeLoader in settings.activeLoaders)
                {
                    if (activeLoader as OpenXRLoader != null)
                    {
                        loaderFound = true;
                        break;
                    }
                }
                return loaderFound;
            }
#endif
    }
}
