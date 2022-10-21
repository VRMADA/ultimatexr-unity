# Changelog

All notable changes to UltimateXR will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.9.3] - 2022-10-21

### Added

- Add support to use UltimateXR through Unity Package Manager using git URL.

### Changed

- Change folder structure to adapt to the Unity Package Manager layout:
  https://docs.unity3d.com/Manual/cus-layout.html

## [0.9.2] - 2022-10-18

### Fixed

- Fix UxrGrabbableObject editor methods that caused compiler errors when creating a build.
- Fix UxrGrabbableObject startup so that component can be added to an object at runtime.

## [0.9.1] - 2022-10-13

### Changed

- Improve some UxrGrabbableObject parameter tooltips.

### Fixed

- Fix GameObjectExt.GetBounds and GetLocalBounds exceptions when no renderers are found.
- Fix GameObjectExt.GetBounds not computing value correctly.

## [0.9.0] - 2022-10-13

### Added

- Add new UxrGrabbableObject constraints functionality with improved manipulation.
- Add UxrGrabbableObject gizmos to visualize rotation/translation constraints.
- Add new UxrGrabbableObject rotation/translation constraint modes.
- Add support to UxrGrabbableObject for rotation constraints on all 3 axes.
- Add support to UxrGrabbableObject for a single rotation constraint over 360 degrees.
- Improve manipulation behavior when grabbing objects to detect the grip and know which part
  of the hand creates more leverage.
- Add possibility to parent to destination in locomotion components: UxrTeleportLocomotion and 
  UxrSmoothLocomotion.
- Add new teleport methods to UxrManager to teleport relative to moving objects.
- Add new functionality to GameObjectExt to compute bounds recursively.
- Add new functionality to MeshExt to compute skinned mesh vertices and bone influences.
- Add new misc functionality to FloatExt, IntExt, Vector3Ext, Vector3IntExt and TransformExt.
- Add new data to UxrAvatarRigInfo.
- Add versioning to avatar rig info serialization and automatic updating.
- Add IUxrLogger interface to unify logging in managers.
- Add logging to UxrWeaponManager.
- Add new properties to UxrComponent with initial Transform data.
- Add new UxrAxis properties and functionality.
- Add possibility to access avatar grabbers at edit-time.

### Changed

- Improve all UxrGrabbableObject and hand grab/release/constrain transitions.
- Move UxrGrabbableObject constraints to the top of the inspector.
- Replace GrabAndMove/RotateAroundAxis manipulation modes by new constraint system.
- Change UxrGrabbableObject rotation and translation constraints reference.
  Rotations are performed around the grabbable object local axes.
  Translations are performed along the initial grabbable object local axes.
- Improve UxrAvatarRig reference solving.

### Removed

- Remove parent reference to UxrGrabbableObject rotation/translation constraints.
- Remove UxrManipulationMode. New constraint system and UxrRotationProvider is used instead.

### Fixed

- Fix manipulation not working correctly on moving platforms.
- Fix incorrect manipulation release on objects with non-default grab button(s).
- Fix UxrGrabbableObject release multipliers not working correctly with values less than 1.
- Fix UxrGrabbableObject Constrain events not being called in some cases.
- Fix UxrAvatarEditor throwing exception when using Fix button to save prefab variant.
- Fix UxrCameraWallFade throwing exception when there are no avatars.
- Fix constrained rotations not being able to go over 180 degrees.
- Fix pre-caching triggered by non-local avatars. Only local avatar triggers pre-caching now.
- Fix locomotion components detecting avatar or grabbed objects as obstacles.
- Fix locomotion not working correctly on moving platforms.
- Fix UxrWeaponManager not tracking actors correctly.
- Fix UxrMagnifyingGlassUrp error when not using URP.
- Fix CyborgAvatar_URP base to use index controllers correctly.
- Fix some CyborgAvatar_BRP base materials that were using the URP variants.

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

[Unreleased]: https://github.com/VRMADA/ultimatexr-unity/compare/v0.9.3...HEAD
[0.9.3]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.3
[0.9.2]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.2
[0.9.1]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.1
[0.9.0]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.0
[0.8.4]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.4
[0.8.3]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.3
[0.8.2]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.2
[0.8.1]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.1
[0.8.0]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.0