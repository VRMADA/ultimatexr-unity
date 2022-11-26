// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHoverTimerClick.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" /> with a <see cref="UxrControlInput" /> component, will
    ///     automatically generate a Click event on the control whenever the cursor spends a given amount of time over it.
    ///     It can be used to implement clicks using the gaze pointer (<see cref="UxrCameraPointer" />).
    /// </summary>
    [RequireComponent(typeof(UxrControlInput))]
    public class UxrHoverTimerClick : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] [Tooltip("Number of seconds the user will need to hover over this element to trigger the Click event")]                                                      private float _lookAtSecondsToClick = 2.0f;
        [SerializeField] [Tooltip("Unscaled time will use the real device timer. If this parameter is unchecked it will use the scaled timer affected by pauses, bullet-times etc.")] private bool  _useUnscaledTime      = true;
        [SerializeField] [Tooltip("Will update the fill value of the Image component on this same GameObject to represent the timer progress. Needs an Image component.")]            private bool  _useFillImage         = true;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _image        = GetComponent<Image>();
            _controlInput = GetComponent<UxrControlInput>();

            _timer = _lookAtSecondsToClick;

            if (_useFillImage && _image == null)
            {
                Debug.LogWarning($"UseFillImage was specified on {GetType().Name} component of GameObject {name} but there is no Image component on it to update fill.");
            }
            else if (_useFillImage && _image != null && _image.type != Image.Type.Filled)
            {
                Debug.LogWarning($"UseFillImage was specified on {GetType().Name} component of GameObject {name} but the Image component is not of a filled type (Image Type property).");
            }
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _controlInput.CursorEntered += Input_Entered;
            _controlInput.CursorExited  += Input_Exited;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            _controlInput.CursorEntered -= Input_Entered;
            _controlInput.CursorExited  -= Input_Exited;
        }

        /// <summary>
        ///     Updates the progress and checks whether to generate the Click event.
        /// </summary>
        private void Update()
        {
            if (_timer > 0.0f)
            {
                _timer -= _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                if (_timer <= 0.0f)
                {
                    _timer = -1.0f;

                    if (_useFillImage && _image)
                    {
                        _image.fillAmount = 1.0f;
                    }

                    ExecuteEvents.ExecuteHierarchy(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                }
                else
                {
                    if (_useFillImage && _image)
                    {
                        _image.fillAmount = Mathf.Clamp01(1.0f - _timer / _lookAtSecondsToClick);
                    }
                }
            }
            else
            {
                if (_useFillImage && _image)
                {
                    _image.fillAmount = 0.0f;
                }
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever the pointer entered the control's Rect.
        /// </summary>
        /// <param name="controlInput">Control input</param>
        /// <param name="eventData">Event data</param>
        private void Input_Entered(UxrControlInput controlInput, PointerEventData eventData)
        {
            _timer = _lookAtSecondsToClick;
        }

        /// <summary>
        ///     Called whenever the pointer exited the control's Rect.
        /// </summary>
        /// <param name="controlInput">Control input</param>
        /// <param name="eventData">Event data</param>
        private void Input_Exited(UxrControlInput controlInput, PointerEventData eventData)
        {
            _timer = -1.0f;
        }

        #endregion

        #region Private Types & Data

        private Image           _image;
        private UxrControlInput _controlInput;
        private float           _timer;

        #endregion
    }
}