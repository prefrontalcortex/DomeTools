using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.OpenXR.Features;


#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Reset View Feature",
    BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
    Company = "Unknown",
    Desc = "Feature to catch reset view in headset and apply it in the app.",
    //DocumentationLink = "https://docs.unity.cn/Packages/com.unity.xr.openxr@0.1/manual/index.html",
    OpenxrExtensionStrings = "", // this extension doesn't exist, a log message will be printed that it couldn't be enabled
    Version = "0.0.1",
    FeatureId = featureId)]
#endif
 
public class ResetViewFeature : OpenXRFeature
{
    public const string featureId = "com.unkonwn.resetview";
    public static UnityEvent AppSpaceChanged = new UnityEvent();
    protected override void OnSessionBegin(ulong xrSession)
    {
        base.OnSessionBegin(xrSession);
        AppSpaceChanged.Invoke();
        Debug.Log("AppSpaceChange detected in the feature");
    }
}