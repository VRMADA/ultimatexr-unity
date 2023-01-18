// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPointerEventData.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Event data class that adds information required by <see cref="UxrPointerInputModule" /> to facilitate the
    ///     processing of UI interaction events.
    /// </summary>
    public class UxrPointerEventData : PointerEventData
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the event data contains valid information.
        /// </summary>
        public bool HasData => pointerCurrentRaycast.isValid || IgnoredGameObject != null || IsNonUI;
        
        /// <summary>
        ///     Gets the <see cref="UxrAvatar" /> responsible for the interaction.
        /// </summary>
        public UxrAvatar Avatar { get; }

        /// <summary>
        ///     Gets the hand responsible for the interaction.
        /// </summary>
        public UxrHandSide HandSide { get; }

        /// <summary>
        ///     Gets the finger tip if this event is being processed by one. Null if not.
        /// </summary>
        public UxrFingerTip FingerTip { get; }

        /// <summary>
        ///     Gets the laser pointer if this event is being processed by one. Null if not.
        /// </summary>
        public UxrLaserPointer LaserPointer { get; }

        /// <summary>
        ///     Gets the current pointer world position.
        /// </summary>
        public Vector3 WorldPos { get; internal set; }

        /// <summary>
        ///     Gets the pointer world position during the last frame.
        /// </summary>
        public Vector3 PreviousWorldPos { get; internal set; }

        /// <summary>
        ///     Gets whether the world position has been initialized.
        /// </summary>
        public bool WorldPosInitialized { get; internal set; }

        /// <summary>
        ///     Gets whether the pointer is pressing this frame.
        /// </summary>
        public bool PressedThisFrame { get; internal set; }

        /// <summary>
        ///     Gets whether the pointer is pressing this frame.
        /// </summary>
        public bool ReleasedThisFrame { get; internal set; }

        /// <summary>
        ///     Gets whether the current raycast UI element is interactive.
        /// </summary>
        public bool IsInteractive { get; internal set; }

        /// <summary>
        ///     Gets whether the current raycast UI element is not a UI GameObject.
        ///     This happens when the raycast is valid and the object has either a 2D or 3D collider. 
        /// </summary>
        public bool IsNonUI => GameObject2D != null || GameObject3D != null;

        /// <summary>
        ///     Gets the UI gameObject that was ignored because it could not be interacted with.
        /// </summary>
        public GameObject IgnoredGameObject { get; internal set; }

        /// <summary>
        ///     Gets the gameObject if the raycast element is an object with a 3D collider.
        /// </summary>
        public GameObject GameObject3D { get; internal set; }

        /// <summary>
        ///     Gets the gameObject if the raycast element is an object with a 2D collider.
        /// </summary>
        public GameObject GameObject2D { get; internal set; }

        /// <summary>
        ///     Gets the <see cref="GameObject" /> that was clicked, if there was one.
        /// </summary>
        public GameObject GameObjectClicked { get; internal set; }

        /// <summary>
        ///     Gets the current cursor speed.
        /// </summary>
        public float Speed { get; internal set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="eventSystem">Event system</param>
        /// <param name="fingerTip">Finger tip responsible for the event</param>
        public UxrPointerEventData(EventSystem eventSystem, UxrFingerTip fingerTip) : base(eventSystem)
        {
            Avatar    = fingerTip.Avatar;
            HandSide  = fingerTip.Side;
            FingerTip = fingerTip;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="eventSystem">Event system</param>
        /// <param name="laserPointer">Laser pointer responsible for the event</param>
        public UxrPointerEventData(EventSystem eventSystem, UxrLaserPointer laserPointer) : base(eventSystem)
        {
            Avatar       = laserPointer.Avatar;
            HandSide     = laserPointer.HandSide;
            LaserPointer = laserPointer;
        }

        #endregion
    }
}