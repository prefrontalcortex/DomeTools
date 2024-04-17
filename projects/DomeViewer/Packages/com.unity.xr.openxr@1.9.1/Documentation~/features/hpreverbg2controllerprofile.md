---
uid: openxr-hp-reverb-g2-controller-profile
---
# HP Reverb G2 Controller Profile

Enables the OpenXR interaction profile for the HP Reverb G2 Controller and exposes the `<ReverbG2Controller>` device layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).  

For more information about the HP Reverb G2 interaction profile, see the [OpenXR Specification](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_hp_mixed_reality_controller).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/x/click`| primaryButton (Left Hand Only) | Boolean | 
|`/input/y/click`| secondaryButton (Left Hand Only) | Boolean | 
|`/input/a/click`| primaryButton (Right Hand Only) | Boolean | 
|`/input/b/click`| secondaryButton (Right Hand Only) | Boolean | 
|`/input/menu/click` | menu | Boolean|
|`/input/squeeze/value`| grip | Float |
|`/input/squeeze/value`| gripPressed | Boolean (float cast to boolean) |
|`/input/trigger/value`|trigger|  Float |
|`/input/trigger/value`| triggerPressed | Boolean (float cast to boolean) |
|`/input/thumbstick`| thumbstick | Vector2 |
|`/input/thumbstick/click`| thumbstickClicked | Boolean |
|`/input/grip/pose` | devicePose | Pose |
|`/input/aim/pose` | pointer | Pose |
|`/output/haptic` | haptic | Vibrate |
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |
