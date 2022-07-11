// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStandardAvatarController.HandInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices;

namespace UltimateXR.Avatar.Controllers
{
    public sealed partial class UxrStandardAvatarController
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores relevant information of a hand required by the <see cref="UxrStandardAvatarController" /> at runtime.
        /// </summary>
        private class HandInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets or sets the index of the grab animation event. That is, the event whose animation is
            ///     <see cref="UxrAnimationType.LeftHandGrab" />/ <see cref="UxrAnimationType.RightHandGrab" />.
            /// </summary>
            public int GrabEventIndex { get; set; }

            /// <summary>
            ///     Gets or sets the hand pose name of <see cref="GrabEventIndex" /> at the beginning.
            /// </summary>
            public string InitialHandGrabPoseName { get; set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrAvatarControllerEvent.Buttons" /> required to activate the
            ///     <see cref="GrabEventIndex" /> at the beginning.
            /// </summary>
            public UxrInputButtons InitialHandGrabButtons { get; set; }

            /// <summary>
            ///     Gets or sets whether the hand is currently grabbing.
            /// </summary>
            public bool IsGrabbing { get; set; }

            /// <summary>
            ///     Gets or sets whether the hand is currently pointing.
            /// </summary>
            public bool IsPointing { get; set; }

            /// <summary>
            ///     Gets or sets whether the hand has currently a finger tip inside a <see cref="IsInsideFingerPointingVolume" />.
            /// </summary>
            public bool IsInsideFingerPointingVolume { get; set; }

            /// <summary>
            ///     Gets or sets whether the hand was grabbing last frame.
            /// </summary>
            public bool WasGrabbingLastFrame { get; set; }

            /// <summary>
            ///     Gets or sets whether the hand was pointing last frame.
            /// </summary>
            public bool WasPointingLastFrame { get; set; }

            /// <summary>
            ///     Gets or sets whether the hand should be let grab again. Used to control grab/release.
            /// </summary>
            public bool LetGrabAgain { get; set; }

            /// <summary>
            ///     Gets or sets the value between range [0.0, 1.0] that controls the grab pose blending.
            /// </summary>
            public float GrabBlendValue { get; set; }

            #endregion
        }

        #endregion
    }
}