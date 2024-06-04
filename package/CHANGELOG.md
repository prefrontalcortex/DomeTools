# Changelog
All notable changes to this package will be documented in this file.\
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.7.3] - 2024-06-04
- fix world-space UI not rendering in Cubemap mode (supported in URP and HDRP but not in BiRP)

## [0.7.2] - 2024-05-30
- fix shader compilation error when URP is not installed
- fix two sample scenes not using the new Dome Tools prefab
- fix NDI sender not properly being assigned in the Dome Tools prefab
- add check to disable the "Warn if no cameras rendering" message in the Game View

## [0.7.1] - 2024-05-28
- fix compilation error when the `KlakNDI` package is not present

## [0.7.0] - 2024-05-28
- fix banding due to wrong render texture setting
- fix cube map and dome warp cameras not using Post Processing and platform settings for MSAA/HDR
- fix Readme using outdated prefab names
- change Readme to reflect new features and include more information on how to get started
- change sample project Unity versions to latest LTS
- add Dome Tools root prefab and editor UI
- add Audio example

## [0.6.0] - 2024-03-27
- versioning, naming
- improved inspector for immutable render texture cases

## [0.5.0-pre] - 2024-03-14
- add new sample scenes
- improve inspector for immutable package to allow texture size adjustments
- change shader folder names to be more consistent
- fix some issues with the dome rig inspector
- fix some issues with shader compilation on HDRP

## [0.4.0-pre] - 2023-12-21
- added traditional cubemap rendering option
- added test patterns
- added nice editor UI for dome rig
- added nice editor UI for dome master output texts
- added NDI sender components and soft dependency on `jp.keijiro.klak.ndi`
- simplified samples and added render-pipeline independent sample
- bumped min. version to 2021.3

## [0.2.2-pre] - 2022-04-23
- added dome display shadergraph
- bumped pfc rendering dependency

## [0.2.1-exp] - 2021-07-15
- updated to SRP 10.6 (new custom frame settings to disable post on HDRP cameras)
- cleanup up sample scenes a bit

## [0.1.0-preview] - 2020-05-04
- initial release
- base set of fulldome content creation and viewing tools
- correct optical unwarping from single image to DomeMaster format
- DomeMaster metadata text labels