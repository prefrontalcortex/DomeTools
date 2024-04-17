#pragma once

#include "Extensions/mock_meta_performance_metrics.h"

// XR_EXT_conformance_automation

struct ConformanceAutomation;
class MockInputState;

void ConformanceAutomation_Create();
void ConformanceAutomation_Destroy();
XrResult ConformanceAutomation_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function);
XrResult ConformanceAutomation_GetInputState(MockInputState* state);
bool ConformanceAutomation_IsActive(XrPath interactionProfile, XrPath userPath, bool defaultValue = true);

// XR_KHR_VULKAN_ENABLE2

#if defined(XR_USE_GRAPHICS_API_VULKAN)
XrResult MockVulkan_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function);
#endif

// XR_KHR_D3D11_ENABLE

#if defined(XR_USE_GRAPHICS_API_D3D11)
XrResult MockD3D11_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function);
#endif
