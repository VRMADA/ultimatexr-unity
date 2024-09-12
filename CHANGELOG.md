# Changelog

All notable changes to UltimateXR will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

TODO: Write update guide:
  -UxrApplyConstrainEventArgs new properties.
  -Rename CheckAndApplyLockHands() to KeepGripsInPlace()
  -Removed UxrGrabbableObject.PlaceOnAnchor
  -SDK constants moved from UxrManager to UxrConstants.
  -Component synchronization in UxrManager now uses IUxrStateSync instead of
   UxrComponent.
  -UxrComponent.TryGetComponentById now is UxrUniqueIdImplementer.TryGetComponentById().
  -UxrManager.ExecuteStateChange is now called ExecuteStateSyncEvent.
  -Avatar prefab that is set up using UxrNetworkManager inspector, now is changed
   to "Update Externally" mode so that it is instantiated using external mode by default.
   Then it will switch to local if the avatar spawned is the local avatar.

### Added

- Add native multiplayer support and connectors for various network SDKs
  (Photon Fusion, Unity NetCode, Mirror) and voice communication SDKs
  (Photon Voice, Dissonance). More connectors will be added soon.
- Add UxrNetworkManager and UxrInstanceManager to provice sync capabilities.
- Update SDK Manager window with new tabs for SDK types, including new multiplayer.
- Add new GlobalSettings accessible using the Tools->UltimateXR Unity menu.
- Add new unique ID functionality to UXR components. All components that inherit
  from UxrComponent will have a unique ID that can be used with
  UxrUniqueIdImplementer.TryGetComponentById().
- Add new Unique ID generation tool to generate unique IDs for projects build with
  earlier versions of UltimateXR.
- Add IUxrUniqueId, IUxrStateSync and IUxrStateSave interfaces to UxrComponent to
  facilitate multiplayer synchronization, serialization, state saves and replay
  functionality in all UltimateXR components.
- Add UxrUniqueIdImplementer, UxrStateSaveImplementer and UxrStateSyncImplementer
  to leverage the interface implementation in custom user classes that cannot
  inherit from UxrComponent due to multiple inheritance limitation.
- Add functionality to UxrManager to save and load the scene state using
  SaveStateChanges() and LoadStateChanges().
- Add functionality to UxrManager to have a single point of entry to all component
  state changes that require synchronization:
  ComponentStateChanged event and ExecuteStateChange() method.
- Add serialization/deserialization methods to all UxrComponent derived classes.
- Add serialization/deserialization methods to all UxrSyncEventArgs.
- Create UxrVarType to enumerate all supported var types that can be synchronized.
- Add ToString() to all UxrSyncEventArgs to help data logging.
- Add new property Options to UxrSyncEventArgs to tell whether the event should
  be synchronized in different environments such as networks or replays.
- Add new BinaryWriter and BinaryReader extensions with functionality to
  serialize/deserialize Unity and UltimateXR data, as well as well-known
  C# types.
- Add new UxrBinarySerializer with binary serialization and deserialization support.
- Add new UxrSyncObject component to identify objects that don't have any
  UxrComponent added, so that they have a unique ID. Also to sync the Transform
  of a GameObject if it's required and no other component does it already.
- Add new interpolators to Math namespace to interpolate different types of
  variables using optional smooth and step options.
- Add support to solve manipulations on objects using an unlimited amount of
  grabs to support multi-user shared interaction.
- Add support to grab grabbable parent objects through grabbable children.
- Add support for dummy grabbable parents. Dummy grabbable parents are grabbable
  objects that can be manipulating only through the children, but still have
  position/rotation constraints.
- Add new functionality ForceSnapPosition and ForceSnapRotation to
  UxrPlacementOptions to force snap even if the grabbable object isn't set up
  that way.
- Add new functionality in UxrGrabManager to move grabbable objects considering
  constraints: SetLocalPositionUsingConstraints(), SetLocalRotationUsingConstraints(),
  SetLocalPositionAndRotationUsingConstraints(), SetPositionUsingConstraints(),
  SetRotationUsingConstraints() and SetPositionAndRotationUsingConstraints().  
- Add new UxrAutoSlideInObject/Anchor components to provide built-in functionality
  like how the battery in the example scene can be inserted/removed smoothly
  from the generators.
- Add new UxrGrabbableResizable component to provide functionality to create
  objects that can be scaled by grabbing them from both sides.
- Add new ComponentProcessorWindow editor base class to generate component
  processors that can modify from single components to the whole project.
- Add UxrEditorUtils.ProcessAllProjectComponents() functionality to process both
  innermost and non-innermost prefab elements.
- Add UxrEditorUtils.ModifyComponent() to perform changes to components in the
  scene or project separately processing instances, prefab variants and original
  prefabs.
- Add automatic detection of UXR installation path.
- Add new menus in Tools->UltimateXR.
- Add PushTransform() and PopTransform() methods in UxrComponent to facilitate
  saving and restoring transformation info.
- Add new parameters to UxrColorTween.AnimateBlinkAlpha to support min alpha and
  max alpha.
- Add new PBR shader with a tinted mask. Use in cyborg avatar to color it.
- Add new cabinet prefab to Lab in example scene to showcase grabbable parent dummies.
- Expose properties in UxrTeleportSpawnCollider to have public access.

### Changed

- Improve manipulation workflow to a system that is more scalable, robust and
  solves the whole interaction process in different stages.
- Separate UxrGrabManager in different files based on functionality.
- Remove all properties from UxrApplyConstrainEventArgs leaving only GrabbableObject.
  Constraints are applied on an object level so it doesn't make sense to use grabbers,
  especially in multi-user environments.
- Remove RequireComponent attribute from UxrGrabbableObjectComponent that forces
  a UxrGrabbableObject component on the same GameObject. GrabbableObject can be
  required or not depending on overridable property IsGrabbableObjectRequired.
- Rename IsOwnershipChanged in UxrManipulationEventArgs to IsGrabbedStateChanged.
- UxrGrabPointShape now gets additional grabberDistanceTransform to also use
  non-default grabber proximity transforms.
- Change how grabbable parenting works: Set to avatar's parent when grabbing and
  to anchor's parent when removing from anchor without grabbing.
- Move SDK constants from UxrManager to UxrConstants.
- Move input/tracking SDK locators from UltimateXR/Editor/Sdks to
  UltimateXR/Editor/Sdks/InputTracking.
- Add teleportation validators to UxrTeleportLocomotionBase to create custom logic
  for destination validation and cancel a teleportation.
- Move AvatarMoving and AvatarMoved events to UxrAvatar as GlobalAvatarMoving and
  GlobalAvatarMoved static events.
- Change Teleported event in UxrTeleportSpawnCollider to use new
  UxrTeleportSpawnUsedEventArgs parameter.
- Change UxrComponent unique ID functionality so that IDs are generated in the
  editor instead of using unique paths in scene with ComponentExt.GetUniqueScenePath().
- Improve avatar arm rig reference solving for hierarchies with siblings in
  the clavicle, upper arm or forearm.
- Change GetUniqueScenePath in ComponentExt and TransformExt to make it cleaner.
- Move AlignWindow, LookAtWindow and MirrorWindow tools to new namespace/folder in
  Editor/Utilities/TransformUtils.
- Improve UxrEditorUtils.Prefabs functionality.
- Improve TaskControllers documentation and functionality.
- Improve UxrLookAt component with new axis functionality.
- Make UxrControllerTracking's properties UpdateAvatarLeftHand/UpdateAvatarRightHand
  public instead of protected.
- Allow UxrTeleportLocomotionBase to be disabled to ignore the component.

### Fixed

- Fix UxrCameraPointer's ClickInput to Everything by default instead of None to avoid
  clicking each frame.
- Fix bug in UxrPointerInputModule that prevents pointer up notifications being
  generated when the finger tip/pointer is dragged out of the UI.
- Fix child dependent grabbable objects not being dynamic when releasing.
- Fix bug in avatar IK where the coordinate system of clavicles/upperarm/forearm
  is inferred erroneously. Arms can look twisted.
- Fix bug in standard avatar controller inspector when clicking Use Avatar Eyes.
- Fix bug in UxrManager to avoid pre-caching multiple times when client network
  avatars are instantiated.
- Fix bug in UxrManager where aync versions of teleport cause an await() to wait
  indefinitely when overlapping more than one teleport at the same time.
- Fix bug in UxrTeleportSpawnCollider that throws null reference exceptions when
  Transform references are not set. When references are null, the Transform of the
  GameObject where the UxrTeleportSpawnCollider is added should be used instead.
- Fix UxrEditorUtils.ProcessAllProjectComponents() not processing all components
  correctly.
- Fix UxrEditorUtils.GetInnermostNon3DModelPrefabRoot() not working on all
  prefab/instance hierarchies correctly.
- Fix UxrEditorUtils.GetInnermostNon3DModelPrefabRoot() to handle 3D model prefabs
  correctly.
- Fix UxrUnityXRControllerInput when controllers have touchpads.
- Fix warnings in example scene when loading ShotgunPump01.mp3 and ShotgunPump02.mp3
- Fix missing EditorGUI.EndProperty() in UxrAxisPropertyDrawer.
- Move UxrLaserPointerTargetTypes to correct namespace UltimateXR.UI instead of
  root UltimateXR namespace.
- Fix UxrGrabPointShapeAxisAngle to compute center of grab correctly using snap point
  instead of using axis center.
- Implement missing minAngle and maxAngle functionality in UxrGrabPointShapeAxisAngle.
- Fix HandPositionAroundPivot manipulation mode drifting when grabbable object is
  child of another grabbable object.
- Fix preview grab poses to work on hierarchies with non-uniform scaling.
- Fix Lock Body Pivot parameter drifting in UxrStandardAvatarController.
- Fix bug in UI system to detect fingertip presses correctly in a moving canvas.
- Fix bug in UxrAvatar.LocalAvatar where a disabled avatar can be returned when
  there are two or more instances of an avatar with Local update mode in the scene.
- Fix bug in manipulation system where throwing physics-driven objects sometimes
  has a small lag between the button release and the physics.

## [0.9.7] - 2024-01-10
  
### Added

- Add support for Meta Quest 3.
- Add support for Magic Leap 2.
- Add support for Virtual Desktop controller naming.
- Add support for Unity UI input on the screen and UltimateXR UI input in VR at
  the same time.
- Add new functionality DontRelease to UxrPlacementOptions that keeps the object
  grabbed when UxrManager.Instance.PlaceObject() is called.
- Add MinSingleRotationDegrees/MaxSingleRotationDegrees to UxrGrabbableObject when
  constrained to a single degree of freedom.
- Add new symbol ULTIMATEXR_UNITY_XR_OCULUS when Unity.XR.Oculus is available.
- Add joystick deadzone filtering in SteamVR.
- Add support for position/rotation smoothing in all controller tracking components.
- Add new UxrLinearPath spline type for linear interpolation in paths.

### Changed

- Improve teleportation raycasts to discard avatar colliders and grabbed objects.
- Improve teleportation to handle avatars with roll/pitch.
- Improve Body IK to handle avatars with roll/pitch. Improved precision by
  performing computations in local avatar space.
- Rename UxrPlacementType to UxrPlacementOptions.
- Improve support for HandPositionAroundPivot manipulation mode.
- Disable UxrInputModule component parameter "Disable Other Input Modules" by default
  instead of being enabled.
- Remove deprecated references to CommonUsages.thumbrest and CommonUsages.thumbTouch
  in UxrUnityXRControllerInput.cs and use OculusUsages.thumbrest and
  OculusUsages.thumbTouch instead if available. Add support for OculusUsages.indexTouch.

### Fixed

- Fix UxrLaserPointer hit quad position using controller forward.
- Fix Pico controllers not working after using home button.
- Fix Valve Index controllers' forward vectors.
- Fix laser pointers not working correctly when mixing UI with 2D/3D objects.
- Fix bug in UI module where finger tips and laser pointers cannot interact with
  multiple canvases when close to each other.
- Fix null reference exception in manipulation system when placing constrained objects
  on anchors and grabbing them again.
- Fix bug in UxrGrabManager that prevents GrabToggle manipulation mode to place
  objects on anchors.
- Fix UxrGrabbableObject manipulation not working correctly when grab points are moved
  around during grabbing, for example when applying constraints.
- Fix bug in UxrGrabbableObject.SetGrabPointEnabled not working correctly.
- Fix UxrGrabPointShapes not computing center of grab correctly in some cases.
- Fix scaling on root avatar GameObject not working correctly with Body/Arm IK.
- Fix the following global input events in UxrControllerInput not being called:
  GlobalButtonStateChanged, GlobalInput1DChanged, GlobalInput2DChanged.
- Fix UxrUnityXRControllerInput components not getting haptic capabilities correctly.
- Fix warnings in example scene when loading ShotgunPump01.mp3 and ShotgunPump02.mp3

### Removed

- Remove deprecated references to CommonUsages.thumbrest and CommonUsages.thumbTouch
  in UxrUnityXRControllerInput.cs and use OculusUsages.thumbrest and
  OculusUsages.thumbTouch instead if available.

## [0.9.6] - 2023-01-18

### Added

- Add SteamVR support for Rift/Rift-S/Quest/Quest2 headsets and controllers.
- Add selective 2D/3D/UI GameObject interaction to UxrLaserPointer.
- Add PrecachingStarting and PrecachingFinished events to UxrManager.
- Add new exposed parameters to UxrLaserPointer for scripting.
- Add new exposed parameters to UxrPointerEventData for scripting.
- Add LocalStandardAvatarController property to UxrAvatar for quick access.

### Changed

- Improve UxrLaserPointer inspector.
- Improve UxrPointerInputModule event handling.
- Make UxrControllerInput::GetIgnoreControllerInput() and SetIgnoreControllerInput()
  static so that they can be called at any point whether the controllers are active or not.
- Change some common operations to favor execution time:
  [#12](https://github.com/VRMADA/ultimatexr-unity/pull/12).
- Make grab preview poses no longer shown by default during play mode in the editor.
  Preview GameObjects are initially deactivated.
- Improve hand pose editor load/save dialog boxes by caching the last load and save
  folders separately.
- Change .meta files in Examples\FullScene\Settings\URP so that the IDs don't collide
  with the default URP project IDs.

### Fixed

- Fix Transform.SetLocalPositionAndRotation when not available through new Unity API.
- Fix UxrLaserPointer to not send UI events when laser is disabled.
- Fix uninitialized hand pose when hand tracking is supported but not available.
- Fix grabbable object position constraint not working correctly when grabbed using
  both hands.
- Fix UxrGrabbableInspector not storing correctly new grab point parameters right
  after it has been created.
- Fix Grab Toggle mode in UxrGrabbableObject not keeping the pose during the grab.
- Fix "Enable When Hand Near" parameter in UxrGrabbableObject being enabled incorrectly
  sometimes when another grabbed object was in closer range.
- Fix hand grab pose incorrectly changing when moving within the range of a grabbable
  object enabled by a non-default grab button.
- Fix bug in hand pose editor that prevents to load external pose files when using
  UltimateXR in package installation mode.
- Fix bug in hand pose editor where the "Add all poses from folder" loads all hand
  pose presets instead.
- Fix UxrGrabManager's GrabObject, PlaceObject, ReleaseObject direct methods calls not
  updating the avatar's grab pose.
- Fix global events in UxrControllerInput that should be static but are not:
  GlobalButtonStateChanged, GlobalInput1DChanged, GlobalInput2DChanged,
  GlobalHapticRequesting.
- Fix UxrAvatar Reset to make it override.
- Fix UxrAvatar.LaserPointers to return correct laser pointers instead of finger tips.
- Fix avatar parent prefab not being stored correctly when inside a nested prefab.
- Fix UxrGrenadeWeapon pin so that the timer cannot be reset by quickly releasing
  and grabbing the pin again.
- Fix UxrSteamControllerInput so that OnDeviceConnected is called only once.

## [0.9.5] - 2022-11-12

### Added

- Improve automatic avatar rig bone reference solving.
- Improve automatic generation of body IK setup in avatar automatic setup.
- Add UxrWristTorsionIKSolver component when torsion bones are found in avatar.
- Improve UxrStandardAvatarController inspector when IK is selected but rig has no nodes.
- Add TrackedHandPose to UxrControllerInputCapabilities enum and applied to Valve Index.
- Add public method SolveBodyIK() to still use body IK when AvatarMode is UpdateExternally.
- Add support to isolate the hand part of the mesh in the hand preview poses if the hands
  are in the same mesh as the body.

### Changed

- Set avatar rig type to full/half body when body bones are found in the avatar rig.

### Fixed

- Fix UxrWristTorsionInfo, UxrWristTorsionIKSolver and UxrAvatarArmInfo to generate
  correct data on all avatar rig coordinate systems.
- Fix components that don't override Reset() or OnValidate().
- Fix body IK when no neck bone is present.
- Fix Valve Index controllers not sending UI input events when adopting a hand pose
  with the middle finger curled.
- Fix bug in avatar finger bone reference solving if the finger has already data.

## [0.9.4] - 2022-10-29

### Added

- Add IUxrGrabbableModifier interface to create components that control certain parts
  of an UxrGrabbableObject. The UxrGrabbableObject inspector automatically disables the
  controlled UI sections and has also capability to show/hide the controlled parameters.
  The goal is to provide a scalable way to extend grabbable functionality by adding
  modifier components to the object.
- Add Reset() and OnValidate() to the overridable Unity methods in UxrComponent.
- Add ConstraintsFinished to UxrGrabbableObject to create logic after custom constraints
  have been applied.
- Add constants to UxrGrabbableObjectEditor for UxrGrabbableObject field names.
- Add "Any" variations to GetButtonsPress/GetButtonsTouch when multiple buttons are
  specified so that any button in the set is enough to meet the criteria instead of all.
- Add EnumExt for Enum extensions.
- Add static events to UxrControllerInput to receive events whenever any controller
  sends input data.

### Changed

- Change UxrApplyConstraintsEventArgs to contain the UxrGrabber instead of UxrGrabbableObject.
  The UxrGrabbableObject can still be accessed using the GrabbedObject property from
  the grabber.
- Use ConstraintsFinished in UxrManipulationHapticFeedback in order to process object
  after custom constraints have been applied.  
- Rename UxrLocomotionTeleportBaseEditor to UxrTeleportLocomotionBaseEditor.
- Update example scene prefabs so that they show the cyborg grab poses by default.

### Fixed

- Fix support for PicoXR controller detection on newer versions of the PicoXR Unity SDK.
- Remove Universal Additional Camera Data scripts added incorrectly to BRP avatar variants.
  Affected avatars are SmallHandsAvatar_BRP, BigHandsAvatar_BRP and CyborgAvatar_BRP.
- Fix joystick directional buttons (left/right/up/down) when getting ignored input.
- Fix bug in UxrGrabbableObjectEditor that under some circumstances throws exceptions
  when previewing grab poses.
- Fix UxrGrabbableObject constrained rotation on a single axis not working correctly when
  parent has different axes.
- Fix UxrTeleportSpawnCollider not raising Teleported event.

## [0.9.3] - 2022-10-24

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
- Fix some CyborgAvatar_BRP base materials that are using the URP variants.

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

[Unreleased]: https://github.com/VRMADA/ultimatexr-unity/compare/v0.9.7...HEAD
[0.9.7]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.7
[0.9.6]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.6
[0.9.5]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.5
[0.9.4]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.4
[0.9.3]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.3
[0.9.2]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.2
[0.9.1]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.1
[0.9.0]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.9.0
[0.8.4]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.4
[0.8.3]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.3
[0.8.2]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.2
[0.8.1]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.1
[0.8.0]: https://github.com/VRMADA/ultimatexr-unity/releases/tag/v0.8.0