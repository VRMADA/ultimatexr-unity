// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.GameObjects;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Devices.Integrations;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.System.Math;
using UltimateXR.Extensions.Unity;
using UltimateXR.Haptics;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Controller base class for all VR input devices, supporting single controllers and dual controller setups.
    /// </summary>
    public abstract partial class UxrControllerInput : UxrAvatarComponent<UxrControllerInput>, IUxrControllerInput, IUxrControllerInputUpdater
    {
        #region Inspector Properties/Serialized Fields

        [Header("Controllers in the avatar hierarchy:")] [SerializeField] protected UxrController3DModel _leftController;
        [SerializeField]                                                  protected UxrController3DModel _rightController;
        [SerializeField]                                                  protected UxrController3DModel _controller;

        [Header("Avatar objects to enable when active:")] [Tooltip("Enables game objects based when the left input device is present.")] [SerializeField]  protected List<GameObject> _enableObjectListLeft;
        [Tooltip(                                                  "Enables game objects based when the right input device is present.")] [SerializeField] protected List<GameObject> _enableObjectListRight;
        [Header("Avatar objects to enable when active:")] [Tooltip("Enables game objects based on the presence of the input device.")] [SerializeField]    protected List<GameObject> _enableObjectList;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called whenever any controller input device is connected or disconnected
        /// </summary>
        public static event EventHandler<UxrDeviceConnectEventArgs> GlobalControllerConnected;

        /// <summary>
        ///     Event called after any controller button state changed.
        /// </summary>
        public static event EventHandler<UxrInputButtonEventArgs> GlobalButtonStateChanged;

        /// <summary>
        ///     Event called after any <see cref="UxrInput1D" /> element changed.
        /// </summary>
        public static event EventHandler<UxrInput1DEventArgs> GlobalInput1DChanged;

        /// <summary>
        ///     Event called after any <see cref="UxrInput2D" /> element changed.
        /// </summary>
        public static event EventHandler<UxrInput2DEventArgs> GlobalInput2DChanged;

        /// <summary>
        ///     Event called right before any haptic feedback was requested.
        /// </summary>
        public static event EventHandler<UxrControllerHapticEventArgs> GlobalHapticRequesting;

        /// <summary>
        ///     Gets the current input component, which is the enabled input component belonging to the local avatar.
        ///     If the avatar has no input component enabled, it will return a dummy input so that there is no need to check
        ///     for nulls. This dummy component doesn't generate input events.
        ///     The only way to get a null would be if there is no local avatar in the scene.
        /// </summary>
        public static UxrControllerInput Current
        {
            get
            {
                // We use this one first because it will return a dummy component if there is no input component enabled
                if (LocalAvatar)
                {
                    return LocalAvatar.ControllerInput;
                }

                // Else, return first enabled non-dummy component. Can be null.
                return EnabledComponentsInLocalAvatar.FirstOrDefault(i => i.GetType() != typeof(UxrDummyControllerInput));
            }
        }

        /// <summary>
        ///     Gets or sets the current log level. This controls the amount of information sent.
        /// </summary>
        public static UxrLogLevel LogLevel { get; set; } = UxrLogLevel.Relevant;

        #endregion

        #region Implicit IUxrControllerInput

        /// <inheritdoc />
        public abstract UxrControllerSetupType SetupType { get; }

        /// <inheritdoc />
        public abstract bool IsHandednessSupported { get; }

        /// <inheritdoc />
        public virtual string LeftControllerName => string.Empty;

        /// <inheritdoc />
        public virtual string RightControllerName => string.Empty;

        /// <inheritdoc />
        public virtual bool MainJoystickIsTouchpad => false;

        /// <inheritdoc />
        public virtual float JoystickDeadZone => 0.15f;

        /// <inheritdoc />
        public virtual UxrHandSide Handedness { get; set; } = UxrHandSide.Right;

        /// <inheritdoc />
        public UxrHandSide Primary => Handedness == UxrHandSide.Left ? UxrHandSide.Left : UxrHandSide.Right;

        /// <inheritdoc />
        public UxrHandSide Secondary => Handedness == UxrHandSide.Left ? UxrHandSide.Right : UxrHandSide.Left;

        /// <inheritdoc />
        public UxrController3DModel LeftController3DModel => SetupType == UxrControllerSetupType.Dual ? _leftController : _controller;

        /// <inheritdoc />
        public UxrController3DModel RightController3DModel => SetupType == UxrControllerSetupType.Dual ? _rightController : _controller;

        /// <inheritdoc />
        public event EventHandler Updating;

        /// <inheritdoc />
        public event EventHandler Updated;

        /// <inheritdoc />
        public event EventHandler<UxrInputButtonEventArgs> ButtonStateChanged;

        /// <inheritdoc />
        public event EventHandler<UxrInput1DEventArgs> Input1DChanged;

        /// <inheritdoc />
        public event EventHandler<UxrInput2DEventArgs> Input2DChanged;

        /// <inheritdoc />
        public event EventHandler<UxrControllerHapticEventArgs> HapticRequesting;

        /// <inheritdoc />
        public abstract bool IsControllerEnabled(UxrHandSide handSide);

        /// <inheritdoc />
        public abstract bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElement);

        /// <inheritdoc />
        public abstract UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide);

        /// <inheritdoc />
        public abstract float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false);

        /// <inheritdoc />
        public abstract Vector2 GetInput2D(UxrHandSide handSide, UxrInput2D input2D, bool getIgnoredInput = false);

        /// <inheritdoc />
        public virtual void SendHapticFeedback(UxrHandSide handSide, UxrHapticClip hapticClip)
        {
            OnHapticRequesting(new UxrControllerHapticEventArgs(handSide, hapticClip));
        }

        /// <inheritdoc />
        public virtual void SendHapticFeedback(UxrHandSide   handSide,
                                               float         frequency,
                                               float         amplitude,
                                               float         durationSeconds,
                                               UxrHapticMode hapticMode = UxrHapticMode.Mix)
        {
            OnHapticRequesting(new UxrControllerHapticEventArgs(handSide, frequency, amplitude, durationSeconds, hapticMode));
        }

        /// <inheritdoc />
        public virtual void StopHapticFeedback(UxrHandSide handSide)
        {
            OnHapticRequesting(UxrControllerHapticEventArgs.GetHapticStopEvent(handSide));
        }

        /// <inheritdoc />
        public uint GetButtonTouchFlags(UxrHandSide handSide, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0;
            }

            return handSide == UxrHandSide.Right ? _buttonTouchFlagsRight : _buttonTouchFlagsLeft;
        }

        /// <inheritdoc />
        public uint GetButtonTouchFlagsLastFrame(UxrHandSide handSide, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0;
            }

            return handSide == UxrHandSide.Right ? _buttonTouchFlagsLastFrameRight : _buttonTouchFlagsLastFrameLeft;
        }

        /// <inheritdoc />
        public uint GetButtonPressFlags(UxrHandSide handSide, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0;
            }

            return handSide == UxrHandSide.Right ? _buttonPressFlagsRight : _buttonPressFlagsLeft;
        }

        /// <inheritdoc />
        public uint GetButtonPressFlagsLastFrame(UxrHandSide handSide, bool getIgnoredInput = false)
        {
            if (ShouldIgnoreInput(handSide, getIgnoredInput))
            {
                return 0;
            }

            return handSide == UxrHandSide.Right ? _buttonPressFlagsLastFrameRight : _buttonPressFlagsLastFrameLeft;
        }

        /// <inheritdoc />
        public bool GetButtonsEvent(UxrHandSide handSide, UxrInputButtons buttons, UxrButtonEventType buttonEventType, bool getIgnoredInput = false)
        {
            return buttonEventType switch
                   {
                               UxrButtonEventType.Touching  => GetButtonsTouch(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.TouchDown => GetButtonsTouchDown(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.TouchUp   => GetButtonsTouchUp(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.Pressing  => GetButtonsPress(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.PressDown => GetButtonsPressDown(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.PressUp   => GetButtonsPressUp(handSide, buttons, getIgnoredInput),
                               _                            => false
                   };
        }

        /// <inheritdoc />
        public bool GetButtonsEventAny(UxrHandSide handSide, UxrInputButtons buttons, UxrButtonEventType buttonEventType, bool getIgnoredInput = false)
        {
            return buttonEventType switch
                   {
                               UxrButtonEventType.Touching  => GetButtonsTouchAny(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.TouchDown => GetButtonsTouchDownAny(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.TouchUp   => GetButtonsTouchUpAny(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.Pressing  => GetButtonsPressAny(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.PressDown => GetButtonsPressDownAny(handSide, buttons, getIgnoredInput),
                               UxrButtonEventType.PressUp   => GetButtonsPressUpAny(handSide, buttons, getIgnoredInput),
                               _                            => false
                   };
        }

        /// <inheritdoc />
        public bool GetButtonsTouch(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            if (buttons == UxrInputButtons.Any)
            {
                return GetButtonTouchFlags(handSide, getIgnoredInput) != 0;
            }

            return GetButtonTouchFlags(handSide, getIgnoredInput).HasFlags((uint)buttons);
        }

        /// <inheritdoc />
        public bool GetButtonsTouchAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            return (GetButtonTouchFlags(handSide, getIgnoredInput) & (uint)buttons) != 0;
        }

        /// <inheritdoc />
        public bool GetButtonsTouchDown(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            if (buttons == UxrInputButtons.Any)
            {
                return (~GetButtonTouchFlagsLastFrame(handSide, getIgnoredInput) & GetButtonTouchFlags(handSide, getIgnoredInput)) != 0;
            }

            return (GetButtonTouchFlagsLastFrame(handSide, getIgnoredInput) & (uint)buttons) == 0 && (GetButtonTouchFlags(handSide, getIgnoredInput) & (uint)buttons) == (uint)buttons;
        }

        /// <inheritdoc />
        public bool GetButtonsTouchDownAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            uint touchFlagsLastFrame = GetButtonTouchFlagsLastFrame(handSide, getIgnoredInput);
            uint touchFlags          = GetButtonTouchFlags(handSide, getIgnoredInput);

            foreach (UxrInputButtons button in buttons.GetFlags())
            {
                uint buttonFlag = (uint)button;

                if ((touchFlagsLastFrame & buttonFlag) == 0 && (touchFlags & buttonFlag) == buttonFlag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public bool GetButtonsTouchUp(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            if (buttons == UxrInputButtons.Any)
            {
                return (GetButtonTouchFlagsLastFrame(handSide, getIgnoredInput) & ~GetButtonTouchFlags(handSide, getIgnoredInput)) != 0;
            }

            return (GetButtonTouchFlagsLastFrame(handSide, getIgnoredInput) & (uint)buttons) == (uint)buttons && (GetButtonTouchFlags(handSide, getIgnoredInput) & (uint)buttons) == 0;
        }

        /// <inheritdoc />
        public bool GetButtonsTouchUpAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            uint touchFlagsLastFrame = GetButtonTouchFlagsLastFrame(handSide, getIgnoredInput);
            uint touchFlags          = GetButtonTouchFlags(handSide, getIgnoredInput);

            foreach (UxrInputButtons button in buttons.GetFlags())
            {
                uint buttonFlag = (uint)button;

                if ((touchFlagsLastFrame & buttonFlag) == buttonFlag && (touchFlags & buttonFlag) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public bool GetButtonsPress(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            if (buttons == UxrInputButtons.Any)
            {
                return GetButtonPressFlags(handSide, getIgnoredInput) != 0;
            }

            return GetButtonPressFlags(handSide, getIgnoredInput).HasFlags((uint)buttons);
        }

        /// <inheritdoc />
        public bool GetButtonsPressAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            return (GetButtonPressFlags(handSide, getIgnoredInput) & (uint)buttons) != 0;
        }

        /// <inheritdoc />
        public bool GetButtonsPressDown(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            if (buttons == UxrInputButtons.Any)
            {
                return (~GetButtonPressFlagsLastFrame(handSide, getIgnoredInput) & GetButtonPressFlags(handSide, getIgnoredInput)) != 0;
            }

            return (GetButtonPressFlagsLastFrame(handSide, getIgnoredInput) & (uint)buttons) == 0 && (GetButtonPressFlags(handSide, getIgnoredInput) & (uint)buttons) == (uint)buttons;
        }

        /// <inheritdoc />
        public bool GetButtonsPressDownAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            uint pressFlagsLastFrame = GetButtonPressFlagsLastFrame(handSide, getIgnoredInput);
            uint pressFlags          = GetButtonPressFlags(handSide, getIgnoredInput);

            foreach (UxrInputButtons button in buttons.GetFlags())
            {
                uint buttonFlag = (uint)button;

                if ((pressFlagsLastFrame & buttonFlag) == 0 && (pressFlags & buttonFlag) == buttonFlag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public bool GetButtonsPressUp(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            if (buttons == UxrInputButtons.Any)
            {
                return (GetButtonPressFlagsLastFrame(handSide, getIgnoredInput) & ~GetButtonPressFlags(handSide, getIgnoredInput)) != 0;
            }

            return (GetButtonPressFlagsLastFrame(handSide, getIgnoredInput) & (uint)buttons) == (uint)buttons && (GetButtonPressFlags(handSide, getIgnoredInput) & (uint)buttons) == 0;
        }

        /// <inheritdoc />
        public bool GetButtonsPressUpAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false)
        {
            uint pressFlagsLastFrame = GetButtonPressFlagsLastFrame(handSide, getIgnoredInput);
            uint pressFlags          = GetButtonPressFlags(handSide, getIgnoredInput);

            foreach (UxrInputButtons button in buttons.GetFlags())
            {
                uint buttonFlag = (uint)button;

                if ((pressFlagsLastFrame & buttonFlag) == buttonFlag && (pressFlags & buttonFlag) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void SendHapticFeedback(UxrHandSide       handSide,
                                       UxrHapticClipType clipType,
                                       float             amplitude,
                                       float             durationSeconds = -1.0f,
                                       UxrHapticMode     hapticMode      = UxrHapticMode.Mix)
        {
            StartCoroutine(SendHapticFeedbackCoroutine(handSide, clipType, amplitude, durationSeconds, hapticMode));
        }

        /// <inheritdoc />
        public void SendGrabbableHapticFeedback(UxrGrabbableObject grabbableObject,
                                                UxrHapticClipType  clipType,
                                                float              amplitude       = DefaultHapticAmplitude,
                                                float              durationSeconds = -1.0f,
                                                UxrHapticMode      hapticMode      = UxrHapticMode.Mix)
        {
            if (UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, grabbableObject, UxrHandSide.Left))
            {
                SendHapticFeedback(UxrHandSide.Left, clipType, amplitude, durationSeconds, hapticMode);
            }

            if (UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, grabbableObject, UxrHandSide.Right))
            {
                SendHapticFeedback(UxrHandSide.Right, clipType, amplitude, durationSeconds, hapticMode);
            }
        }

        /// <inheritdoc />
        public void SendGrabbableHapticFeedback(UxrGrabbableObject grabbableObject, UxrHapticClip hapticClip)
        {
            if (UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, grabbableObject, UxrHandSide.Left))
            {
                SendHapticFeedback(UxrHandSide.Left, hapticClip);
            }

            if (UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, grabbableObject, UxrHandSide.Right))
            {
                SendHapticFeedback(UxrHandSide.Right, hapticClip);
            }
        }

        /// <inheritdoc />
        public UxrController3DModel GetController3DModel(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? _leftController : _rightController;
        }

        /// <inheritdoc />
        public IEnumerable<GameObject> GetControllerElementsGameObjects(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            UxrController3DModel controller3DModel = handSide == UxrHandSide.Left ? _leftController : _rightController;
            return controller3DModel != null ? controller3DModel.GetElements(controllerElements) : Enumerable.Empty<GameObject>();
        }

        /// <inheritdoc />
        public void StartControllerElementsBlinking(UxrHandSide           handSide,
                                                    UxrControllerElements controllerElements,
                                                    Color                 emissionColor,
                                                    float                 blinksPerSec    = 3.0f,
                                                    float                 durationSeconds = -1.0f)
        {
            UxrController3DModel controller3DModel = GetController3DModel(handSide);

            if (controller3DModel == null)
            {
                return;
            }

            controller3DModel.GetElements(controllerElements).ForEach(go => UxrObjectBlink.StartBlinking(go, emissionColor, blinksPerSec, durationSeconds));
        }

        /// <inheritdoc />
        public void StopControllerElementsBlinking(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            UxrController3DModel controller3DModel = GetController3DModel(handSide);

            if (controller3DModel)
            {
                controller3DModel.GetElements(controllerElements).ForEach(UxrObjectBlink.StopBlinking);
            }
        }

        /// <inheritdoc />
        public void StopAllBlinking(UxrHandSide handSide)
        {
            StopControllerElementsBlinking(handSide, UxrControllerElements.Everything);
        }

        /// <inheritdoc />
        public bool IsAnyControllerElementBlinking(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            UxrController3DModel controller3DModel = GetController3DModel(handSide);

            if (controller3DModel != null)
            {
                return controller3DModel.GetElements(controllerElements).Any(UxrObjectBlink.CheckBlinking);
            }

            return false;
        }

        /// <inheritdoc />
        public bool AreAllControllerElementsBlinking(UxrHandSide handSide, UxrControllerElements controllerElements)
        {
            UxrController3DModel controller3DModel = GetController3DModel(handSide);

            if (controller3DModel != null)
            {
                return controller3DModel.GetElements(controllerElements).All(UxrObjectBlink.CheckBlinking);
            }

            return false;
        }

        #endregion

        #region Implicit IUxrDevice

        /// <inheritdoc />
        public virtual string SDKDependency => null;

        /// <inheritdoc />
        public event EventHandler<UxrDeviceConnectEventArgs> DeviceConnected;

        #endregion

        #region Explicit IUxrControllerInputUpdater

        /// <summary>
        ///     This is the explicit implementation of <see cref="IUxrControllerInputUpdater.UpdateInput" />.
        ///     It is only accessible from the UXR framework because it's an explicit implementation,
        ///     so it can only be called when casting an object to this interface. Since this interface
        ///     is internal it can only be called from inside the UXR assembly.
        ///     API users will be able to implement their own input devices by inheriting from this
        ///     class and overriding <see cref="UpdateInput" />.
        /// </summary>
        void IUxrControllerInputUpdater.UpdateInput()
        {
            // Trigger Updating event
            OnUpdating();

            _buttonTouchFlagsLastFrameLeft = _buttonTouchFlagsLeft;
            _buttonPressFlagsLastFrameLeft = _buttonPressFlagsLeft;

            _buttonTouchFlagsLastFrameRight = _buttonTouchFlagsRight;
            _buttonPressFlagsLastFrameRight = _buttonPressFlagsRight;

            // Call the overridable UpdateInput()
            UpdateInput();

            // In devices where there is no handedness, UxrControllerInput.UpdateInput() should update the left button flags
            // and this method will take care of copying them to the right so that both hands return the same input.

            if (IsHandednessSupported == false)
            {
                _buttonTouchFlagsRight = _buttonTouchFlagsLeft;
                _buttonPressFlagsRight = _buttonPressFlagsLeft;
            }

            // Update controllers graphics and hands if necessary
            if (_leftController != null && _leftController.isActiveAndEnabled)
            {
                _leftController.UpdateFromInput(this);
            }

            if (_rightController != null && _rightController.isActiveAndEnabled)
            {
                _rightController.UpdateFromInput(this);
            }

            // Trigger input events (buttons, controllers1D/2D...)
            RaiseInputEvents();

            // Trigger Updated event
            OnUpdated();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets whether the given controller input should be ignored.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to check. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <returns>True if the given input should be ignored</returns>
        public static bool GetIgnoreControllerInput(UxrHandSide handSide)
        {
            return s_ignoreControllerInput[handSide];
        }

        /// <summary>
        ///     Sets whether the given controller input should be ignored.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to change. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="ignore">Boolean telling whether to ignore the given controller input</param>
        public static void SetIgnoreControllerInput(UxrHandSide handSide, bool ignore)
        {
            s_ignoreControllerInput[handSide] = ignore;
        }

        /// <summary>
        ///     Gets the controller button (<see cref="UxrInputButtons" />) enum value given a controller element (
        ///     <see cref="UxrControllerElements" />) enum value.
        /// </summary>
        /// <remarks>This method doesn't support using flag composition for <see cref="element" />, use only a single value</remarks>
        /// <param name="element">Controller element to get the button enum value for</param>
        /// <returns>
        ///     Button enum value representing the controller element, or <see cref="UxrInputButtons.None" /> if it doesn't exist
        /// </returns>
        public static UxrInputButtons ControllerElementToButton(UxrControllerElements element)
        {
            switch (element)
            {
                case UxrControllerElements.Joystick:       return UxrInputButtons.Joystick;
                case UxrControllerElements.Joystick2:      return UxrInputButtons.Joystick2;
                case UxrControllerElements.Trigger:        return UxrInputButtons.Trigger;
                case UxrControllerElements.Trigger2:       return UxrInputButtons.Trigger2;
                case UxrControllerElements.Grip:           return UxrInputButtons.Grip;
                case UxrControllerElements.ThumbCapSense:  return UxrInputButtons.ThumbCapSense;
                case UxrControllerElements.IndexCapSense:  return UxrInputButtons.IndexCapSense;
                case UxrControllerElements.MiddleCapSense: return UxrInputButtons.MiddleCapSense;
                case UxrControllerElements.RingCapSense:   return UxrInputButtons.RingCapSense;
                case UxrControllerElements.LittleCapSense: return UxrInputButtons.LittleCapSense;
                case UxrControllerElements.Button1:        return UxrInputButtons.Button1;
                case UxrControllerElements.Button2:        return UxrInputButtons.Button2;
                case UxrControllerElements.Button3:        return UxrInputButtons.Button3;
                case UxrControllerElements.Button4:        return UxrInputButtons.Button4;
                case UxrControllerElements.Bumper:         return UxrInputButtons.Bumper;
                case UxrControllerElements.Bumper2:        return UxrInputButtons.Bumper2;
                case UxrControllerElements.Back:           return UxrInputButtons.Back;
                case UxrControllerElements.Menu:           return UxrInputButtons.Menu;
            }

            return UxrInputButtons.None;
        }

        /// <summary>
        ///     Gets the controller element (<see cref="UxrControllerElements" />) enum value given a controller button (
        ///     <see cref="UxrInputButtons" />) enum value.
        /// </summary>
        /// <remarks>This method doesn't support using flag composition for <see cref="button" />, use only a single value</remarks>
        /// <param name="button">Controller button to get the element enum value for</param>
        /// <returns>
        ///     Controller element enum value representing the controller button, or <see cref="UxrControllerElements.None" /> if
        ///     it doesn't exist
        /// </returns>
        public static UxrControllerElements ButtonToControllerElement(UxrInputButtons button)
        {
            switch (button)
            {
                case UxrInputButtons.Joystick:       return UxrControllerElements.Joystick;
                case UxrInputButtons.JoystickLeft:   return UxrControllerElements.Joystick;
                case UxrInputButtons.JoystickRight:  return UxrControllerElements.Joystick;
                case UxrInputButtons.JoystickUp:     return UxrControllerElements.Joystick;
                case UxrInputButtons.JoystickDown:   return UxrControllerElements.Joystick;
                case UxrInputButtons.Joystick2:      return UxrControllerElements.Joystick2;
                case UxrInputButtons.Joystick2Left:  return UxrControllerElements.Joystick2;
                case UxrInputButtons.Joystick2Right: return UxrControllerElements.Joystick2;
                case UxrInputButtons.Joystick2Up:    return UxrControllerElements.Joystick2;
                case UxrInputButtons.Joystick2Down:  return UxrControllerElements.Joystick2;
                case UxrInputButtons.DPadLeft:       return UxrControllerElements.DPad;
                case UxrInputButtons.DPadRight:      return UxrControllerElements.DPad;
                case UxrInputButtons.DPadUp:         return UxrControllerElements.DPad;
                case UxrInputButtons.DPadDown:       return UxrControllerElements.DPad;
                case UxrInputButtons.Trigger:        return UxrControllerElements.Trigger;
                case UxrInputButtons.Trigger2:       return UxrControllerElements.Trigger2;
                case UxrInputButtons.Grip:           return UxrControllerElements.Grip;
                case UxrInputButtons.ThumbCapSense:  return UxrControllerElements.ThumbCapSense;
                case UxrInputButtons.IndexCapSense:  return UxrControllerElements.IndexCapSense;
                case UxrInputButtons.MiddleCapSense: return UxrControllerElements.MiddleCapSense;
                case UxrInputButtons.RingCapSense:   return UxrControllerElements.RingCapSense;
                case UxrInputButtons.LittleCapSense: return UxrControllerElements.LittleCapSense;
                case UxrInputButtons.Button1:        return UxrControllerElements.Button1;
                case UxrInputButtons.Button2:        return UxrControllerElements.Button2;
                case UxrInputButtons.Button3:        return UxrControllerElements.Button3;
                case UxrInputButtons.Button4:        return UxrControllerElements.Button4;
                case UxrInputButtons.Bumper:         return UxrControllerElements.Bumper;
                case UxrInputButtons.Bumper2:        return UxrControllerElements.Bumper2;
                case UxrInputButtons.Back:           return UxrControllerElements.Back;
                case UxrInputButtons.Menu:           return UxrControllerElements.Menu;
            }

            return UxrControllerElements.None;
        }

        /// <summary>
        ///     Gets the <see cref="UxrInput1D" /> enum value given a controller element (<see cref="UxrControllerElements" />
        ///     ) enum value.
        /// </summary>
        /// <remarks>This method doesn't support using flag composition for <see cref="element" />, use only a single value</remarks>
        /// <param name="element">Controller element to get the <see cref="UxrInput1D" /> enum value for</param>
        /// <returns>
        ///     <see cref="UxrInput1D" /> enum value representing the controller element, or <see cref="UxrInput1D.None" /> if it
        ///     doesn't exist
        /// </returns>
        public static UxrInput1D ControllerElementToInput1D(UxrControllerElements element)
        {
            switch (element)
            {
                case UxrControllerElements.Grip:     return UxrInput1D.Grip;
                case UxrControllerElements.Trigger:  return UxrInput1D.Trigger;
                case UxrControllerElements.Trigger2: return UxrInput1D.Trigger2;
            }

            return UxrInput1D.None;
        }

        /// <summary>
        ///     Gets the controller elements <see cref="UxrControllerElements" /> enum value given a <see cref="UxrInput1D" /> enum
        ///     value.
        /// </summary>
        /// <param name="input1D">1D input element to get the <see cref="UxrControllerElements" /> enum value for</param>
        /// <returns>
        ///     <see cref="UxrControllerElements" /> enum value representing the input1D, or
        ///     <see cref="UxrControllerElements.None" /> if it doesn't exist
        /// </returns>
        public static UxrControllerElements Input1DToControllerElement(UxrInput1D input1D)
        {
            switch (input1D)
            {
                case UxrInput1D.Grip:     return UxrControllerElements.Grip;
                case UxrInput1D.Trigger:  return UxrControllerElements.Trigger;
                case UxrInput1D.Trigger2: return UxrControllerElements.Trigger2;
            }

            return UxrControllerElements.None;
        }

        /// <summary>
        ///     Gets the <see cref="UxrInput2D" /> enum value given a controller element (<see cref="UxrControllerElements" />
        ///     ) enum value.
        /// </summary>
        /// <remarks>This method doesn't support using flag composition for <see cref="element" />, use only a single value</remarks>
        /// <param name="element">Controller element to get the <see cref="UxrInput2D" /> enum value for</param>
        /// <returns>
        ///     <see cref="UxrInput2D" /> enum value representing the controller element, or <see cref="UxrInput2D.None" /> if it
        ///     doesn't exist
        /// </returns>
        public static UxrInput2D ControllerElementToInput2D(UxrControllerElements element)
        {
            switch (element)
            {
                case UxrControllerElements.Joystick:  return UxrInput2D.Joystick;
                case UxrControllerElements.Joystick2: return UxrInput2D.Joystick2;
            }

            return UxrInput2D.None;
        }

        /// <summary>
        ///     Gets the controller elements <see cref="UxrControllerElements" /> enum value given a <see cref="UxrInput2D" /> enum
        ///     value.
        /// </summary>
        /// <param name="input2D">2D input element to get the <see cref="UxrControllerElements" /> enum value for</param>
        /// <returns>
        ///     <see cref="UxrControllerElements" /> enum value representing the input2D, or
        ///     <see cref="UxrControllerElements.None" /> if it doesn't exist
        /// </returns>
        public static UxrControllerElements Input2DToControllerElement(UxrInput2D input2D)
        {
            switch (input2D)
            {
                case UxrInput2D.Joystick:  return UxrControllerElements.Joystick;
                case UxrInput2D.Joystick2: return UxrControllerElements.Joystick2;
            }

            return UxrControllerElements.None;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Enables or disables the list of objects that should be enabled whenever the left controller is available in a dual
        ///     controller setup, and the avatar is being rendered.
        /// </summary>
        internal void EnableObjectListLeft(bool enable)
        {
            _enableObjectListLeft?.ForEach(go => go.SetActive(enable));
        }

        /// <summary>
        ///     Enables or disables the list of objects that should be enabled whenever the right controller is available in a dual
        ///     controller setup, and the avatar is being rendered.
        /// </summary>
        internal void EnableObjectListRight(bool enable)
        {
            _enableObjectListRight?.ForEach(go => go.SetActive(enable));
        }

        /// <summary>
        ///     Enables or disables the list of objects that should be enabled whenever the single controller is available in a
        ///     single controller setup, and the avatar is being rendered.
        /// </summary>
        internal void EnableObjectListSingle(bool enable)
        {
            _enableObjectList?.ForEach(go => go.SetActive(enable));
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes internal data
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (SetupType == UxrControllerSetupType.Single)
            {
                if (_controller != null && _controller.gameObject.IsPrefab())
                {
                    Debug.LogError($"The controller in the {GetType().Name} component needs to be instantiated in the avatar. It cannot be an asset.");
                }
            }
            else if (SetupType == UxrControllerSetupType.Dual)
            {
                if (_leftController != null && _leftController.gameObject.IsPrefab())
                {
                    Debug.LogError($"The left controller in the {GetType().Name} component needs to be instantiated in the avatar. It cannot be an asset.");
                }

                if (_rightController != null && _rightController.gameObject.IsPrefab())
                {
                    Debug.LogError($"The right controller in the {GetType().Name} component needs to be instantiated in the avatar. It cannot be an asset.");
                }
            }

            // Reset event dictionaries. We use these for sending only a single zero value event when a input1D/2D is not being pressed.

            foreach (UxrInput1D input1D in Enum.GetValues(typeof(UxrInput1D)))
            {
                if (HasControllerElements(UxrHandSide.Left, Input1DToControllerElement(input1D)) && _controllers1DResetLeft.ContainsKey(input1D) == false)
                {
                    _controllers1DResetLeft.Add(input1D, true);
                }

                if (HasControllerElements(UxrHandSide.Right, Input1DToControllerElement(input1D)) && _controllers1DResetRight.ContainsKey(input1D) == false)
                {
                    _controllers1DResetRight.Add(input1D, true);
                }
            }

            foreach (UxrInput2D input2D in Enum.GetValues(typeof(UxrInput2D)))
            {
                if (HasControllerElements(UxrHandSide.Left, Input2DToControllerElement(input2D)) && _controllers2DResetLeft.ContainsKey(input2D) == false)
                {
                    _controllers2DResetLeft.Add(input2D, true);
                }

                if (HasControllerElements(UxrHandSide.Right, Input2DToControllerElement(input2D)) && _controllers2DResetRight.ContainsKey(input2D) == false)
                {
                    _controllers2DResetRight.Add(input2D, true);
                }
            }
        }

        /// <summary>
        ///     Sets events to null in order to help remove unused references
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            DeviceConnected    = null;
            Updating           = null;
            Updated            = null;
            ButtonStateChanged = null;
            Input1DChanged     = null;
            Input2DChanged     = null;
            HapticRequesting   = null;
        }

        /// <summary>
        ///     Unity Start event
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (RaiseConnectOnStart)
            {
                OnDeviceConnected(new UxrDeviceConnectEventArgs(true));
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that sends a pre-defined haptic feedback clip emulating it using a composition of smaller steps
        ///     with varying frequency and amplitude.
        /// </summary>
        /// <param name="handSide">Target hand</param>
        /// <param name="clipType">Pre-defined clip to play on the controller to make it vibrate</param>
        /// <param name="amplitude">Amplitude [0.0, 1.0]</param>
        /// <param name="durationSeconds">Duration in seconds</param>
        /// <param name="hapticMode">Mix or replace</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator SendHapticFeedbackCoroutine(UxrHandSide       handSide,
                                                        UxrHapticClipType clipType,
                                                        float             amplitude,
                                                        float             durationSeconds,
                                                        UxrHapticMode     hapticMode = UxrHapticMode.Mix)
        {
            int steps = 10;

            switch (clipType)
            {
                case UxrHapticClipType.RumbleFreqVeryLow:
                    SendHapticFeedback(handSide, 10.0f, amplitude, durationSeconds <= 0.0f ? 0.5f : durationSeconds, hapticMode);
                    break;

                case UxrHapticClipType.RumbleFreqLow:
                    SendHapticFeedback(handSide, 25.0f, amplitude, durationSeconds <= 0.0f ? 0.5f : durationSeconds, hapticMode);
                    break;

                case UxrHapticClipType.RumbleFreqNormal:
                    SendHapticFeedback(handSide, 64.0f, amplitude, durationSeconds <= 0.0f ? 0.5f : durationSeconds, hapticMode);
                    break;

                case UxrHapticClipType.RumbleFreqHigh:
                    SendHapticFeedback(handSide, 160.0f, amplitude, durationSeconds <= 0.0f ? 0.5f : durationSeconds, hapticMode);
                    break;

                case UxrHapticClipType.RumbleFreqVeryHigh:
                    SendHapticFeedback(handSide, 320.0f, amplitude, durationSeconds <= 0.0f ? 0.5f : durationSeconds, hapticMode);
                    break;

                case UxrHapticClipType.Click:
                    SendHapticFeedback(handSide, 0.0f, amplitude, durationSeconds <= 0.0f ? 0.05f : durationSeconds, hapticMode);
                    break;

                case UxrHapticClipType.Shot:
                    durationSeconds = durationSeconds <= 0.0f ? 0.12f : durationSeconds;
                    SendHapticFeedback(handSide, 0.0f, amplitude, durationSeconds * 0.5f, hapticMode);
                    for (int i = 0; i < steps; ++i)
                    {
                        float t = (float)i / (steps - 1);

                        SendHapticFeedback(handSide, Mathf.Lerp(180.0f, 64.0f, t), amplitude, durationSeconds * 0.5f / steps, hapticMode);

                        yield return new WaitForSeconds(durationSeconds * 0.5f / steps);
                    }

                    break;

                case UxrHapticClipType.ShotBig:
                    durationSeconds = durationSeconds <= 0.0f ? 0.25f : durationSeconds;
                    for (int i = 0; i < steps; ++i)
                    {
                        float t = (float)i / (steps - 1);
                        SendHapticFeedback(handSide,
                                           Mathf.Lerp(320.0f, 32.0f, t),
                                           amplitude * Mathf.Clamp01(amplitude * (2.0f - t * 2.0f)),
                                           durationSeconds / steps,
                                           hapticMode);
                        yield return new WaitForSeconds(durationSeconds / steps);
                    }

                    break;

                case UxrHapticClipType.ShotBigger:
                    durationSeconds = durationSeconds <= 0.0f ? 0.4f : durationSeconds;
                    for (int i = 0; i < steps; ++i)
                    {
                        float t = (float)i / (steps - 1);
                        SendHapticFeedback(handSide,
                                           Mathf.Lerp(200.0f, 32.0f, t),
                                           amplitude * Mathf.Clamp01(amplitude * (2.0f - t * 2.0f)),
                                           durationSeconds / steps,
                                           hapticMode);
                        yield return new WaitForSeconds(durationSeconds / steps);
                    }

                    break;

                case UxrHapticClipType.Slide:
                    durationSeconds = durationSeconds <= 0.0f ? 0.5f : durationSeconds;
                    for (int i = 0; i < steps; ++i)
                    {
                        float t = (float)i / (steps - 1);

                        SendHapticFeedback(handSide, 160.0f, amplitude * (1.0f - t), durationSeconds / steps, hapticMode);

                        yield return new WaitForSeconds(durationSeconds / steps);
                    }

                    break;

                case UxrHapticClipType.Explosion:
                    durationSeconds = durationSeconds <= 0.0f ? 0.5f : durationSeconds;
                    for (int i = 0; i < steps; ++i)
                    {
                        float t = (float)i / (steps - 1);

                        SendHapticFeedback(handSide, Mathf.Lerp(180.0f, 32.0f, t), amplitude, durationSeconds / steps, hapticMode);

                        yield return new WaitForSeconds(durationSeconds / steps);
                    }

                    break;
            }

            yield return null;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="DeviceConnected" /> event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDeviceConnected(UxrDeviceConnectEventArgs e)
        {
            DeviceConnected?.Invoke(this, e);
            GlobalControllerConnected?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for the <see cref="Updating" /> event
        /// </summary>
        protected virtual void OnUpdating()
        {
            Updating?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Event trigger for the <see cref="Updated" /> event
        /// </summary>
        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Event trigger for the <see cref="ButtonStateChanged" /> event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnButtonStateChanged(UxrInputButtonEventArgs e)
        {
            ButtonStateChanged?.Invoke(this, e);
            GlobalButtonStateChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for the <see cref="Input1DChanged" /> event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnInput1DChanged(UxrInput1DEventArgs e)
        {
            Input1DChanged?.Invoke(this, e);
            GlobalInput1DChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for the <see cref="Input2DChanged" /> event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnInput2DChanged(UxrInput2DEventArgs e)
        {
            Input2DChanged?.Invoke(this, e);
            GlobalInput2DChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for the <see cref="HapticRequesting" /> event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnHapticRequesting(UxrControllerHapticEventArgs e)
        {
            HapticRequesting?.Invoke(this, e);
            GlobalHapticRequesting?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the necessary input events if there are changes in the current frame input state.
        /// </summary>
        private void RaiseInputEvents()
        {
            // We will generate events based on the input gathered. We use event trigger methods
            // for this internally, but we check first if there is any subscription to the event
            // in order to avoid iterating through all the elements unnecessarily
            if (ButtonStateChanged != null || GlobalButtonStateChanged != null)
            {
                // Button events
                foreach (UxrInputButtons button in Enum.GetValues(typeof(UxrInputButtons)))
                {
                    UxrControllerElements controllerElement = ButtonToControllerElement(button);

                    foreach (UxrHandSide handSide in Enum.GetValues(typeof(UxrHandSide)))
                    {
                        if (controllerElement != UxrControllerElements.None && HasControllerElements(handSide, controllerElement))
                        {
                            foreach (UxrButtonEventType eventType in Enum.GetValues(typeof(UxrButtonEventType)))
                            {
                                if (GetButtonsEvent(handSide, button, eventType))
                                {
                                    OnButtonStateChanged(new UxrInputButtonEventArgs(handSide, button, eventType));
                                }
                            }
                        }
                    }
                }
            }

            if (Input1DChanged != null || GlobalInput1DChanged != null)
            {
                // UxrInput1D events
                foreach (UxrInput1D input1D in Enum.GetValues(typeof(UxrInput1D)))
                {
                    UxrControllerElements controllerElement = Input1DToControllerElement(input1D);

                    if (controllerElement != UxrControllerElements.None)
                    {
                        if (HasControllerElements(UxrHandSide.Left, controllerElement))
                        {
                            float input1DValue = GetInput1D(UxrHandSide.Left, input1D);

                            if (input1DValue == 0.0f)
                            {
                                if (_controllers1DResetLeft[input1D] == false)
                                {
                                    _controllers1DResetLeft[input1D] = true;
                                }
                            }
                            else
                            {
                                _controllers1DResetLeft[input1D] = false;
                            }

                            OnInput1DChanged(new UxrInput1DEventArgs(UxrHandSide.Left, input1D, input1DValue));
                        }

                        if (HasControllerElements(UxrHandSide.Right, controllerElement))
                        {
                            float input1DValue = GetInput1D(UxrHandSide.Right, input1D);

                            if (input1DValue == 0.0f)
                            {
                                if (_controllers1DResetRight[input1D] == false)
                                {
                                    _controllers1DResetRight[input1D] = true;
                                }
                            }
                            else
                            {
                                _controllers1DResetRight[input1D] = false;
                            }

                            OnInput1DChanged(new UxrInput1DEventArgs(UxrHandSide.Right, input1D, input1DValue));
                        }
                    }
                }
            }

            if (Input2DChanged != null || GlobalInput2DChanged != null)
            {
                // UxrInput2D events
                foreach (UxrInput2D input2D in Enum.GetValues(typeof(UxrInput2D)))
                {
                    UxrControllerElements controllerElement = Input2DToControllerElement(input2D);

                    if (controllerElement != UxrControllerElements.None)
                    {
                        if (HasControllerElements(UxrHandSide.Left, controllerElement))
                        {
                            Vector2 input2DValue = GetInput2D(UxrHandSide.Left, input2D);

                            if (input2DValue == Vector2.zero)
                            {
                                if (_controllers2DResetLeft[input2D] == false)
                                {
                                    _controllers2DResetLeft[input2D] = true;
                                }
                            }
                            else
                            {
                                _controllers2DResetLeft[input2D] = false;
                            }

                            OnInput2DChanged(new UxrInput2DEventArgs(UxrHandSide.Left, input2D, input2DValue));
                        }

                        if (HasControllerElements(UxrHandSide.Right, controllerElement))
                        {
                            Vector2 input2DValue = GetInput2D(UxrHandSide.Right, input2D);

                            if (input2DValue == Vector2.zero)
                            {
                                if (_controllers2DResetRight[input2D] == false)
                                {
                                    _controllers2DResetRight[input2D] = true;
                                }
                            }
                            else
                            {
                                _controllers2DResetRight[input2D] = false;
                            }

                            OnInput2DChanged(new UxrInput2DEventArgs(UxrHandSide.Right, input2D, input2DValue));
                        }
                    }
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Filters a two-axis input using a dead-zone. Values inside the dead-zone will remain (0.0, 0.0).
        /// </summary>
        /// <param name="input2DPos">2-axis value</param>
        /// <param name="deadZone">Dead-zone threshold [0.0, 1.0]</param>
        /// <returns>Filtered input</returns>
        protected static Vector2 FilterTwoAxesDeadZone(Vector2 input2DPos, float deadZone)
        {
            Vector2 filtered2D = input2DPos;

            if (Mathf.Abs(filtered2D.x) < deadZone)
            {
                filtered2D.x = 0.0f;
            }

            if (Mathf.Abs(filtered2D.y) < deadZone)
            {
                filtered2D.y = 0.0f;
            }

            return filtered2D;
        }

        /// <summary>
        ///     Transforms a two-axis input to an angle. 0 degrees is right and degrees increase counterclockwise.
        /// </summary>
        /// <param name="input2D">2-axis input</param>
        /// <returns>Angle in degrees</returns>
        protected static float Input2DToAngle(Vector2 input2D)
        {
            float controllerAngle = Mathf.Atan2(input2D.y, input2D.x) * Mathf.Rad2Deg;

            if (controllerAngle < 0.0f)
            {
                controllerAngle += 360.0f;
            }

            return controllerAngle;
        }

        /// <summary>
        ///     Checks if the given 2-axis input corresponds to a left press in a digital pad.
        /// </summary>
        /// <param name="input2D">2-axis input</param>
        /// <returns>True if the input corresponds to a left press</returns>
        protected static bool IsInput2dDPadLeft(Vector2 input2D)
        {
            float touchPadAngle = Input2DToAngle(input2D);
            return IsInput2dDPadLeft(touchPadAngle);
        }

        /// <summary>
        ///     Checks if the given 2-axis input represented as an angle in degrees corresponds to a left press in a digital pad.
        ///     0 degrees is right and degrees increase counterclockwise.
        /// </summary>
        /// <param name="touchPadAngle">2-axis input in degrees</param>
        /// <returns>True if the input corresponds to a left press</returns>
        protected static bool IsInput2dDPadLeft(float touchPadAngle)
        {
            return touchPadAngle > 135.0f && touchPadAngle <= 225.0f;
        }

        /// <summary>
        ///     Checks if the given 2-axis input corresponds to a right press in a digital pad.
        /// </summary>
        /// <param name="input2D">2-axis input</param>
        /// <returns>True if the input corresponds to a right press</returns>
        protected static bool IsInput2dDPadRight(Vector2 input2D)
        {
            float touchPadAngle = Input2DToAngle(input2D);
            return IsInput2dDPadRight(touchPadAngle);
        }

        /// <summary>
        ///     Checks if the given 2-axis input represented as an angle in degrees corresponds to a right press in a digital pad.
        ///     0 degrees is right and degrees increase counterclockwise.
        /// </summary>
        /// <param name="touchPadAngle">2-axis input in degrees</param>
        /// <returns>True if the input corresponds to a right press</returns>
        protected static bool IsInput2dDPadRight(float touchPadAngle)
        {
            return (touchPadAngle > 315.0f && touchPadAngle <= 360.0f) || (touchPadAngle >= 0.0f && touchPadAngle <= 45.0f);
        }

        /// <summary>
        ///     Checks if the given 2-axis input corresponds to an up press in a digital pad.
        /// </summary>
        /// <param name="input2D">2-axis input</param>
        /// <returns>True if the input corresponds to an up press</returns>
        protected static bool IsInput2dDPadUp(Vector2 input2D)
        {
            float touchPadAngle = Input2DToAngle(input2D);
            return IsInput2dDPadUp(touchPadAngle);
        }

        /// <summary>
        ///     Checks if the given 2-axis input represented as an angle in degrees corresponds to an up press in a digital pad.
        ///     0 degrees is right and degrees increase counterclockwise.
        /// </summary>
        /// <param name="touchPadAngle">2-axis input in degrees</param>
        /// <returns>True if the input corresponds to an up press</returns>
        protected static bool IsInput2dDPadUp(float touchPadAngle)
        {
            return touchPadAngle > 45.0f && touchPadAngle <= 135.0f;
        }

        /// <summary>
        ///     Checks if the given 2-axis input corresponds to a down press in a digital pad.
        /// </summary>
        /// <param name="input2D">2-axis input</param>
        /// <returns>True if the input corresponds to a down press</returns>
        protected static bool IsInput2dDPadDown(Vector2 input2D)
        {
            float touchPadAngle = Input2DToAngle(input2D);
            return IsInput2dDPadUp(touchPadAngle);
        }

        /// <summary>
        ///     Checks if the given 2-axis input represented as an angle in degrees corresponds to a down press in a digital pad.
        ///     0 degrees is right and degrees increase counterclockwise.
        /// </summary>
        /// <param name="touchPadAngle">2-axis input in degrees</param>
        /// <returns>True if the input corresponds to a down press</returns>
        protected static bool IsInput2dDPadDown(float touchPadAngle)
        {
            return touchPadAngle > 225.0f && touchPadAngle <= 315.0f;
        }

        /// <summary>
        ///     Virtual method that should be overriden in child classes in order to
        ///     update the current input state information (buttons and all the other elements in the controllers).
        /// </summary>
        protected virtual void UpdateInput()
        {
        }

        /// <summary>
        ///     Checks whether the given hand input should be ignored.
        /// </summary>
        /// <param name="handSide">Which hand</param>
        /// <param name="getIgnoredInput">If a hand input should be ignored, whether to get it anyway</param>
        /// <returns>Whether the given hand input should be ignored</returns>
        protected bool ShouldIgnoreInput(UxrHandSide handSide, bool getIgnoredInput)
        {
            return GetIgnoreControllerInput(handSide) && !getIgnoredInput;
        }

        /// <summary>
        ///     Gets flags representing the current button state
        /// </summary>
        /// <param name="buttonFlags">Which button flags to retrieve</param>
        /// <returns>Button flags</returns>
        protected uint GetButtonFlags(ButtonFlags buttonFlags)
        {
            switch (buttonFlags)
            {
                case ButtonFlags.TouchFlagsLeft:  return _buttonTouchFlagsLeft;
                case ButtonFlags.PressFlagsLeft:  return _buttonPressFlagsLeft;
                case ButtonFlags.TouchFlagsRight: return _buttonTouchFlagsRight;
                case ButtonFlags.PressFlagsRight: return _buttonPressFlagsRight;
            }

            return 0;
        }

        /// <summary>
        ///     Sets the given button flags
        /// </summary>
        /// <param name="buttonFlags">Which button flags to set</param>
        /// <param name="buttons">Which button(s) to set</param>
        /// <param name="set">True or false representing the flag state</param>
        protected void SetButtonFlags(ButtonFlags buttonFlags, UxrInputButtons buttons, bool set)
        {
            if (set)
            {
                switch (buttonFlags)
                {
                    case ButtonFlags.TouchFlagsLeft:
                        SetButtonFlags(ref _buttonTouchFlagsLeft, buttons);
                        break;

                    case ButtonFlags.PressFlagsLeft:
                        SetButtonFlags(ref _buttonPressFlagsLeft, buttons);
                        break;

                    case ButtonFlags.TouchFlagsRight:
                        SetButtonFlags(ref _buttonTouchFlagsRight, buttons);
                        break;

                    case ButtonFlags.PressFlagsRight:
                        SetButtonFlags(ref _buttonPressFlagsRight, buttons);
                        break;
                }
            }
            else
            {
                switch (buttonFlags)
                {
                    case ButtonFlags.TouchFlagsLeft:
                        ClearButtonFlags(ref _buttonTouchFlagsLeft, buttons);
                        break;

                    case ButtonFlags.PressFlagsLeft:
                        ClearButtonFlags(ref _buttonPressFlagsLeft, buttons);
                        break;

                    case ButtonFlags.TouchFlagsRight:
                        ClearButtonFlags(ref _buttonTouchFlagsRight, buttons);
                        break;

                    case ButtonFlags.PressFlagsRight:
                        ClearButtonFlags(ref _buttonPressFlagsRight, buttons);
                        break;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sets the given button flags
        /// </summary>
        /// <param name="flags">Which button flags to set</param>
        /// <param name="buttons">Which button(s) to set</param>
        private void SetButtonFlags(ref uint flags, UxrInputButtons buttons)
        {
            flags = flags.WithFlags((uint)buttons);
        }

        /// <summary>
        ///     Clears (sets to 0) the given button flags
        /// </summary>
        /// <param name="flags">Which button flags to clear</param>
        /// <param name="buttons">Which button(s) to clear</param>
        private void ClearButtonFlags(ref uint flags, UxrInputButtons buttons)
        {
            flags = flags.WithoutFlags((uint)buttons);
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Used by child classes to notify that the Connect event should be forcefully raised during Start().
        ///     This is required to propagate Connect events properly when a new scene is loaded and the devices are already
        ///     connected and thus not sending any events. We still need to get Connect notifications, so child classes
        ///     need to detect during Awake() if the device(s) are already connected and if so, notify that the
        ///     Connect event needs to be raised.
        /// </summary>
        protected bool RaiseConnectOnStart { get; set; }

        /// <summary>
        ///     Minimum axis value required to consider an analog input as a DPad digital press in any direction.
        /// </summary>
        protected const float AnalogAsDPadThreshold = 0.2f;

        /// <summary>
        ///     Default haptic amplitude if not specified
        /// </summary>
        protected const float DefaultHapticAmplitude = 0.6f;

        #endregion

        #region Private Types & Data

        private static readonly Dictionary<UxrHandSide, bool> s_ignoreControllerInput = new Dictionary<UxrHandSide, bool>
                                                                                        {
                                                                                                    { UxrHandSide.Left, false },
                                                                                                    { UxrHandSide.Right, false }
                                                                                        };

        private readonly Dictionary<UxrInput1D, bool> _controllers1DResetLeft  = new Dictionary<UxrInput1D, bool>();
        private readonly Dictionary<UxrInput1D, bool> _controllers1DResetRight = new Dictionary<UxrInput1D, bool>();
        private readonly Dictionary<UxrInput2D, bool> _controllers2DResetLeft  = new Dictionary<UxrInput2D, bool>();
        private readonly Dictionary<UxrInput2D, bool> _controllers2DResetRight = new Dictionary<UxrInput2D, bool>();

        private uint _buttonTouchFlagsLastFrameLeft;
        private uint _buttonPressFlagsLastFrameLeft;
        private uint _buttonTouchFlagsLastFrameRight;
        private uint _buttonPressFlagsLastFrameRight;

        private uint _buttonTouchFlagsLeft;
        private uint _buttonPressFlagsLeft;
        private uint _buttonTouchFlagsRight;
        private uint _buttonPressFlagsRight;

        #endregion
    }
}