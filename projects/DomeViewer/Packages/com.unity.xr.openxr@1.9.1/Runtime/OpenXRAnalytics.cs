using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
using UnityEngine.Analytics;
#endif //UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS

namespace UnityEngine.XR.OpenXR
{
    internal static class OpenXRAnalytics
    {
        private const int kMaxEventsPerHour = 1000;
        private const int kMaxNumberOfElements = 1000;
        private const string kVendorKey = "unity.openxr";
        private const string kEventInitialize = "openxr_initialize";

#if ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
        private static bool s_Initialized = false;
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS


        [Serializable]
        private struct InitializeEvent
#if UNITY_2023_2_OR_NEWER && UNITY_ANALYTICS
            : IAnalytic.IData
#endif //UNITY_2023_2_OR_NEWER && UNITY_ANALYTICS
        {
            public bool success;
            public string runtime;
            public string runtime_version;
            public string plugin_version;
            public string api_version;
            public string[] available_extensions;
            public string[] enabled_extensions;
            public string[] enabled_features;
            public string[] failed_features;
        }

#if UNITY_2023_2_OR_NEWER && UNITY_ANALYTICS
        [AnalyticInfo(eventName: kEventInitialize, vendorKey: kVendorKey, maxEventsPerHour: kMaxEventsPerHour, maxNumberOfElements: kMaxNumberOfElements)]
        private class XrInitializeAnalytic : IAnalytic
        {
            private InitializeEvent? data = null;

            public XrInitializeAnalytic(InitializeEvent data)
            {
                this.data = data;
            }

            public bool TryGatherData(out IAnalytic.IData data, [NotNullWhen(false)] out Exception error)
            {
                error = null;
                data = this.data;
                return data != null;
            }
        }
#endif //UNITY_2023_2_OR_NEWER && UNITY_ANALYTICS

        private static bool Initialize()
        {
#if ENABLE_TEST_SUPPORT || !ENABLE_CLOUD_SERVICES_ANALYTICS || !UNITY_ANALYTICS
            return false;
#elif UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            return EditorAnalytics.enabled;
#else
            if (s_Initialized)
                return true;

#if UNITY_EDITOR
            if (!EditorAnalytics.enabled)
                return false;

            if (AnalyticsResult.Ok != EditorAnalytics.RegisterEventWithLimit(kEventInitialize, kMaxEventsPerHour, kMaxNumberOfElements, kVendorKey))
#else
            if (AnalyticsResult.Ok != Analytics.Analytics.RegisterEvent(kEventInitialize, kMaxEventsPerHour, kMaxNumberOfElements, kVendorKey))
#endif //UNITY_EDITOR
                return false;

            s_Initialized = true;

            return true;
#endif //ENABLE_TEST_SUPPORT || !ENABLE_CLOUD_SERVICES_ANALYTICS || !UNITY_ANALYTICS
        }

        public static void SendInitializeEvent(bool success)
        {
#if UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
            if (!s_Initialized && !Initialize())
                return;

            var data = CreateInitializeEvent(success);

#if UNITY_EDITOR
            SendEditorAnalytics(data);
#else
            SendPlayerAnalytics(data);
#endif //UNITY_EDITOR
#endif //UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
        }

        private static InitializeEvent CreateInitializeEvent(bool success)
        {
            return new InitializeEvent
            {
                success = success,
                runtime = OpenXRRuntime.name,
                runtime_version = OpenXRRuntime.version,
                plugin_version = OpenXRRuntime.pluginVersion,
                api_version = OpenXRRuntime.apiVersion,
                enabled_extensions = OpenXRRuntime.GetEnabledExtensions()
                    .Select(ext => $"{ext}_{OpenXRRuntime.GetExtensionVersion(ext)}")
                    .ToArray(),
                available_extensions = OpenXRRuntime.GetAvailableExtensions()
                    .Select(ext => $"{ext}_{OpenXRRuntime.GetExtensionVersion(ext)}")
                    .ToArray(),
                enabled_features = OpenXRSettings.Instance.features
                    .Where(f => f != null && f.enabled)
                    .Select(f => $"{f.GetType().FullName}_{f.version}").ToArray(),
                failed_features = OpenXRSettings.Instance.features
                    .Where(f => f != null && f.failedInitialization)
                    .Select(f => $"{f.GetType().FullName}_{f.version}").ToArray()
            };
        }


#if UNITY_EDITOR && UNITY_ANALYTICS
        private static void SendEditorAnalytics(InitializeEvent data)
        {
#if UNITY_2023_2_OR_NEWER
            EditorAnalytics.SendAnalytic(new XrInitializeAnalytic(data));
#else
            EditorAnalytics.SendEventWithLimit(kEventInitialize, data);
#endif //UNITY_2023_2_OR_NEWER
       }
#elif UNITY_ANALYTICS
        private static void SendPlayerAnalytics(InitializeEvent data)
        {
            Analytics.Analytics.SendEvent(kEventInitialize, data);
        }
#endif //UNITY_EDITOR
    }
}
