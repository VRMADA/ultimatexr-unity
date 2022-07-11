# Changelog

All notable changes to UltimateXR will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.8.1] - 2022-07-11

### Changed

- Use ULTIMATEXR_USE_PICOXR_SDK instead of UXR_USE_PICOXR_SDK for consistency.

### Fixed

- Add new ControllerNames device names to UxrHtcViveInput to correctly detect HTC Vive controllers.
- Add new ControllerNames device names to UxrValveIndexInput to correctly detect Knuckles controllers.
- Add new ULTIMATEXR_UNITY_URP symbol to avoid compiler errors when Unity's URP package is not installed.

## [0.8.0] - 2022-07-06

### Added

- First public release!

[Unreleased]: https://github.com/VRMADA/ultimatexr-unity/compare/v0.8.1...HEAD
[0.8.1]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.1
[0.8.0]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.0