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
        ///     Gets the <see cref="UxrAvatar" /> responsible for the interaction.
        /// </summary>
        public UxrAvatar Avatar { get; }

        /// <summary>
        ///     Gets the hand responsible for the interaction.
        /// </summary>
        public UxrHandSide HandSide { get; }

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
        /// <param name="avatar">Avatar responsible for the event</param>
        /// <param name="handSide">Hand responsible for the event</param>
        public UxrPointerEventData(EventSystem eventSystem, UxrAvatar avatar, UxrHandSide handSide) : base(eventSystem)
        {
            Avatar   = avatar;
            HandSide = handSide;
        }

        #endregion
    }
}