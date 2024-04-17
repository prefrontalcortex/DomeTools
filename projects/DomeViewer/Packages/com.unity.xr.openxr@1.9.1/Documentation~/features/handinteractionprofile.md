---
uid: openxr-hand-interaction-profile
---
# Hand Interaction Profile

The hand interaction profile is designed for runtimes which provide hand inputs using hand tracking devices instead of controllers with triggers or buttons.
This allows hand tracking devices to provide commonly used gestures and action poses. Enables this OpenXR interaction profile will expose the `<HandInteraction>` device layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).  

OpenXR Specification about Hand Interaction Profile will be updated here when it is available.

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/grip/pose` | devicePose | Pose |
|`/input/aim/pose` | pointer | Pose |
|`/input/pinch_ext/pose` | pinchPose | Pose |
|`/input/poke_ext/pose` | pokePose | Pose |
|`/input/pinch_ext/value`| pinchValue | Float |
|`/input/pinch_ext/ready_ext` | pinchReady | Boolean|
|`/input/aim_activate_ext/value`| pointerActivateValue | Float |
|`/input/aim_activate_ext/ready_ext` | pointerActivateReady | Boolean|
|`/input/grasp_ext/value`| graspValue | Float |
|`/input/grasp_ext/ready_ext` | graspReady | Boolean|
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |