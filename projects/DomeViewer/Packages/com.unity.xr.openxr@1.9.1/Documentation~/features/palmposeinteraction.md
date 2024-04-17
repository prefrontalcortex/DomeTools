---
uid: openxr-palm-pose-interaction
---
# Palm Pose Interaction

Unity OpenXR provides support for the Palm Pose extension specified by Khronos. Use this layout to retrieve the pose data that the extension returns. This extension also adds a new input component path using this "palm_ext" pose identifier to existing interaction profiles when active.

Enables the OpenXR interaction feature for Palm Pose Interaction and exposes the `<PalmPose>` layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).

For more information about the Palm Pose extension, see the [OpenXR Specification](hhttps://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_palm_pose).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
| `/input/palm_ext/pose` | pose | Pose |
