# Changelog

All notable changes to UltimateXR will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.8.4] - 2022-08-05

### Added

- Add new ULTIMATEXR_UNITY_TMPRO symbol when TextMeshPro is available.
- Add support to UxrTextContentTween for TextMeshPro text components.

### Changed

- UxrTextContentTween.Animate() now uses a GameObject as target parameter so that
  either a Unity UI Text component or a TextMeshPro text component can be animated.

### Fixed

- Fix UxrInterpolator.InterpolateText() use of rich text color tag.
- Fix UxrAvatar to avoid infinite loops when enumerating the avatar prefab chain.
- Fix UxrAvatarRigInfo.GetWorldElbowAxis() for left side when T-pose is found.
- Fix UxrAvatarRig.ClearRigElements() to clear missing references.
- Fix missing ULTIMATEXR_UNITY_URP in UxrMagnifyingGlassUrp to avoid URP hard requirement.

## [0.8.3] - 2022-08-01

### Added

- Add editor tooltips to UxrTeleportLocomotionBase and UxrTeleportLocomotion.

### Fixed

- Fix UxrLaserPointerRaycaster bug that prevented using laser pointers as UI input.
- Fix LaserDot.shader so that it works in stereo VR.

## [0.8.2] - 2022-07-21

### Added

- Add support for rotational constraints on more than one axis.
- Add access to the grabbable object on UxrApplyConstraintsEventArgs.

### Fixed

- Remove built-in compatibility in hand shader to fix shader errors when building.
  Compatibility will be added again as soon as Unity issue is fixed.
  Issue status can be followed here: https://github.com/VRMADA/ultimatexr-unity/issues/2
- Fix grabbable objects being manipulated on movable platforms.
- Fix UxrManipulationHapticFeedback component sending feedback incorrectly on movable platforms.

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

[Unreleased]: https://github.com/VRMADA/ultimatexr-unity/compare/v0.8.4...HEAD
[0.8.4]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.4
[0.8.3]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.3
[0.8.2]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.2
[0.8.1]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.1
[0.8.0]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.0