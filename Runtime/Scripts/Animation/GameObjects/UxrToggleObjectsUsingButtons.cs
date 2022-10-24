// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleObjectsUsingButtons.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Devices;
using UnityEngine;

namespace UltimateXR.Animation.GameObjects
{
    /// <summary>
    ///     Component that allows to enable/disable GameObjects based on input from the VR controller buttons.
    /// </summary>
    public class UxrToggleObjectsUsingButtons : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private List<GameObject>   _objectList;
        [SerializeField] private bool               _startEnabled         = true;
        [SerializeField] private UxrHandSide        _controllerHand       = UxrHandSide.Left;
        [SerializeField] private UxrInputButtons    _buttonsEnable        = UxrInputButtons.Button1;
        [SerializeField] private UxrButtonEventType _buttonsEventEnable   = UxrButtonEventType.PressDown;
        [SerializeField] private UxrInputButtons    _buttonsDisable       = UxrInputButtons.Button1;
        [SerializeField] private UxrButtonEventType _buttonsEventsDisable = UxrButtonEventType.TouchDown;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the object list to enable/disable.
        /// </summary>
        public List<GameObject> ObjectList
        {
            get => _objectList;
            set => _objectList = value;
        }

        /// <summary>
        ///     Gets or sets which controller hand is responsible for the input.
        /// </summary>
        public UxrHandSide ControllerHand
        {
            get => _controllerHand;
            set => _controllerHand = value;
        }

        /// <summary>
        ///     Gets or sets the button(s) used to enable.
        /// </summary>
        public UxrInputButtons ButtonsToEnable
        {
            get => _buttonsEnable;
            set => _buttonsEnable = value;
        }

        /// <summary>
        ///     Gets or sets the button event to enable.
        /// </summary>
        public UxrButtonEventType EnableButtonEvent
        {
            get => _buttonsEventEnable;
            set => _buttonsEventEnable = value;
        }

        /// <summary>
        ///     Gets or sets the button(s) to disable.
        /// </summary>
        public UxrInputButtons ButtonsToDisable
        {
            get => _buttonsDisable;
            set => _buttonsDisable = value;
        }

        /// <summary>
        ///     Gets or sets the button event to disable.
        /// </summary>
        public UxrButtonEventType DisableButtonEvent
        {
            get => _buttonsEventsDisable;
            set => _buttonsEventsDisable = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Called at the beginning. Sets the object initial state.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _state = _startEnabled;
            SetObjectsState(_startEnabled);
        }

        /// <summary>
        ///     Called each frame. Checks for VR controller button events and toggles states.
        /// </summary>
        private void Update()
        {
            if (!UxrAvatar.LocalAvatar)
            {
                return;
            }

            if (_state == false && UxrAvatar.LocalAvatarInput.GetButtonsEvent(ControllerHand, ButtonsToEnable, EnableButtonEvent))
            {
                SetObjectsState(true);
            }
            else if (_state && UxrAvatar.LocalAvatarInput.GetButtonsEvent(ControllerHand, ButtonsToDisable, DisableButtonEvent))
            {
                SetObjectsState(false);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sets the object state of the list of objects in this component.
        /// </summary>
        /// <param name="value">State they should be changed to</param>
        private void SetObjectsState(bool value)
        {
            _state = value;

            foreach (GameObject obj in ObjectList)
            {
                obj.SetActive(value);
            }
        }

        #endregion

        #region Private Types & Data

        private bool _state;

        #endregion
    }
}