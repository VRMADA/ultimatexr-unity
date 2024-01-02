// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSteamVRControllerInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Core;
using UltimateXR.Haptics;
using UnityEngine;
#if ULTIMATEXR_USE_STEAMVR_SDK
using System;
using System.Linq;
using System.Text;
using Valve.VR;
using UltimateXR.Avatar.Rig;
using UltimateXR.Manipulation;
#endif

#pragma warning disable 414 // Disable warnings due to unused values

namespace UltimateXR.Devices.Integrations.SteamVR
{
    /// <summary>
    ///     Base class for all SteamVR input devices.
    ///     Provides common input handling thanks to using the actions exported by the
    ///     SteamVRActionsExporter.
    ///     Child classes will require some overrides and minimal input handling if necessary.
    /// </summary>
    public abstract class UxrSteamVRControllerInput : UxrControllerInput
    {
        #region Inspector Properties/Serialized Fields

        // These will be shown only in custom inspectors for controllers that use them (f.e. index controllers).
        [SerializeField] [HideInInspector] private string _openHandPoseName;
        [SerializeField] [HideInInspector] private float  _indexCurlAmount   = 60.0f;
        [SerializeField] [HideInInspector] private float  _middleCurlAmount  = 60.0f;
        [SerializeField] [HideInInspector] private float  _ringCurlAmount    = 60.0f;
        [SerializeField] [HideInInspector] private float  _littleCurlAmount  = 60.0f;
        [SerializeField] [HideInInspector] private float  _thumbCurlAmount   = 60.0f;
        [SerializeField] [HideInInspector] private float  _thumbSpreadAmount = 30.0f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets list of controller names that the component can handle.
        /// </summary>
        public abstract IEnumerable<string> ControllerNames { get; }

        /// <summary>
        ///     Gets if the class will use hand skeletons.
        /// </summary>
        public virtual bool UsesHandSkeletons => false;

        #endregion

        #region Public Overrides UxrControllerInput

        /// <inheritdoc />
        public override bool IsHandednessSupported => true;

        /// <summary>
        ///     SteamVR child classes will require SteamVR SDK to access functionality.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkSteamVR;

        /// <inheritdoc />
        public override bool IsControllerEnabled(UxrHandSide handSide)
        {
#if ULTIMATEXR_USE_STEAMVR_SDK
            if (s_controllerList.TryGetValue(GetType().Name, out List<int> controllerIndices))
            {
                return controllerIndices.Contains(handSide == UxrHandSide.Left
                                                              ? (int)OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand)
                                                              : (int)OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand));
            }

            return false;
#else
            return false;
#endif
        }

        /// <inheritdoc />
        public override float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0.0f;
            }

#if ULTIMATEXR_USE_STEAMVR_SDK
            SteamVR_Input_Sources source = handSide == UxrHandSide.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;

            if (_actionsInput1D.TryGetValue(input1D, out SteamVR_Action_Single action))
            {
                return action[source].axis;
            }

#endif
            return 0.0f;
        }

        /// <inheritdoc />
        public override Vector2 GetInput2D(UxrHandSide handSide, UxrInput2D input2D, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return Vector2.zero;
            }

#if ULTIMATEXR_USE_STEAMVR_SDK
            SteamVR_Input_Sources source = handSide == UxrHandSide.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;

            if (_actionsInput2D.TryGetValue(input2D, out SteamVR_Action_Vector2 action))
            {
                return FilterTwoAxesDeadZone(action[source].axis, JoystickDeadZone);
            }

#endif
            return Vector2.zero;
        }

        /// <inheritdoc />
        public override void SendHapticFeedback(UxrHandSide handSide, UxrHapticClip hapticClip)
        {
            if (hapticClip.FallbackClipType != UxrHapticClipType.None)
            {
                SendHapticFeedback(handSide, hapticClip.FallbackClipType, hapticClip.FallbackAmplitude, hapticClip.FallbackDurationSeconds, hapticClip.HapticMode);
            }
        }

        /// <inheritdoc />
        public override void SendHapticFeedback(UxrHandSide   handSide,
                                                float         frequency,
                                                float         amplitude,
                                                float         durationSeconds,
                                                UxrHapticMode hapticMode = UxrHapticMode.Mix)
        {
#if ULTIMATEXR_USE_STEAMVR_SDK
            SteamVR_Input_Sources source = handSide == UxrHandSide.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            _handHapticsAction.Execute(0.0f, durationSeconds, frequency, amplitude, source);
#endif
        }

        /// <inheritdoc />
        public override void StopHapticFeedback(UxrHandSide handSide)
        {
            // TODO. Doesn't seem to be supported.
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Takes care of registering the component in the global list of SteamVR input components.
        ///     Builds the action list to access input and starts listening for device connections.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

#if ULTIMATEXR_USE_STEAMVR_SDK
            // Build actions
            BuildActionObjects();

            if (enabled)
            {
                // Listen to device connected events
                SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
                SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
            }

            _awakeFinished = true;

            if (enabled)
            {
                // Disabled by default at the beginning, unless we already these controllers registered.
                // If we already have the controllers registered it is due to an Awake() when loading a new scene.
                enabled             = s_controllerList.TryGetValue(InputClassName, out List<int> controllerIndices) && controllerIndices.Count > 0;
                RaiseConnectOnStart = enabled;

                if (!s_initializedSteamVR)
                {
                    global::Valve.VR.SteamVR.Initialize();
                    SteamVR_Input.GetActionSet(UxrSteamVRConstants.ActionSetName).Activate();
                    s_initializedSteamVR = true;
                }
            }
#else
            enabled = false;
#endif
        }

        /// <summary>
        ///     Called when the component is disabled. In the case the component was using skeletal
        ///     input, the hand will be driven by the Avatar Animator back again.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (UsesHandSkeletons)
            {
                if (!_awakeFinished || !UxrManager.Instance || !Avatar || !Avatar.AvatarController)
                {
                    return;
                }

                UxrStandardAvatarController standardAvatarController = Avatar.AvatarController as UxrStandardAvatarController;

                if (standardAvatarController == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(_openHandPoseName))
                {
                    standardAvatarController.LeftHandDefaultPoseNameOverride  = null;
                    standardAvatarController.RightHandDefaultPoseNameOverride = null;
                    standardAvatarController.LeftHandGrabPoseNameOverride     = null;
                    standardAvatarController.RightHandGrabPoseNameOverride    = null;
                }
            }
        }

        /// <summary>
        ///     Initializes SteamVR if necessary and activates the UltimateXR action set.
        ///     Will initialize skeleton functionality if necessary.
        /// </summary>
        protected override void Start()
        {
            base.Start();

#if ULTIMATEXR_USE_STEAMVR_SDK
            if (UsesHandSkeletons)
            {
                _handSkeletonActionLeft.SetSkeletalTransformSpace(EVRSkeletalTransformSpace.Model);
                _handSkeletonActionRight.SetSkeletalTransformSpace(EVRSkeletalTransformSpace.Model);
            }

#endif
        }

        /// <summary>
        ///     If the component has skeleton capabilities, the hand bones will be updated here.
        /// </summary>
        private void LateUpdate()
        {
            if (Avatar == null)
            {
            }

#if ULTIMATEXR_USE_STEAMVR_SDK
            // Update using skeleton if necessary
            if (UsesHandSkeletons)
            {
                UxrStandardAvatarController avatarControllerStandard = Avatar.AvatarController as UxrStandardAvatarController;
                UxrAvatarRig                avatarRig                = Avatar.AvatarRig;

                if (avatarControllerStandard == null)
                {
                    return;
                }

                float curlIndex  = _handSkeletonActionLeft.fingerCurls[SteamVR_Skeleton_FingerIndexes.index] * _indexCurlAmount;
                float curlMiddle = _handSkeletonActionLeft.fingerCurls[SteamVR_Skeleton_FingerIndexes.middle] * _middleCurlAmount;
                float curlRing   = _handSkeletonActionLeft.fingerCurls[SteamVR_Skeleton_FingerIndexes.ring] * _ringCurlAmount;
                float curlLittle = _handSkeletonActionLeft.fingerCurls[SteamVR_Skeleton_FingerIndexes.pinky] * _littleCurlAmount;
                float curlThumb  = _handSkeletonActionLeft.fingerCurls[SteamVR_Skeleton_FingerIndexes.thumb] * _thumbCurlAmount;
                float splayThumb = _handSkeletonActionLeft.fingerSplays[SteamVR_Skeleton_FingerIndexes.thumb] * _thumbSpreadAmount;

                if (!UxrGrabManager.Instance.IsHandGrabbing(Avatar, UxrHandSide.Left) && !avatarControllerStandard.IsLeftHandInsideFingerPointingVolume)
                {
                    if (!string.IsNullOrEmpty(_openHandPoseName))
                    {
                        avatarControllerStandard.LeftHandDefaultPoseNameOverride = _openHandPoseName;
                        avatarControllerStandard.LeftHandGrabPoseNameOverride    = _openHandPoseName;
                        Avatar.SetCurrentHandPoseImmediately(UxrHandSide.Left, _openHandPoseName);

                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Left, Avatar.LeftHand.Index,  curlIndex,        curlIndex,        curlIndex);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Left, Avatar.LeftHand.Middle, curlMiddle,       curlMiddle,       curlMiddle);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Left, Avatar.LeftHand.Ring,   curlRing,         curlRing,         curlRing);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Left, Avatar.LeftHand.Little, curlLittle,       curlLittle,       curlLittle);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Left, Avatar.LeftHand.Thumb,  curlThumb * 0.1f, curlThumb * 0.3f, curlThumb * 1.0f, splayThumb);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_openHandPoseName))
                    {
                        avatarControllerStandard.LeftHandDefaultPoseNameOverride = null;
                    }
                }

                curlIndex  = _handSkeletonActionRight.fingerCurls[SteamVR_Skeleton_FingerIndexes.index] * _indexCurlAmount;
                curlMiddle = _handSkeletonActionRight.fingerCurls[SteamVR_Skeleton_FingerIndexes.middle] * _middleCurlAmount;
                curlRing   = _handSkeletonActionRight.fingerCurls[SteamVR_Skeleton_FingerIndexes.ring] * _ringCurlAmount;
                curlLittle = _handSkeletonActionRight.fingerCurls[SteamVR_Skeleton_FingerIndexes.pinky] * _littleCurlAmount;
                curlThumb  = _handSkeletonActionRight.fingerCurls[SteamVR_Skeleton_FingerIndexes.thumb] * _thumbCurlAmount;
                splayThumb = _handSkeletonActionRight.fingerSplays[SteamVR_Skeleton_FingerIndexes.thumb] * _thumbSpreadAmount;

                if (!UxrGrabManager.Instance.IsHandGrabbing(Avatar, UxrHandSide.Right) && !avatarControllerStandard.IsRightHandInsideFingerPointingVolume)
                {
                    if (!string.IsNullOrEmpty(_openHandPoseName))
                    {
                        avatarControllerStandard.RightHandDefaultPoseNameOverride = _openHandPoseName;
                        avatarControllerStandard.RightHandGrabPoseNameOverride    = _openHandPoseName;
                        Avatar.SetCurrentHandPoseImmediately(UxrHandSide.Right, _openHandPoseName);

                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Right, Avatar.RightHand.Index,  curlIndex,        curlIndex,        curlIndex);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Right, Avatar.RightHand.Middle, curlMiddle,       curlMiddle,       curlMiddle);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Right, Avatar.RightHand.Ring,   curlRing,         curlRing,         curlRing);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Right, Avatar.RightHand.Little, curlLittle,       curlLittle,       curlLittle);
                        UxrAvatarRig.CurlFinger(Avatar, UxrHandSide.Right, Avatar.RightHand.Thumb,  curlThumb * 0.1f, curlThumb * 0.3f, curlThumb * 1.0f, splayThumb);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_openHandPoseName))
                    {
                        avatarControllerStandard.RightHandDefaultPoseNameOverride = null;
                    }
                }
            }

#endif
        }

        #endregion

        #region Protected Overrides UxrControllerInput

        /// <summary>
        ///     Updates the complete input state using our common SteamVR actions. This allows to use the same interface
        ///     for all controllers and enables the implementation of new devices with minimal effort.
        /// </summary>
        protected override void UpdateInput()
        {
            base.UpdateInput();

            // Get joystick values
            Vector2 leftJoystickValue   = GetInput2D(UxrHandSide.Left,  UxrInput2D.Joystick);
            Vector2 rightJoystickValue  = GetInput2D(UxrHandSide.Right, UxrInput2D.Joystick);
            Vector2 leftJoystick2Value  = GetInput2D(UxrHandSide.Left,  UxrInput2D.Joystick2);
            Vector2 rightJoystick2Value = GetInput2D(UxrHandSide.Right, UxrInput2D.Joystick2);

#if ULTIMATEXR_USE_STEAMVR_SDK
            var system = OpenVR.System;

            if (system == null)
            {
                return;
            }

            // Update buttons
            foreach (UxrInputButtons button in Enum.GetValues(typeof(UxrInputButtons)))
            {
                if (button != UxrInputButtons.None && button != UxrInputButtons.Any && button != UxrInputButtons.Everything)
                {
                    SetButtonFlags(ButtonFlags.PressFlagsLeft,  button, _actionsButtonClick[button][SteamVR_Input_Sources.LeftHand].state);
                    SetButtonFlags(ButtonFlags.PressFlagsRight, button, _actionsButtonClick[button][SteamVR_Input_Sources.RightHand].state);
                    SetButtonFlags(ButtonFlags.TouchFlagsLeft,  button, _actionsButtonTouch[button][SteamVR_Input_Sources.LeftHand].state);
                    SetButtonFlags(ButtonFlags.TouchFlagsRight, button, _actionsButtonTouch[button][SteamVR_Input_Sources.RightHand].state);
                }
            }

#endif

            // These ones are mainly for teleporting functionality when we don't get touch values out of joysticks:
            if (leftJoystickValue != Vector2.zero && leftJoystickValue.magnitude > AnalogAsDPadThreshold)
            {
                SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Joystick, true);
            }

            if (rightJoystickValue != Vector2.zero && rightJoystickValue.magnitude > AnalogAsDPadThreshold)
            {
                SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Joystick, true);
            }

            // Same for joystick2 just in case
            if (leftJoystick2Value != Vector2.zero && leftJoystick2Value.magnitude > AnalogAsDPadThreshold)
            {
                SetButtonFlags(ButtonFlags.TouchFlagsLeft, UxrInputButtons.Joystick2, true);
            }

            if (rightJoystick2Value != Vector2.zero && rightJoystick2Value.magnitude > AnalogAsDPadThreshold)
            {
                SetButtonFlags(ButtonFlags.TouchFlagsRight, UxrInputButtons.Joystick2, true);
            }

            // Update joystick/DPad direction buttons using joystick analog value and pressed state
            uint leftDirectionFlags = GetButtonFlags(MainJoystickIsTouchpad ? ButtonFlags.PressFlagsLeft : ButtonFlags.TouchFlagsLeft);

            if (leftJoystickValue != Vector2.zero && leftJoystickValue.magnitude > AnalogAsDPadThreshold && (leftDirectionFlags & (int)UxrInputButtons.Joystick) != 0)
            {
                float leftJoystickAngle = Input2DToAngle(leftJoystickValue);

                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  IsInput2dDPadLeft(leftJoystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, IsInput2dDPadRight(leftJoystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    IsInput2dDPadUp(leftJoystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  IsInput2dDPadDown(leftJoystickAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadLeft,  false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadUp,    false);
                SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.DPadDown,  false);
            }

            uint leftButtonPressFlags = GetButtonFlags(ButtonFlags.PressFlagsLeft);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickLeft,  (leftButtonPressFlags & (uint)UxrInputButtons.DPadLeft) != 0);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickRight, (leftButtonPressFlags & (uint)UxrInputButtons.DPadRight) != 0);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickUp,    (leftButtonPressFlags & (uint)UxrInputButtons.DPadUp) != 0);
            SetButtonFlags(ButtonFlags.PressFlagsLeft, UxrInputButtons.JoystickDown,  (leftButtonPressFlags & (uint)UxrInputButtons.DPadDown) != 0);

            uint rightDirectionFlags = GetButtonFlags(MainJoystickIsTouchpad ? ButtonFlags.PressFlagsRight : ButtonFlags.TouchFlagsRight);

            if (rightJoystickValue != Vector2.zero && rightJoystickValue.magnitude > AnalogAsDPadThreshold && (rightDirectionFlags & (int)UxrInputButtons.Joystick) != 0)
            {
                float rightJoystickAngle = Input2DToAngle(rightJoystickValue);

                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadLeft,  IsInput2dDPadLeft(rightJoystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadRight, IsInput2dDPadRight(rightJoystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadUp,    IsInput2dDPadUp(rightJoystickAngle));
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadDown,  IsInput2dDPadDown(rightJoystickAngle));
            }
            else
            {
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadLeft,  false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadRight, false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadUp,    false);
                SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.DPadDown,  false);
            }

            uint rightButtonPressFlags = GetButtonFlags(ButtonFlags.PressFlagsRight);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickLeft,  (rightButtonPressFlags & (uint)UxrInputButtons.DPadLeft) != 0);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickRight, (rightButtonPressFlags & (uint)UxrInputButtons.DPadRight) != 0);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickUp,    (rightButtonPressFlags & (uint)UxrInputButtons.DPadUp) != 0);
            SetButtonFlags(ButtonFlags.PressFlagsRight, UxrInputButtons.JoystickDown,  (rightButtonPressFlags & (uint)UxrInputButtons.DPadDown) != 0);
        }

        #endregion

        #region Private Types & Data

        private string InputClassName => GetType().Name;

        // Global data
        private static readonly Dictionary<string, List<int>> s_controllerList = new Dictionary<string, List<int>>();
        private static          bool                          s_initializedSteamVR;

        // Local data
        private bool _awakeFinished;

        #endregion

#if ULTIMATEXR_USE_STEAMVR_SDK

        /// <summary>
        ///     Given a controller name, gets a list of controller names using the Virtual Desktop controller naming convention.  
        /// </summary>
        /// <param name="controllerName">Controller name to get the virtual desktop controller names for</param>
        /// <returns>List of virtual desktop controller names</returns>
        private static IEnumerable<string> GetVirtualDesktopWrappedControllerNames(string controllerName)
        {
            yield return $"OpenVR Controller({controllerName}) - Left";
            yield return $"OpenVR Controller({controllerName}) - Right";
        }

        /// <summary>
        ///     Called when a SteamVR device is connected
        /// </summary>
        /// <param name="index">Device index</param>
        /// <param name="connected">True if connected, false if disconnected</param>
        private static void OnDeviceConnected(int index, bool connected)
        {
            if (OpenVR.System == null)
            {
                if (LogLevel >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{nameof(UxrSteamVRControllerInput)}::{nameof(OnDeviceConnected)}: OpenVR.System is null");
                }
                return;
            }

            if (OpenVR.System.GetTrackedDeviceClass((uint)index) != ETrackedDeviceClass.Controller)
            {
                // Ignore devices that aren't controllers
                return;
            }

            var renderModelName = new StringBuilder(ModelNameMaxLength);
            var error           = ETrackedPropertyError.TrackedProp_Success;

            OpenVR.System.GetStringTrackedDeviceProperty((uint)index, ETrackedDeviceProperty.Prop_ModelNumber_String, renderModelName, ModelNameMaxLength, ref error);

            string modelNameString = renderModelName.ToString();

            if (LogLevel >= UxrLogLevel.Relevant)
            {
                Debug.Log($"{nameof(UxrSteamVRControllerInput)}::{nameof(OnDeviceConnected)}: connected={connected}, model={modelNameString}");
            }

            IEnumerable<UxrSteamVRControllerInput> inputsSteamVR = AllComponents.Where(i => i is UxrSteamVRControllerInput).Cast<UxrSteamVRControllerInput>();

            UxrSteamVRControllerInput inputSteamVR = inputsSteamVR.FirstOrDefault(i =>
                        i.ControllerNames.Any(n => string.Equals(n, modelNameString)) ||  i.ControllerNames.SelectMany(GetVirtualDesktopWrappedControllerNames).Any(n => string.Equals(n, modelNameString)));

            if (inputSteamVR != null)
            {
                // Model is one of the registered SteamVR inputs and needs to be processed
                if (LogLevel >= UxrLogLevel.Relevant)
                {
                    Debug.Log($"{nameof(UxrSteamVRControllerInput)}::{nameof(OnDeviceConnected)}: Device name {modelNameString} was registered by {inputSteamVR.InputClassName} and is being processed!");
                }
                
                if (!s_controllerList.TryGetValue(inputSteamVR.InputClassName, out List<int> controllerIndices))
                {
                    controllerIndices = new List<int>();
                    s_controllerList.Add(inputSteamVR.InputClassName, controllerIndices);
                }

                if (connected)
                {
                    // Connected
                    controllerIndices.Add(index);

                    if (inputSteamVR.enabled == false)
                    {
                        // First controller: Notify device is connected since we consider the device the whole setup
                        inputSteamVR.enabled = true;
                        inputSteamVR.OnDeviceConnected(new UxrDeviceConnectEventArgs(true));
                    }
                }
                else
                {
                    // Disconnected
                    controllerIndices.Remove(index);

                    if (controllerIndices.Count == 0)
                    {
                        // Last controller disconnected: Notify device is disconnected.
                        inputSteamVR.enabled = false;
                        inputSteamVR.OnDeviceConnected(new UxrDeviceConnectEventArgs(false));
                    }
                }
            }
            else
            {
                if (LogLevel >= UxrLogLevel.Relevant)
                {
                    Debug.Log($"{nameof(UxrSteamVRControllerInput)}::{nameof(OnDeviceConnected)}: Device is not recognized as input by any of {inputsSteamVR.Count()} components");
                }
            }
        }

        /// <summary>
        ///     Gets the action bound to the given button and input type.
        /// </summary>
        /// <param name="button">Button to look for</param>
        /// <param name="inputType">Type of input to handle. Use <see cref="UxrSteamVRConstants" />.</param>
        /// <returns>
        ///     Action bound to the given button and input type. If the button doesn't exist
        ///     in the current controller it will return a fake action showing no input
        /// </returns>
        private static SteamVR_Action_Boolean GetButtonAction(UxrInputButtons button, string inputType)
        {
            return SteamVR_Input.GetAction<SteamVR_Action_Boolean>(UxrSteamVRConstants.ActionSetName, $"{button.ToString().ToLower()}_{inputType}_{UxrSteamVRConstants.BindingVarBool}");
        }

        /// <summary>
        ///     Gets the action bound to the given <see cref="UxrInput1D" />.
        /// </summary>
        /// <param name="input1D">Element to look for</param>
        /// <returns>
        ///     Action bound to the given 1D input. If the element doesn't exist in the current controller it will return a
        ///     fake action showing no input
        /// </returns> 
        private static SteamVR_Action_Single GetInput1DAction(UxrInput1D input1D)
        {
            return SteamVR_Input.GetAction<SteamVR_Action_Single>(UxrSteamVRConstants.ActionSetName, $"{input1D.ToString().ToLower()}_{UxrSteamVRConstants.BindingVarVector1}");
        }

        /// <summary>
        ///     Gets the action bound to the given <see cref="UxrInput2D" />.
        /// </summary>
        /// <param name="input2D">Element to look for</param>
        /// <returns>
        ///     Action bound to the given 2D input. If the element doesn't exist in the current controller it will return a fake
        ///     action showing no input
        /// </returns>
        private static SteamVR_Action_Vector2 GetInput2DAction(UxrInput2D input2D)
        {
            return SteamVR_Input.GetAction<SteamVR_Action_Vector2>(UxrSteamVRConstants.ActionSetName, $"{input2D.ToString().ToLower()}_{UxrSteamVRConstants.BindingVarVector2}");
        }

        /// <summary>
        ///     Builds all action objects needed to check for input using SteamVR.
        ///     We use enumeration of all elements inside an Enum to build our action list and,
        ///     thanks to the functionality of SteamVR, when an action doesn't exist it will
        ///     generate a fake action showing no input.
        /// </summary>
        private void BuildActionObjects()
        {
            // Buttons
            foreach (UxrInputButtons button in Enum.GetValues(typeof(UxrInputButtons)))
            {
                if (button != UxrInputButtons.None && button != UxrInputButtons.Any && button != UxrInputButtons.Everything)
                {
                    _actionsButtonClick.Add(button, GetButtonAction(button, UxrSteamVRConstants.BindingInputClick));
                    _actionsButtonTouch.Add(button, GetButtonAction(button, UxrSteamVRConstants.BindingInputTouch));
                }
            }

            // UxrInput1D
            foreach (UxrInput1D input1D in Enum.GetValues(typeof(UxrInput1D)))
            {
                if (input1D != UxrInput1D.None)
                {
                    _actionsInput1D.Add(input1D, GetInput1DAction(input1D));
                }
            }

            // UxrInput2D
            foreach (UxrInput2D input2D in Enum.GetValues(typeof(UxrInput2D)))
            {
                if (input2D != UxrInput2D.None)
                {
                    _actionsInput2D.Add(input2D, GetInput2DAction(input2D));
                }
            }
        }

#endif

#if ULTIMATEXR_USE_STEAMVR_SDK
        private const int ModelNameMaxLength = 256;

        private readonly Dictionary<UxrInputButtons, SteamVR_Action_Boolean> _actionsButtonClick = new Dictionary<UxrInputButtons, SteamVR_Action_Boolean>();
        private readonly Dictionary<UxrInputButtons, SteamVR_Action_Boolean> _actionsButtonTouch = new Dictionary<UxrInputButtons, SteamVR_Action_Boolean>();
        private readonly Dictionary<UxrInput1D, SteamVR_Action_Single>       _actionsInput1D     = new Dictionary<UxrInput1D, SteamVR_Action_Single>();
        private readonly Dictionary<UxrInput2D, SteamVR_Action_Vector2>      _actionsInput2D     = new Dictionary<UxrInput2D, SteamVR_Action_Vector2>();

        private readonly SteamVR_Action_Skeleton  _handSkeletonActionLeft  = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>(UxrSteamVRConstants.ActionSetName, UxrSteamVRConstants.ActionNameHandSkeletonLeft);
        private readonly SteamVR_Action_Skeleton  _handSkeletonActionRight = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>(UxrSteamVRConstants.ActionSetName, UxrSteamVRConstants.ActionNameHandSkeletonRight);
        private readonly SteamVR_Action_Vibration _handHapticsAction       = SteamVR_Input.GetAction<SteamVR_Action_Vibration>(UxrSteamVRConstants.ActionSetName, UxrSteamVRConstants.ActionNameHandHaptics);

#endif
    }
}

#pragma warning restore 414 // Restore warnings due to unused values