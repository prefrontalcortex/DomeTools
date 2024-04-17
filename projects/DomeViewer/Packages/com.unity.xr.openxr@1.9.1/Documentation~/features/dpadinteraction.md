---
uid: openxr-dpad-interaction
---
# D-Pad Interaction

Unity OpenXR provides support for the Dpad Binding extension specified by Khronos. Use this layout to retrieve the bindings data that the extension returns. This extension allows the application to bind one or more digital actions to a trackpad or thumbstick as though it were a dpad by defining additional component paths to suggest bindings for.

Enables the OpenXR interaction profile for Dpad Interaction and exposes the `<DPad>` layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).

For more information about the Dpad Binding extension, see the [OpenXR Specification](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_dpad_binding).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
| `/input/thumbstick/dpad_up` | thumbstickDpadUp | Boolean |
| `/input/thumbstick/dpad_down` | thumbstickDpadDown | Boolean |
| `/input/thumbstick/dpad_left` | thumbstickDpadLeft | Boolean |
| `/input/thumbstick/dpad_right` | thumbstickDpadRight | Boolean |
| `/input/trackpad/dpad_up` | trackpadDpadUp | Boolean |
| `/input/trackpad/dpad_down` | trackpadDpadDown | Boolean |
| `/input/trackpad/dpad_left` | trackpadDpadLeft | Boolean |
| `/input/trackpad/dpad_right` | trackpadDpadRight | Boolean |
| `/input/trackpad/dpad_center` | trackpadDpadCenter | Boolean |


