# Changelog
All notable changes to this package will be documented in this file.\
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.5.0] - 2026-11-17
### Added
- Quest: "Open file from this device…" in the Media list opens the system file picker, so videos and images can be played from any folder on the headset (for example `Movies` or `Download`) without extra permissions. Large videos start right away and are not copied
- NDI sources are discovered automatically, the list updates every 3 seconds
- Cursor keys move the desktop camera (#35)

### Changed
- Update to Unity 6000.3 LTS (Unity 6), URP 17, XR Interaction Toolkit 3.3, XR Hands 1.7 and OpenXR 1.16
- Larger field of view steps for the mouse wheel on desktop
- Local media on Quest: only list formats the device can play (mp4, mov, png, jpg)

### Fixed
- Loading jpg / png images on Quest (#48)
- Artifacts on the seat texture (#36)
- A video or image that fails to load resets the Media selection instead of leaving the previous content on screen

### Removed
- Unused Needle Engine exporter package and an unused XR Controls prefab

## [1.4.0] - 2025-04-04
- Update to Unity 2021.3.44 and update packages
- NDI 2.1.4 (pfc build) with audio, and NDI receiving on Android
- Audio follows the active view: the audio listener switches between VR and desktop
- Virtual speaker distance follows the dome size
- NDI receiver rotated to match the dome's forward direction
- Fix missing hands on Windows by removing hand tracking modules there
- Add a Dome Master test image with directions
- Manifest and project settings meet the Meta Store requirements

## [1.3.0] - 2024-09-17
- Switch to the modified KlakNDI package with audio support
- Update NDI

## [1.2.0] - 2024-07-29
- NDI with audio (#9)
- Update the NDI library to 6.0.1
- Teleporting with hand tracking, activated by a hand gesture (#13)
- Keep the user inside the dome: the walkable radius is limited and follows the dome size (#14, #20)
- The VR menu follows the user (tag-along)
- Update to Unity 2021.3.37 and XR Hands 1.5
- No longer require SD card permission (#21)

## [1.1.1] - 2024-04-23
- Reset view: add OpenXR feature for recentering
- Update OpenXR to 1.9.1
- Use a modified XR Management package so that Android builds keep the internet permission (needed for NDI)

## [1.1.0] - 2024-04-09
- Add support for demo assets on Quest builds
- Add dithering when viewing miniature dome from below for clearer visibility
- Fix menu and raycaster issues
- Fix version number and build info displayed in app
- Fix accidental zooming when scrolling on the menu

### Known Issues
- NDI streaming currently doesn't support audio input (#9)
- Performance isn't ideal for 4k inputs on Quest 2 (#12)
- When using hand tracking, teleporting is currently disabled (#13)
- When walking around and then switching to Miniature View, the user is positioned too far away from the model (#14)
- When walking around, teleporting, or switching to a smaller Dome layout, the user can leave the dome (#14)

## [1.0.0] - 2024-04-05
- Initial release of Dome Viewer for Windows and Android/Quest