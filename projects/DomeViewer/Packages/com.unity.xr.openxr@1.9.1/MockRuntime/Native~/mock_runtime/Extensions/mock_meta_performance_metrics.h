// Mock extension for XR_META_performance_metrics extension
// https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_META_performance_metrics

#pragma once

#include "openxr/openxr.h"
#include <list>
#include <map>
#include <memory>
#include <string>
#include <unordered_map>

class MockRuntime;

class MockMetaPerformanceMetrics
{
public:
    enum class InternalPaths : int;

    struct MockResult
    {
        XrResult rv;
        XrPerformanceMetricsCounterMETA value;
    };

    static void Init(MockRuntime& runtime, const int numMockCPUs);
    static void Deinit();
    static MockMetaPerformanceMetrics* Instance();

    static InternalPaths InternalPathFromString(const std::string& s);

    XrResult EnumeratePaths(
        XrInstance instance,
        uint32_t counterPathCapacityInput,
        uint32_t* counterPathCountOutput,
        XrPath* counterPaths);
    XrResult SetState(XrSession session, const XrPerformanceMetricsStateMETA* state);
    XrResult GetState(XrSession session, XrPerformanceMetricsStateMETA* state);
    XrResult QueryCounter(
        XrSession session,
        XrPath counterPath,
        XrPerformanceMetricsCounterMETA* counter);

    void SeedCounterOnce(const std::string& counterPath, MockResult result);

private:
    MockMetaPerformanceMetrics(MockRuntime& runtime, const int numMockCPUs);

    static std::unique_ptr<MockMetaPerformanceMetrics> s_ext;
    MockRuntime& m_Runtime;

    const int m_NumMockCPUs;

    // we use an ordered set just to keep things consistent for enumeration
    std::map<XrPath, InternalPaths> m_Paths;

    bool m_Enabled;
    bool m_NoResultsSeededWarningShown;

    std::map<InternalPaths, std::list<MockResult>> m_SeededResults;
    MockResult m_DefaultResult{
        XR_SUCCESS,
        {
            XR_TYPE_PERFORMANCE_METRICS_COUNTER_META,
            nullptr,
            XR_PERFORMANCE_METRICS_COUNTER_ANY_VALUE_VALID_BIT_META,
            XR_PERFORMANCE_METRICS_COUNTER_UNIT_GENERIC_META,
            0,
            0.f,
        }};
};

XrResult MockMetaPerformanceMetrics_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function);
