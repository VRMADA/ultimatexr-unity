// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrButton3D.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     Base class to simplify interacting with 3D button objects by programming 2D UI elements.
    ///     A 2D Unity UI Canvas is placed on top of the 3D buttons. The Canvas will contain invisible
    ///     <see cref="UxrControlInput" /> UI components by using <see cref="UxrNonDrawingGraphic" /> instead of images.
    ///     The <see cref="UxrControlInput" /> components will get the user input and through child implementations of
    ///     <see cref="UxrButton3D" /> the 3D objects will be "pushed", "rotated" creating 3D behaviour using 2D logic.
    /// </summary>
    [RequireComponent(typeof(UxrControlInput))]
    public class UxrButton3D : UxrComponent<Canvas, UxrButton3D>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform _targetTransform;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the UI input component.
        /// </summary>
        public UxrControlInput ControlInput => GetCachedComponent<UxrControlInput>();

        /// <summary>
        ///     Gets the <see cref="Transform" /> of the 3D object that is going to move, rotate, scale...
        /// </summary>
        public Transform Target => _targetTransform;

        /// <summary>
        ///     Gets <see cref="Target" />'s local position during Awake().
        /// </summary>
        public Vector3 InitialTargetLocalPosition { get; private set; }

        /// <summary>
        ///     Gets <see cref="Target" />'s local rotation during Awake().
        /// </summary>
        public Quaternion InitialTargetLocalRotation { get; private set; }

        /// <summary>
        ///     Gets <see cref="Target" />'s world position during Awake().
        /// </summary>
        public Vector3 InitialTargetPosition { get; private set; }

        /// <summary>
        ///     Gets <see cref="Target" />'s world rotation during Awake().
        /// </summary>
        public Quaternion InitialTargetRotation { get; private set; }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Target != null)
            {
                InitialTargetLocalPosition = Target.localPosition;
                InitialTargetLocalRotation = Target.localRotation;
                InitialTargetPosition      = Target.position;
                InitialTargetRotation      = Target.rotation;
            }
        }

        /// <summary>
        ///     Subscribes to the input control events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (ControlInput)
            {
                ControlInput.Pressed  += ControlInput_Pressed;
                ControlInput.Released += ControlInput_Released;
            }
        }

        /// <summary>
        ///     Unsubscribes from the input control events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (ControlInput)
            {
                ControlInput.Pressed  -= ControlInput_Pressed;
                ControlInput.Released -= ControlInput_Released;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Receives the key down event.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        private void ControlInput_Pressed(UxrControlInput controlInput, PointerEventData eventData)
        {
            OnKeyPressed(controlInput, eventData);
        }

        /// <summary>
        ///     Receives the key up event.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        private void ControlInput_Released(UxrControlInput controlInput, PointerEventData eventData)
        {
            OnKeyReleased(controlInput, eventData);
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the key pressed event. It can be overridden in child classes to handle key presses without
        ///     subscribing to events.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        protected virtual void OnKeyPressed(UxrControlInput controlInput, PointerEventData eventData)
        {
        }

        /// <summary>
        ///     Event trigger for the key released event. It can be overridden in child classes to handle key releases without
        ///     subscribing to events.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        protected virtual void OnKeyReleased(UxrControlInput controlInput, PointerEventData eventData)
        {
        }

        #endregion
    }
}