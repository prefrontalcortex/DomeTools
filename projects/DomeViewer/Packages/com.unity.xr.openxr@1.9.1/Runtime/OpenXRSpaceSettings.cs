using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR
{
    public partial class OpenXRSettings
    {
        /// <summary>
        /// Activates reference space recentering when using floor-based tracking origin.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a recentering event is performed, OpenXR will attempt to recenter the world space origin based on the local-floor reference space, if supported by the platform's hardware.
        /// </para>
        /// <para>
        /// If that reference space isn't supported, OpenXR will then attempt to approximate it using stage space or local space.
        /// </para>
        /// <para>
        /// Calling this method won't trigger a recenter event. This event will be sent from the platform's runtime.
        /// </para>
        /// </remarks>
        /// <param name="allowRecentering">Boolean value that activates the recentering feature.</param>
        /// <param name="floorOffset">Estimated height used when approximating the floor-based position when recentering the space. By default, this value is 1.5f</param>
        public static void SetAllowRecentering(bool allowRecentering, float floorOffset = 1.5f)
        {
            Internal_SetAllowRecentering(allowRecentering, floorOffset);
        }

        /// <summary>
        /// Returns the current state of the recentering feature.
        /// </summary>
        public static bool AllowRecentering
        {
            get
            {
                return Internal_GetAllowRecentering();
            }
        }

        /// <summary>
        /// Returns the current floor offset value used when approximating the floor-based position when recentering the space.
        /// </summary>
        public static float FloorOffset
        {
            get
            {
                return Internal_GetFloorOffset();
            }
        }

        [DllImport("UnityOpenXR", EntryPoint = "NativeConfig_SetAllowRecentering")]
        private static extern void Internal_SetAllowRecentering(bool active, float height);

        [DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetAllowRecentering")]
        private static extern bool Internal_GetAllowRecentering();

        [DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetFloorOffsetHeight")]
        private static extern float Internal_GetFloorOffset();
    }
}
