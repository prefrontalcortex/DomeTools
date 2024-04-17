#if !UNITY_2020_2_OR_NEWER

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using UnityEditor;
using UnityEditor.PackageManager;

namespace UnityEditor.XR.Management.Legacy
{
    [InitializeOnLoad]
    class XRLegacyStandaloneSubsystem
    {
        static XRLegacyStandaloneSubsystem()
        {
            PackageManager.Client.Add("com.unity.subsystemregistration");
        }
    }
}

#endif // !UNITY_2020_2_OR_NEWER
