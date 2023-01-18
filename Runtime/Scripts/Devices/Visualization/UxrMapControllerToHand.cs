// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMapControllerToHand.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    /// <summary>
    ///     UltimateXR has support for rendering avatars in 'controllers mode' (where the currently
    ///     active controllers are rendered with optional hands on top) or full Avatar mode, where the actual
    ///     visual representation of the user is rendered.
    ///     'Controllers mode' can be useful during tutorials or menus while the full Avatar mode will be used
    ///     mainly for the core game/simulation.
    ///     When we developed the big example scene we included a small gallery with all supported controllers
    ///     so the user could grab and inspect them. But there was no link between the user input and how the
    ///     hands and the controllers behaved (like the 'controllers mode').
    ///     Since one of the coolest UltimateXR features is mapping user input to controllers and having IK drive
    ///     the hands we also wanted to link the grabbing mechanics to this. In short, what we did was to add
    ///     UxrControllerHand components to the regular avatar hands as well, and then at runtime link them
    ///     to the currently grabbed controller and feed the avatar input. This way we are now able to not
    ///     just grab to controller, but also see how the hand and the controller respond to the user input.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public class UxrMapControllerToHand : UxrGrabbableObjectComponent<UxrMapControllerToHand>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _ambidextrous;

        #endregion

        #region Unity

        /// <summary>
        ///     Cache components.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _controller3DModel = GetComponentInChildren<UxrController3DModel>();
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Initializes the hand:
        ///     <list type="bullet"></list>
        ///     <item>
        ///         Block input mainly to prevent locomotion while pressing the controllers. We want the user to be able to move
        ///         the joysticks and press buttons without triggering unwanted events.
        ///     </item>
        ///     <item>
        ///         Use the current hand pose, which is used to grab the controller, to initialize the
        ///         <see cref="UxrControllerHand" /> component.
        ///     </item>
        /// </summary>
        /// <param name="grabber">Grabber that was used to grab the controller</param>
        /// <param name="controllerHand">The controller hand component</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator InitializeHandCoroutine(UxrGrabber grabber, UxrControllerHand controllerHand)
        {
            yield return null;

            controllerHand.InitializeFromCurrentHandPose(grabber.Avatar, grabber.Side);

            if (grabber.Side == UxrHandSide.Left)
            {
                _ignoreInputLeft = UxrControllerInput.GetIgnoreControllerInput(UxrHandSide.Left);
                UxrControllerInput.SetIgnoreControllerInput(UxrHandSide.Left, true);
            }
            else
            {
                _ignoreInputRight = UxrControllerInput.GetIgnoreControllerInput(UxrHandSide.Right);
                UxrControllerInput.SetIgnoreControllerInput(UxrHandSide.Right, true);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     This callback will be executed at the end of all the UltimateXR frame pipeline when the user is grabbing the
        ///     controller. We feed both the controller and the hand's IK the user input.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            _controller3DModel.UpdateFromInput(UxrAvatar.LocalAvatarInput);

            if (_controller3DModel.ControllerHand)
            {
                _controller3DModel.ControllerHand.UpdateIKManually();
            }

            if (_controller3DModel.ControllerHandLeft)
            {
                _controller3DModel.ControllerHandLeft.UpdateIKManually();
            }

            if (_controller3DModel.ControllerHandRight)
            {
                _controller3DModel.ControllerHandRight.UpdateIKManually();
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when the user grabs the controller. We bind the controller to the
        ///     hand that grabbed it and subscribe to events.
        /// </summary>
        /// <param name="e">Grab parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;

            UxrControllerHand controllerHand = GetControllerHand(e.Grabber);

            if (controllerHand)
            {
                if (_controller3DModel.NeedsBothHands)
                {
                    // Controller can be grabbed with both hands simultaneously

                    if (e.Grabber.Side == UxrHandSide.Left)
                    {
                        _controller3DModel.ControllerHandLeft = controllerHand;
                    }
                    else
                    {
                        _controller3DModel.ControllerHandRight = controllerHand;
                    }
                }
                else
                {
                    // Some controllers are grabbed with one hand only but can be grabbed with both. Switch the component handedness
                    // if necessary to adapt to the hand that is grabbing it

                    if (_ambidextrous)
                    {
                        if (e.Grabber.Side != _controller3DModel.HandSide)
                        {
                            _controller3DModel.SwitchHandedness();
                        }
                    }

                    // Assign hand

                    _controller3DModel.ControllerHand = controllerHand;
                }

                // Initialize the hand using a coroutine so that it is performed the next frame.
                StartCoroutine(InitializeHandCoroutine(e.Grabber, controllerHand));

                controllerHand.enabled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            OnControllerReleaseOrPlace(e);
        }

        /// <inheritdoc />
        protected override void OnObjectPlaced(UxrManipulationEventArgs e)
        {
            OnControllerReleaseOrPlace(e);
        }

        /// <summary>
        ///     Called when the user releases or places this controller. We remove the connection
        ///     with the controller and the hand that grabbed it, and unsubscribe from events.
        /// </summary>
        /// <param name="e">Grab parameters</param>
        private void OnControllerReleaseOrPlace(UxrManipulationEventArgs e)
        {
            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;

            UxrControllerHand controllerHand = GetControllerHand(e.Grabber);

            if (controllerHand)
            {
                controllerHand.enabled = false;

                if (_controller3DModel.NeedsBothHands)
                {
                    if (e.Grabber.Side == UxrHandSide.Left)
                    {
                        _controller3DModel.ControllerHandLeft = null;
                    }
                    else
                    {
                        _controller3DModel.ControllerHandRight = null;
                    }
                }
                else
                {
                    _controller3DModel.ControllerHand = null;
                }

                // Restore blocked input

                UxrControllerInput.SetIgnoreControllerInput(e.Grabber.Side, e.Grabber.Side == UxrHandSide.Left ? _ignoreInputLeft : _ignoreInputRight);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the UxrControllerHand component that belongs to the grabber passed. This basically
        ///     just checks if it is the left or right one and looks for the UxrControllerHand component
        ///     in the hand bone object.
        /// </summary>
        /// <param name="grabber">The grabber we want to get the UxrControllerHand for</param>
        /// <returns>UxrControllerHand or null if none was found</returns>
        private UxrControllerHand GetControllerHand(UxrGrabber grabber)
        {
            if (grabber && grabber.Side == UxrHandSide.Left && grabber.Avatar != null)
            {
                return grabber.Avatar.LeftHandBone.GetComponent<UxrControllerHand>();
            }
            if (grabber && grabber.Side == UxrHandSide.Right && grabber.Avatar != null)
            {
                return grabber.Avatar.RightHandBone.GetComponent<UxrControllerHand>();
            }

            return null;
        }

        #endregion

        #region Private Types & Data

        private UxrController3DModel _controller3DModel;
        private bool                 _ignoreInputLeft;
        private bool                 _ignoreInputRight;

        #endregion
    }
}