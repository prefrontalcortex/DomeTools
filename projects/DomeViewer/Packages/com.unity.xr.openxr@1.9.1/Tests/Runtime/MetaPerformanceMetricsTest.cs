using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using ProviderXRStats = UnityEngine.XR.Provider.XRStats;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class MetaPerformanceMetricsTests : OpenXRLoaderSetup
    {
        private static readonly string kAppCPUFrametimeStr = "/perfmetrics_meta/app/cpu_frametime";
        private static readonly string kAppGPUFrametimeStr = "/perfmetrics_meta/app/gpu_frametime";
        private static readonly string kAppMotionToPhotonLatencyStr =
            "/perfmetrics_meta/app/motion_to_photon_latency";
        private static readonly string kCompositorCPUFrametimeStr =
            "/perfmetrics_meta/compositor/cpu_frametime";
        private static readonly string kCompositorGPUFrametimeStr =
            "/perfmetrics_meta/compositor/gpu_frametime";
        private static readonly string kCompositorDroppedFrameCountStr =
            "/perfmetrics_meta/compositor/dropped_frame_count";
        private static readonly string kDeviceCPUUtilizationAvgStr =
            "/perfmetrics_meta/device/cpu_utilization_average";
        private static readonly string kDeviceCPUUtilizationWorstStr =
            "/perfmetrics_meta/device/cpu_utilization_worst";
        private static readonly string kDeviceGPUUtilizationStr =
            "/perfmetrics_meta/device/gpu_utilization";

        private readonly List<string> xrPathStrings = new List<string>
        {
            kAppCPUFrametimeStr,
            kAppGPUFrametimeStr,
            kAppGPUFrametimeStr, // because we have 2 consumers
            kAppMotionToPhotonLatencyStr,
            kCompositorCPUFrametimeStr,
            kCompositorGPUFrametimeStr,
            kCompositorGPUFrametimeStr,  // because we have 2 consumers
            kCompositorDroppedFrameCountStr,
            kCompositorDroppedFrameCountStr, // because we have 2 consumers
            kDeviceCPUUtilizationAvgStr,
            kDeviceCPUUtilizationWorstStr,
            kDeviceGPUUtilizationStr
        };

        private static readonly string kUnityStatsAppCpuTime = "perfmetrics.appcputime";
        private static readonly string kUnityStatsAppGpuTime = "perfmetrics.appgputime";
        private static readonly string kUnityStatsAppGpuTimeFunc = "GPUAppLastFrameTime";
        private static readonly string kUnityStatsCompositorCpuTime =
            "perfmetrics.compositorcputime";
        private static readonly string kUnityStatsCompositorGpuTime =
            "perfmetrics.compositorgputime";
        private static readonly string kUnityStatsCompositorGpuTimeFunc =
            "GPUCompositorLastFrameTime";
        private static readonly string kUnityStatsGpuUtil = "perfmetrics.gpuutil";
        private static readonly string kUnityStatsCpuUtilAvg = "perfmetrics.cpuutilavg";
        private static readonly string kUnityStatsCpuUtilWorst = "perfmetrics.cpuutilworst";
        private static readonly string kUnityStatsDroppedFrameCount = "appstats.compositordroppedframes";
        private static readonly string kUnityStatsDroppedFrameCountFunc = "droppedFrameCount";
        private static readonly string kUnityStatsMotionToPhotonFunc = "motionToPhoton";

        private readonly List<string> unityStatStrings = new List<string>
        {
            kUnityStatsAppCpuTime,
            kUnityStatsAppGpuTime,
            kUnityStatsAppGpuTimeFunc,
            kUnityStatsCompositorCpuTime,
            kUnityStatsCompositorGpuTime,
            kUnityStatsCompositorGpuTimeFunc,
            kUnityStatsGpuUtil,
            kUnityStatsCpuUtilAvg,
            kUnityStatsCpuUtilWorst,
            kUnityStatsDroppedFrameCount,
            kUnityStatsDroppedFrameCountFunc,
            kUnityStatsMotionToPhotonFunc
        };

        private readonly Dictionary<string, XrPerformanceMetricsCounterUnitMETA> unitMap =
            new Dictionary<string, XrPerformanceMetricsCounterUnitMETA>()
        {
            {
                kAppCPUFrametimeStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_MILLISECONDS_META
            },
            {
                kAppGPUFrametimeStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_MILLISECONDS_META
            },
            {
                kAppMotionToPhotonLatencyStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_MILLISECONDS_META
            },
            {
                kCompositorCPUFrametimeStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_MILLISECONDS_META
            },
            {
                kCompositorGPUFrametimeStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_MILLISECONDS_META
            },
            {
                kCompositorDroppedFrameCountStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_GENERIC_META
            },
            {
                kDeviceCPUUtilizationAvgStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_PERCENTAGE_META
            },
            {
                kDeviceCPUUtilizationWorstStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_PERCENTAGE_META
            },
            {
                kDeviceGPUUtilizationStr,
                XrPerformanceMetricsCounterUnitMETA.XR_PERFORMANCE_METRICS_COUNTER_UNIT_PERCENTAGE_META
            }
        };

        private enum XrPerformanceMetricsCounterUnitMETA
        {
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_GENERIC_META = 0,
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_PERCENTAGE_META = 1,
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_MILLISECONDS_META = 2,
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_BYTES_META = 3,
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_HERTZ_META = 4,
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_MAX_ENUM_META = 0x7FFFFFFF
        }

        [UnityTest]
        public IEnumerator TestAllMetrics()
        {
            base.InitializeAndStart();
            yield return new WaitForXrFrame(2);

            List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);

            Assert.That(displays.Count, Is.EqualTo(1));

            // insert a -1 dummy value to ensure we're ready when we start querying
            for (int i = -1; i < 5; i++)
            {
                foreach (string xrPathString in xrPathStrings)
                {
                    MockRuntime.MetaPerformanceMetrics_SeedCounterOnce_Float(
                        xrPathString,
                        (float)i,
                        (uint)unitMap[xrPathString]
                    );
                }
            }

            // wait for results to come available.
            bool didSync = false;
            for (int i = 0; i < 60; i++)
            {
                if (ProviderXRStats.TryGetStat(displays[0], kUnityStatsAppCpuTime, out var sync))
                {
                    if (sync < 0)
                    {
                        didSync = true;
                        break;
                    }
                    if (sync > 0)
                        Assert.Fail("did not receive -1.f sync stat");
                }
                yield return new WaitForXrFrame();
            }
            Assert.True(didSync, "did not receive -1.f sync stat");

            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForXrFrame();
                foreach (string unityStatString in unityStatStrings)
                {
                    Assert.True(
                        ProviderXRStats.TryGetStat(displays[0], unityStatString, out var stat),
                        "did not get stat for " + unityStatString
                    );
                    Assert.That(stat, Is.EqualTo((float)i).Within(0.001));
                }
            }
        }
    }
}
