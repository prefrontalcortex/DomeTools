#include "mock_meta_performance_metrics.h"

#include "../mock.h"
#include <sstream>

#define CHECK_EXT()                                        \
    if (nullptr == MockMetaPerformanceMetrics::Instance()) \
        return XR_ERROR_FUNCTION_UNSUPPORTED;

// storage for static
std::unique_ptr<MockMetaPerformanceMetrics> MockMetaPerformanceMetrics::s_ext;

enum class MockMetaPerformanceMetrics::InternalPaths : int
{
    Invalid = 0,
    AppCPUFrametime,
    AppGPUFrametime,
    AppMotionToPhotonLatency,
    CompositorCPUFrametime,
    CompositorGPUFrametime,
    CompositorDroppedFrameCount,
    CompositorSpacewarpMode,
    DeviceCPUUtilizationAvg,
    DeviceCPUUtilizationWorst,
    DeviceGPUUtilization,
};

namespace
{
constexpr char kAppCPUFrametimeStr[] = "/perfmetrics_meta/app/cpu_frametime";
constexpr char kAppGPUFrametimeStr[] = "/perfmetrics_meta/app/gpu_frametime";
constexpr char kAppMotionToPhotonLatencyStr[] = "/perfmetrics_meta/app/motion_to_photon_latency";
constexpr char kCompositorCPUFrametimeStr[] = "/perfmetrics_meta/compositor/cpu_frametime";
constexpr char kCompositorGPUFrametimeStr[] = "/perfmetrics_meta/compositor/gpu_frametime";
constexpr char kCompositorDroppedFrameCountStr[] = "/perfmetrics_meta/compositor/dropped_frame_count";
constexpr char kCompositorSpacewarpModeStr[] = "/perfmetrics_meta/compositor/spacewarp_mode";
constexpr char kDeviceCPUUtilizationAvgStr[] = "/perfmetrics_meta/device/cpu_utilization_average";
constexpr char kDeviceCPUUtilizationWorstStr[] = "/perfmetrics_meta/device/cpu_utilization_worst";
constexpr char kDeviceGPUUtilizationStr[] = "/perfmetrics_meta/device/gpu_utilization";
} // namespace

MockMetaPerformanceMetrics::InternalPaths MockMetaPerformanceMetrics::InternalPathFromString(
    const std::string& s)
{
    if (s == kAppCPUFrametimeStr)
        return InternalPaths::AppCPUFrametime;
    if (s == kAppGPUFrametimeStr)
        return InternalPaths::AppGPUFrametime;
    if (s == kAppMotionToPhotonLatencyStr)
        return InternalPaths::AppMotionToPhotonLatency;
    if (s == kCompositorCPUFrametimeStr)
        return InternalPaths::CompositorCPUFrametime;
    if (s == kCompositorGPUFrametimeStr)
        return InternalPaths::CompositorGPUFrametime;
    if (s == kCompositorDroppedFrameCountStr)
        return InternalPaths::CompositorDroppedFrameCount;
    if (s == kCompositorSpacewarpModeStr)
        return InternalPaths::CompositorSpacewarpMode;
    if (s == kDeviceCPUUtilizationAvgStr)
        return InternalPaths::DeviceCPUUtilizationAvg;
    if (s == kDeviceCPUUtilizationWorstStr)
        return InternalPaths::DeviceCPUUtilizationWorst;
    if (s == kDeviceGPUUtilizationStr)
        return InternalPaths::DeviceGPUUtilization;
    return InternalPaths::Invalid;
}

MockMetaPerformanceMetrics::MockMetaPerformanceMetrics(MockRuntime& runtime, const int numMockCPUs)
    : m_NumMockCPUs{numMockCPUs}
    , m_Runtime{runtime}
{
    static constexpr const char* k_ListPaths[] = {
        kAppCPUFrametimeStr,
        kAppGPUFrametimeStr,
        kAppMotionToPhotonLatencyStr,
        kCompositorCPUFrametimeStr,
        kCompositorGPUFrametimeStr,
        kCompositorDroppedFrameCountStr,
        kCompositorSpacewarpModeStr,
        kDeviceCPUUtilizationAvgStr,
        kDeviceCPUUtilizationWorstStr,
        kDeviceGPUUtilizationStr,
    };

    for (const char* const pathString : k_ListPaths)
    {
        const XrPath path = runtime.StringToPath(pathString);
        m_Paths[path] = InternalPathFromString(pathString);
    }

    for (int i = 0; i < m_NumMockCPUs; i++)
    {
        std::stringstream ss{"/perfmetrics_meta/device/cpu"};
        ss << i << "_utilization";

        std::string pathString = ss.str();
        const XrPath path = runtime.StringToPath(pathString.c_str());
        m_Paths[path] = InternalPathFromString(pathString);
    }
}

void MockMetaPerformanceMetrics::Init(MockRuntime& runtime, const int numMockCPUs)
{
    s_ext.reset(new MockMetaPerformanceMetrics(runtime, numMockCPUs));
}

void MockMetaPerformanceMetrics::Deinit()
{
    s_ext.reset();
}

MockMetaPerformanceMetrics* MockMetaPerformanceMetrics::Instance()
{
    return s_ext.get();
}

XrResult MockMetaPerformanceMetrics::EnumeratePaths(
    XrInstance instance,
    uint32_t counterPathCapacityInput,
    uint32_t* counterPathCountOutput,
    XrPath* counterPaths)
{
    *counterPathCountOutput = static_cast<uint32_t>(m_Paths.size());
    if (counterPathCapacityInput >= *counterPathCountOutput)
    {
        int i = 0;
        for (const auto kv : m_Paths)
        {
            counterPaths[i] = kv.first;
            i++;
        }
    }
    return XR_SUCCESS;
}

XrResult MockMetaPerformanceMetrics::SetState(XrSession session, const XrPerformanceMetricsStateMETA* state)
{
    if (!state || state->type != XR_TYPE_PERFORMANCE_METRICS_STATE_META)
    {
        return XR_ERROR_VALIDATION_FAILURE;
    }
    m_Enabled = state->enabled == XR_TRUE;
    return XR_SUCCESS;
}

XrResult MockMetaPerformanceMetrics::GetState(XrSession session, XrPerformanceMetricsStateMETA* state)
{
    if (!state || state->type != XR_TYPE_PERFORMANCE_METRICS_STATE_META)
    {
        return XR_ERROR_VALIDATION_FAILURE;
    }
    state->enabled = m_Enabled ? XR_TRUE : XR_FALSE;
    return XR_SUCCESS;
}

XrResult MockMetaPerformanceMetrics::QueryCounter(
    XrSession session,
    XrPath counterPath,
    XrPerformanceMetricsCounterMETA* counter)
{
    if (!m_Enabled || !counter || counter->type != XR_TYPE_PERFORMANCE_METRICS_COUNTER_META)
    {
        // disabled state return value confirmed by Xiang Wei @ Meta.
        return XR_ERROR_VALIDATION_FAILURE;
    }

    std::list<MockResult>& values = m_SeededResults[m_Paths[counterPath]];
    if (values.empty())
    {
        if (!m_NoResultsSeededWarningShown)
        {
            m_NoResultsSeededWarningShown = true;
            MOCK_TRACE_DEBUG("No results were seeded for the requested stat. If you aren't testing stats, ignore this warning.");
        }
        *counter = m_DefaultResult.value;
        return m_DefaultResult.rv;
    }

    // FIFO, fill in the counter struct and return the seeded return value.
    *counter = values.front().value;
    XrResult rv = values.front().rv;
    values.pop_front();
    return rv;
}

void MockMetaPerformanceMetrics::SeedCounterOnce(const std::string& counterPathString, MockResult result)
{
    const XrPath path = m_Runtime.StringToPath(counterPathString.c_str());
    if (path == XR_NULL_PATH)
    {
        // providing an invalid path during testing shouldn't happen, kill and warn our dev
        // if you want to test whether a path exists, use the runtime functions themselves.
        MOCK_TRACE_ERROR("Could not find path %s", counterPathString.c_str());
        return;
    }

    if (result.value.type != XR_TYPE_PERFORMANCE_METRICS_COUNTER_META)
    {
        MOCK_TRACE_ERROR(
            "Invalid or no type supplied for XrPerformanceMetricsCounterMETA. Type MUST be"
            " XR_TYPE_PERFORMANCE_METRICS_COUNTER_META.");
        return;
    }
    if (result.value.next != nullptr)
    {
        MOCK_TRACE_ERROR(
            "This structure does not currently support chaining (`next` should be `nullptr`).");
        return;
    }
    std::list<MockResult>& values = m_SeededResults[m_Paths[path]];
    values.push_back(result);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR
xrEnumeratePerformanceMetricsCounterPathsMETA(
    XrInstance instance,
    uint32_t counterPathCapacityInput,
    uint32_t* counterPathCountOutput,
    XrPath* counterPaths)
{
    LOG_FUNC();
    CHECK_RUNTIME();
    CHECK_INSTANCE(instance);
    CHECK_EXT();
    MOCK_HOOK_BEFORE();

    const XrResult result =
        MockMetaPerformanceMetrics::Instance()->EnumeratePaths(
            instance,
            counterPathCapacityInput,
            counterPathCountOutput,
            counterPaths);

    MOCK_HOOK_AFTER(result);

    return result;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR
xrSetPerformanceMetricsStateMETA(
    XrSession session,
    const XrPerformanceMetricsStateMETA* state)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    CHECK_EXT();
    MOCK_HOOK_BEFORE();

    const XrResult result = MockMetaPerformanceMetrics::Instance()->SetState(session, state);

    MOCK_HOOK_AFTER(result);

    return result;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR
xrGetPerformanceMetricsStateMETA(
    XrSession session,
    XrPerformanceMetricsStateMETA* state)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    CHECK_EXT();
    MOCK_HOOK_BEFORE();

    const XrResult result = MockMetaPerformanceMetrics::Instance()->GetState(session, state);

    MOCK_HOOK_AFTER(result);

    return result;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR
xrQueryPerformanceMetricsCounterMETA(
    XrSession session,
    XrPath counterPath,
    XrPerformanceMetricsCounterMETA* counter)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    CHECK_EXT();
    MOCK_HOOK_BEFORE();

    const XrResult result =
        MockMetaPerformanceMetrics::Instance()->QueryCounter(
            session,
            counterPath,
            counter);

    MOCK_HOOK_AFTER(result);

    return result;
}

XrResult MockMetaPerformanceMetrics_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function)
{
    GET_PROC_ADDRESS(xrEnumeratePerformanceMetricsCounterPathsMETA)
    GET_PROC_ADDRESS(xrSetPerformanceMetricsStateMETA)
    GET_PROC_ADDRESS(xrGetPerformanceMetricsStateMETA)
    GET_PROC_ADDRESS(xrQueryPerformanceMetricsCounterMETA)
    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

#undef CHECK_EXT
