// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrControllerInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices.Visualization;
using UltimateXR.Haptics;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Controller interface for all XR input devices, supporting single controller and dual controller setups.
    /// </summary>
    public interface IUxrControllerInput : IUxrDevice
    {
        #region Public Types & Data

        /// <summary>
        ///     Event called right before the controller input state is updated.
        /// </summary>
        event EventHandler Updating;

        /// <summary>
        ///     Event called right after the controller input state has been updated.
        /// </summary>
        event EventHandler Updated;

        /// <summary>
        ///     Event called after a controller button state changed.
        /// </summary>
        event EventHandler<UxrInputButtonEventArgs> ButtonStateChanged;

        /// <summary>
        ///     Event called after a <see cref="UxrInput1D" /> element changed.
        /// </summary>
        event EventHandler<UxrInput1DEventArgs> Input1DChanged;

        /// <summary>
        ///     Event called after a <see cref="UxrInput2D" /> element changed.
        /// </summary>
        event EventHandler<UxrInput2DEventArgs> Input2DChanged;

        /// <summary>
        ///     Event called right before haptic feedback was requested.
        /// </summary>
        event EventHandler<UxrControllerHapticEventArgs> HapticRequesting;

        /// <summary>
        ///     Gets the setup type. See <see cref="UxrControllerSetupType" />.
        /// </summary>
        UxrControllerSetupType SetupType { get; }

        /// <summary>
        ///     <para>
        ///         Gets whether <see cref="Handedness" /> can be used. In <see cref="UxrControllerSetupType.Single" /> devices, it
        ///         may be used to control which hand is holding the controller. In <see cref="UxrControllerSetupType.Dual" />
        ///         devices it is used to determine which hands have the <see cref="Primary" /> (dominant) and
        ///         <see cref="Secondary" /> (non-dominant) roles.
        ///     </para>
        ///     Devices such as gamepads don't support handedness and will target the single device no matter which
        ///     <see cref="UxrHandSide" /> is used. In this case it is good practice to use <see cref="Primary" /> to target the
        ///     device in order to make the code cleaner.
        /// </summary>
        bool IsHandednessSupported { get; }

        /// <summary>
        ///     <para>
        ///         Gets which hand is holding the controller in <see cref="UxrControllerSetupType.Single" /> setups where
        ///         <see cref="IsHandednessSupported" /> is available. In <see cref="UxrControllerSetupType.Dual" /> setups
        ///         it identifies the dominant hand. In both cases, <see cref="Handedness" /> determines which hand it is.
        ///     </para>
        ///     In <see cref="UxrControllerSetupType.Single" /> devices where handedness is not applicable (
        ///     <see cref="IsHandednessSupported" /> is false) it is good practice to use <see cref="Primary" /> to address the
        ///     device, even if both left and right can too.
        /// </summary>
        /// <seealso cref="Handedness" />
        UxrHandSide Primary { get; }

        /// <summary>
        ///     Gets which hand is not holding the controller in <see cref="UxrControllerSetupType.Single" /> setups where
        ///     <see cref="IsHandednessSupported" /> is available. In <see cref="UxrControllerSetupType.Dual" /> setups
        ///     it identifies the non-dominant hand.
        /// </summary>
        /// <seealso cref="Handedness" />
        UxrHandSide Secondary { get; }

        /// <summary>
        ///     Gets the left controller name, or empty if not connected / doesn't exist. In
        ///     <see cref="UxrControllerSetupType.Single" /> configurations where <see cref="IsHandednessSupported" /> is not
        ///     available, both sides will return the same name.
        /// </summary>
        string LeftControllerName { get; }

        /// <summary>
        ///     Gets the right controller name, or empty if not connected / doesn't exist. In
        ///     <see cref="UxrControllerSetupType.Single" /> configurations where <see cref="IsHandednessSupported" /> is not
        ///     available, both sides will return the same name.
        /// </summary>
        string RightControllerName { get; }

        /// <summary>
        ///     Gets the left instanced 3D controller model, if available. In <see cref="UxrControllerSetupType.Single" />
        ///     configurations where <see cref="IsHandednessSupported" /> is false, both sides will return the same model.
        /// </summary>
        UxrController3DModel LeftController3DModel { get; }

        /// <summary>
        ///     Gets the right instanced 3D controller model, if available. In <see cref="UxrControllerSetupType.Single" />
        ///     configurations where <see cref="IsHandednessSupported" /> is false, both sides will return the same model.
        /// </summary>
        UxrController3DModel RightController3DModel { get; }

        /// <summary>
        ///     Gets a value indicating whether the main two-axis input element is a touchpad. If false, it usually means the main
        ///     joystick is a thumbstick.
        /// </summary>
        bool MainJoystickIsTouchpad { get; }

        /// <summary>
        ///     Gets the controller's joystick dead zone [0.0, 1.0]. Some controllers may have a more sensitive joystick,
        ///     and this property can be used to compensate in different implementations.
        /// </summary>
        float JoystickDeadZone { get; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the handedness, which is the <see cref="Primary" /> -dominant- hand in
        ///         <see cref="UxrControllerSetupType.Dual" /> controller setups. In <see cref="UxrControllerSetupType.Single" />
        ///         controller setups where the controller is grabbed with one hand, it determines which hand is being used.
        ///     </para>
        ///     If <see cref="IsHandednessSupported" /> false, such as in gamepads, the handedness value should be ignored.
        /// </summary>
        /// <seealso cref="IsHandednessSupported" />
        /// <seealso cref="Primary" />
        /// <seealso cref="Secondary" />
        UxrHandSide Handedness { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the given controller is enabled.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to check. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <returns>Whether the given controller is enabled</returns>
        bool IsControllerEnabled(UxrHandSide handSide);

        /// <summary>
        ///     Checks if the given controller has specific elements.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to check. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="controllerElements">Flags indicating the element(s) to look for</param>
        /// <returns>True if the controller has all the elements specified. If one is missing, it will return false</returns>
        bool HasControllerElements(UxrHandSide handSide, UxrControllerElements controllerElements);

        /// <summary>
        ///     Gets the capabilities of the XR controller.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to check. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <returns>Device capabilities flags</returns>
        UxrControllerInputCapabilities GetControllerCapabilities(UxrHandSide handSide);

        /// <summary>
        ///     Gets whether the given controller input should be ignored.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to check. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <returns>True if the given input should be ignored</returns>
        bool GetIgnoreControllerInput(UxrHandSide handSide);

        /// <summary>
        ///     Sets whether the given controller input should be ignored.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to change. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="ignore">Boolean telling whether to ignore the given controller input</param>
        void SetIgnoreControllerInput(UxrHandSide handSide, bool ignore);

        /// <summary>
        ///     Gets the state of an analog controller input element.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="input1D">Element to get the input from</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>Input value [0.0, 1.0]</returns>
        float GetInput1D(UxrHandSide handSide, UxrInput1D input1D, bool getIgnoredInput = false);

        /// <summary>
        ///     Gets the state of a 2D input element (joystick, touchpad...).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="input2D">Element to get the input from</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     Vector2 telling the state of the controller element. Each component between [-1.0, 1.0]
        /// </returns>
        Vector2 GetInput2D(UxrHandSide handSide, UxrInput2D input2D, bool getIgnoredInput = false);

        /// <summary>
        ///     Gets an uint value representing touch states for each the controller <see cref="UxrInputButtons" /> flags in the
        ///     current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>Button flags representing touch states for each controller button in the current frame</returns>
        /// <seealso cref="UxrInputButtons" />
        uint GetButtonTouchFlags(UxrHandSide handSide, bool getIgnoredInput = false);

        /// <summary>
        ///     Gets an uint value representing touch states for each the controller <see cref="UxrInputButtons" /> flags in the
        ///     last frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>Button flags representing touch states for each controller button in the last frame</returns>
        /// <seealso cref="UxrInputButtons" />
        uint GetButtonTouchFlagsLastFrame(UxrHandSide handSide, bool getIgnoredInput = false);

        /// <summary>
        ///     Gets an uint value representing press states for each the controller <see cref="UxrInputButtons" /> flags in the
        ///     current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>Button flags representing press states for each controller button in the current frame</returns>
        /// <seealso cref="UxrInputButtons" />
        uint GetButtonPressFlags(UxrHandSide handSide, bool getIgnoredInput = false);

        /// <summary>
        ///     Gets an uint value representing press states for each the in the last frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>Button flags representing press states for each controller button in the last frame</returns>
        /// <seealso cref="UxrInputButtons" />
        uint GetButtonPressFlagsLastFrame(UxrHandSide handSide, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if a given input event took place for a button or all buttons in a set in the current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">
        ///     Button (or buttons by flag composition) to check. If it's a combination, all buttons require to
        ///     meet the event criteria
        /// </param>
        /// <param name="buttonEventType">Input event type to check for</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given event happened during the current frame for the specified button. If more than one button
        ///     was specified by using flags it will return true only if the input event happened for all the given buttons.
        /// </returns>
        bool GetButtonsEvent(UxrHandSide handSide, UxrInputButtons buttons, UxrButtonEventType buttonEventType, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if a given input event took place for a button or any button in a set in the current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">
        ///     Button (or buttons by flag composition) to check. If it's a combination, any button that meets
        ///     the event criteria will be enough
        /// </param>
        /// <param name="buttonEventType">Input event type to check for</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given event happened during the current frame for the specified button. If more than one button
        ///     was specified by using flags it will return true as long as any button had the event.
        /// </returns>
        bool GetButtonsEventAny(UxrHandSide handSide, UxrInputButtons buttons, UxrButtonEventType buttonEventType, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or all buttons in a set are being touched in the current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being touched in the current frame. If more than one button was specified by using
        ///     flags it will return true only if all are being touched.
        /// </returns>
        bool GetButtonsTouch(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or any button in a set is being touched in the current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being touched in the current frame. If more than one button was specified by using
        ///     flags it will return true if any button in the set is being touched.
        /// </returns>
        bool GetButtonsTouchAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or buttons are being touched in the current frame but weren't the previous frame
        ///     (touch-down).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is just started being touched in the current frame. If more than one button was specified
        ///     by using flags it will return true only if all meet the condition.
        /// </returns>
        bool GetButtonsTouchDown(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or any button in a set is being touched in the current frame but not in the previous
        ///     frame (touch-down).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is just started being touched in the current frame. If more than one button was specified
        ///     by using flags it will return true if any meets the condition.
        /// </returns>
        bool GetButtonsTouchDownAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or buttons aren't being touched in the current frame but were during the previous frame
        ///     (release touch).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being released from touching in the current frame. If more than one button was
        ///     specified by using flags it will return true only if all meet the condition.
        /// </returns>
        bool GetButtonsTouchUp(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or any button in a set isn't being touched in the current frame but was during the
        ///     previous frame (release touch).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being released from touching in the current frame. If more than one button was
        ///     specified by using flags it will return true as long as any meets the condition.
        /// </returns>
        bool GetButtonsTouchUpAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or buttons are being pressed in the current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being pressed in the current frame. If more than one button was specified by using
        ///     flags it will return true only if all are being pressed.
        /// </returns>
        bool GetButtonsPress(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or any button in a set is being pressed in the current frame.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being pressed in the current frame. If more than one button was specified by using
        ///     flags it will return true as long as any is being pressed.
        /// </returns>
        bool GetButtonsPressAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or buttons are being pressed in the current frame but weren't the previous frame
        ///     (press-down).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is just started being pressed in the current frame. If more than one button was specified
        ///     by using flags it will return true only if all meet the condition.
        /// </returns>
        bool GetButtonsPressDown(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or any button in a set is being pressed in the current frame but wasn't the previous
        ///     frame (press-down).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is just started being pressed in the current frame. If more than one button was specified
        ///     by using flags it will return true only if any meets the condition.
        /// </returns>
        bool GetButtonsPressDownAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or buttons aren't being pressed in the current frame but were during the previous frame
        ///     (release press).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being released from pressing in the current frame. If more than one button was
        ///     specified by using flags it will return true only if all meet the condition.
        /// </returns>
        bool GetButtonsPressUp(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Checks if the given button or any button in a set isn't being pressed in the current frame but was during the
        ///     previous frame (release press).
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get input from. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="buttons">Button (or buttons by flag composition) to check</param>
        /// <param name="getIgnoredInput">Whether to return ignored input by <see cref="SetIgnoreControllerInput" /></param>
        /// <returns>
        ///     True if the given button is being released from pressing in the current frame. If more than one button was
        ///     specified by using flags it will return true if any meets the condition.
        /// </returns>
        bool GetButtonsPressUpAny(UxrHandSide handSide, UxrInputButtons buttons, bool getIgnoredInput = false);

        /// <summary>
        ///     Sends haptic feedback to a controller if the controller supports it.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to send the haptic feedback to. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="hapticClip">Clip to send</param>
        void SendHapticFeedback(UxrHandSide handSide, UxrHapticClip hapticClip);

        /// <summary>
        ///     Sends haptic feedback to a controller if the controller supports it.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to send the haptic feedback to. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="frequency">Frequency of the feedback in hz</param>
        /// <param name="amplitude">Amplitude of the feedback between range [0.0, 1.0]</param>
        /// <param name="durationSeconds">Feedback duration in seconds</param>
        /// <param name="hapticMode">The mode (stop and override all current haptics or mix it with the current existing haptics)</param>
        void SendHapticFeedback(UxrHandSide   handSide,
                                float         frequency,
                                float         amplitude,
                                float         durationSeconds,
                                UxrHapticMode hapticMode = UxrHapticMode.Mix);

        /// <summary>
        ///     Sends a predefined haptic clip to a controller.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to send the haptic feedback to. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="clipType">The clip type from a set of predefined clips</param>
        /// <param name="amplitude">The intensity of the haptic feedback</param>
        /// <param name="durationSeconds">The duration in seconds. A zero/negative value will use a default duration.</param>
        /// <param name="hapticMode">Whether the clip will stop all currently playing haptics or mix with them</param>
        public void SendHapticFeedback(UxrHandSide       handSide,
                                       UxrHapticClipType clipType,
                                       float             amplitude,
                                       float             durationSeconds = -1.0f,
                                       UxrHapticMode     hapticMode      = UxrHapticMode.Mix);

        /// <summary>
        ///     Sends haptic feedback to XR controllers that are being used to manipulate a grabbable object.
        ///     Each hand associated to an XR controller that is grabbing the object will receive haptic feedback.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="clipType">Clip type to send</param>
        /// <param name="amplitude">Intensity of the haptic feedback</param>
        /// <param name="durationSeconds">Duration in seconds</param>
        /// <param name="hapticMode">Override current haptic feedback or mix it?</param>
        public void SendGrabbableHapticFeedback(UxrGrabbableObject grabbableObject,
                                                UxrHapticClipType  clipType,
                                                float              amplitude,
                                                float              durationSeconds = -1.0f,
                                                UxrHapticMode      hapticMode      = UxrHapticMode.Mix);

        /// <summary>
        ///     Sends haptic feedback to XR controllers that are being used to manipulate a grabbable object.
        ///     Each hand associated to an XR controller that is grabbing the object will receive haptic feedback.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="hapticClip">Haptic clip to send</param>
        public void SendGrabbableHapticFeedback(UxrGrabbableObject grabbableObject, UxrHapticClip hapticClip);

        /// <summary>
        ///     Stops all current haptics in a given controller.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to stop sending haptic feedback to. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        void StopHapticFeedback(UxrHandSide handSide);

        /// <summary>
        ///     Gets the instanced controller 3D model for a given hand.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get the 3D model of. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        UxrController3DModel GetController3DModel(UxrHandSide handSide);

        /// <summary>
        ///     Returns a list of GameObjects that represent parts of the instantiated controller. This can be useful
        ///     to highlight buttons or other elements during tutorials.
        ///     Functionality to make these elements blink is also provided by the framework.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller to get the elements of. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="controllerElements">Element (or elements using flags) to retrieve the GameObject(s) for</param>
        /// <returns>A list of GameObject, one for each element requested</returns>
        /// <seealso cref="StartControllerElementsBlinking" />
        /// <seealso cref="StopControllerElementsBlinking" />
        /// <seealso cref="StopAllBlinking" />
        /// <seealso cref="IsAnyControllerElementBlinking" />
        /// <seealso cref="AreAllControllerElementsBlinking" />
        IEnumerable<GameObject> GetControllerElementsGameObjects(UxrHandSide handSide, UxrControllerElements controllerElements);

        /// <summary>
        ///     Starts blinking one or more elements in a controller. This can be useful during tutorials to highlight which
        ///     button(s) to press.
        /// </summary>
        /// <param name="handSide">
        ///     Which controller. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="controllerElements">Element (or elements using flags) that should blink</param>
        /// <param name="emissionColor">
        ///     Emission color use to blink. Usually it can be set to white with a mid alpha value to avoid
        ///     too much brightness
        /// </param>
        /// <param name="blinksPerSec">Blinks per second</param>
        /// <param name="durationSeconds">Duration in seconds that it should blink</param>
        /// <seealso cref="GetControllerElementsGameObjects" />
        /// <seealso cref="StopControllerElementsBlinking" />
        /// <seealso cref="StopAllBlinking" />
        /// <seealso cref="IsAnyControllerElementBlinking" />
        /// <seealso cref="AreAllControllerElementsBlinking" />
        void StartControllerElementsBlinking(UxrHandSide           handSide,
                                             UxrControllerElements controllerElements,
                                             Color                 emissionColor,
                                             float                 blinksPerSec    = 3.0f,
                                             float                 durationSeconds = -1.0f);

        /// <summary>
        ///     Stops controller elements to blink
        /// </summary>
        /// <param name="handSide">
        ///     Which controller. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="controllerElements">Element (or elements using flags) that should stop blinking</param>
        /// <seealso cref="GetControllerElementsGameObjects" />
        /// <seealso cref="StartControllerElementsBlinking" />
        /// <seealso cref="StopAllBlinking" />
        /// <seealso cref="IsAnyControllerElementBlinking" />
        /// <seealso cref="AreAllControllerElementsBlinking" />
        void StopControllerElementsBlinking(UxrHandSide handSide, UxrControllerElements controllerElements);

        /// <summary>
        ///     Stops all controller elements to blink
        /// </summary>
        /// <param name="handSide">
        ///     Which controller. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <seealso cref="GetControllerElementsGameObjects" />
        /// <seealso cref="StartControllerElementsBlinking" />
        /// <seealso cref="StopControllerElementsBlinking" />
        /// <seealso cref="IsAnyControllerElementBlinking" />
        /// <seealso cref="AreAllControllerElementsBlinking" />
        void StopAllBlinking(UxrHandSide handSide);

        /// <summary>
        ///     Checks if any specific controller element is currently blinking
        /// </summary>
        /// <param name="handSide">
        ///     Which controller. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="controllerElements">Element (or elements using flags) that should be checked</param>
        /// <returns>True if any of the given elements is blinking</returns>
        /// <seealso cref="GetControllerElementsGameObjects" />
        /// <seealso cref="StartControllerElementsBlinking" />
        /// <seealso cref="StopControllerElementsBlinking" />
        /// <seealso cref="StopAllBlinking" />
        /// <seealso cref="AreAllControllerElementsBlinking" />
        bool IsAnyControllerElementBlinking(UxrHandSide handSide, UxrControllerElements controllerElements);

        /// <summary>
        ///     Checks if all elements of a specific controller element are currently blinking
        /// </summary>
        /// <param name="handSide">
        ///     Which controller. In <see cref="UxrControllerSetupType.Single" /> devices where
        ///     <see cref="IsHandednessSupported" /> is false, such as in gamepads, both hands will address the single device.
        /// </param>
        /// <param name="controllerElements">Element (or elements using flags) that should be checked</param>
        /// <returns>True if all of the given elements are blinking</returns>
        /// <seealso cref="GetControllerElementsGameObjects" />
        /// <seealso cref="StartControllerElementsBlinking" />
        /// <seealso cref="StopControllerElementsBlinking" />
        /// <seealso cref="StopAllBlinking" />
        /// <seealso cref="IsAnyControllerElementBlinking" />
        bool AreAllControllerElementsBlinking(UxrHandSide handSide, UxrControllerElements controllerElements);

        #endregion
    }
}