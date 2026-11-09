# Changelog
All notable changes to this package will be documented in this file.\
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.5.0] - 2026-11-09
- Update to Unity 6000.3 LTS, update XR packages and fix XR controller assets so VR controllers work again
- Update NDI to 2.1.4 (pfc build) with audio support and Android receiving fixes
- Add audio listener switching between VR and desktop, and set speaker distance based on dome size
- Fix missing hands on Windows standalone by removing hand tracking modules there
- Update manifest and project settings to meet Meta Store requirements
- Add domemaster test image with directions
- Add cursor keys to movement (#35)
- Fix artifacts on the seat texture (#36)
- Fix loading jpg / png images on Quest (#48)
- Remove unused Needle Engine exporter package and an unused XR Controls prefab

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